namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories;
    using Microsoft.Teams.Apps.QBot.Infrastructure.QnAService;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;
    using ErrorCode = Microsoft.Teams.Apps.QBot.Domain.Errors.ErrorCode;

    /// <summary>
    /// Teams message service implementation.
    /// </summary>
    internal sealed class TeamsMessageService : ITeamsMessageService
    {
        /// <summary>
        /// Conversation Id format for conversation thread in a channel.
        /// {0} channel Id.
        /// {1} Root message Id.
        /// </summary>
        private const string ConversationIdFormat = "{0};messageid={1}";

        private const string QuestionAnsweredActivityType = "questionAnswered";
        private const string AnswerAcceptedActivityType = "answerAccepted";
        private const string SetupCourseActivityType = "setupCourse";
        private const int MaxRetry = 3;

        private readonly IAppSettings appSettings;
        private readonly IGraphServiceClient graphServiceClient;
        private readonly BotFrameworkHttpAdapter botAdaper;
        private readonly IMessageFactory messageFactory;
        private readonly IAppSettingsRepository repository;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IDeepLinkCreator deepLinkCreator;
        private readonly ILogger<TeamsMessageService> logger;
        private readonly AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMessageService"/> class.
        /// </summary>
        /// <param name="appSettings">App Settings.</param>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <param name="botAdaper">Bot Adapter.</param>
        /// <param name="messageFactory">Message factory.</param>
        /// <param name="repository">App settings repository.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="deepLinkCreator">Deep link creator.</param>
        /// <param name="logger">Logger.</param>
        public TeamsMessageService(
            IAppSettings appSettings,
            GraphServiceClient graphServiceClient,
            BotFrameworkHttpAdapter botAdaper,
            IMessageFactory messageFactory,
            IAppSettingsRepository repository,
            IStringLocalizer<Strings> localizer,
            IDeepLinkCreator deepLinkCreator,
            ILogger<TeamsMessageService> logger)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            this.botAdaper = botAdaper ?? throw new ArgumentNullException(nameof(botAdaper));
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.deepLinkCreator = deepLinkCreator ?? throw new ArgumentNullException(nameof(deepLinkCreator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.retryPolicy = GetRetryPolicy(MaxRetry, this.logger);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetQuestionsMessageAsync(string teamId, string channelId, IEnumerable<string> questionsMessageIds)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                throw new ArgumentException($"'{nameof(teamId)}' cannot be null or empty.", nameof(teamId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (questionsMessageIds is null)
            {
                throw new ArgumentNullException(nameof(questionsMessageIds));
            }

            var response = new Dictionary<string, string>();
            var messageIds = questionsMessageIds.ToArray();
            int index = 0;
            while (index < messageIds.Length)
            {
                // Prepare batch request content. (max 20 requests).
                var batchRequestContent = new BatchRequestContent();
                var requestIdMap = new Dictionary<string, string>();
                for (int j = 0; j < 20 && index < messageIds.Length; j++, index++)
                {
                    var messageId = messageIds[index];
                    var request = this.graphServiceClient
                        .Teams[teamId]
                        .Channels[channelId]
                        .Messages[messageId]
                        .Request()
                        .WithAppOnly()
                        .WithMaxRetry(MaxRetry)
                        .GetHttpRequestMessage();

                    request.Method = HttpMethod.Get;
                    var requestId = batchRequestContent.AddBatchRequestStep(request);
                    requestIdMap.Add(messageId, requestId);
                }

                BatchResponseContent result;
                try
                {
                    result = await this.graphServiceClient
                        .Batch
                        .Request()
                        .WithAppOnly()
                        .WithMaxRetry(MaxRetry)
                        .PostAsync(batchRequestContent);
                }
                catch (ServiceException exception)
                {
                    this.logger.LogWarning(exception, $"Failed to send batch request to fetch messages. StatusCode {exception.StatusCode}");
                    throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Failed to send batch request to fetch messages.", exception);
                }

                foreach (var messageId in requestIdMap.Keys)
                {
                    try
                    {
                        var message = await result.GetResponseByIdAsync<ChatMessage>(requestIdMap.GetValueOrDefault(messageId));
                        response.Add(messageId, GetQuestionMessage(message));
                    }
                    catch (ServiceException exception)
                    {
                        // No permission granted.
                        if (exception.StatusCode == HttpStatusCode.Forbidden)
                        {
                            this.logger.LogWarning(exception, "Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.");
                            throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, $"{nameof(this.GetQuestionsMessageAsync)}: Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.", exception);
                        }

                        // Resource not found
                        if (exception.StatusCode == HttpStatusCode.NotFound)
                        {
                            // Message not found.
                            this.logger.LogWarning(exception, $"Message not found. Message Id: {messageId}");
                            response.Add(messageId, null);
                        }

                        this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch message: {messageId}");
                        throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
                    }
                }
            }

            return response;
        }

        /// <inheritdoc/>
        public async Task<string> GetQuestionMessageAsync(string teamId, string channelId, string questionMessageId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                throw new ArgumentException($"'{nameof(teamId)}' cannot be null or empty.", nameof(teamId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(questionMessageId))
            {
                throw new ArgumentException($"'{nameof(questionMessageId)}' cannot be null or empty.", nameof(questionMessageId));
            }

            try
            {
                var chatMessage = await this.graphServiceClient
                    .Teams[teamId]
                    .Channels[channelId]
                    .Messages[questionMessageId]
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                return GetQuestionMessage(chatMessage);
            }
            catch (ServiceException exception)
            {
                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, $"{nameof(this.GetQuestionsMessageAsync)}: Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.", exception);
                }

                // Resource not found
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(exception, $"Question Message not found. Message Id: {questionMessageId}");
                    throw new QBotException(HttpStatusCode.NotFound, ErrorCode.Unknown, $"Resource not found.", exception);
                }

                this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch message: {questionMessageId}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<QuestionResponse> GetQuestionResponseAsync(string teamId, string channelId, string questionMessageId, string responseMessageId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                throw new ArgumentException($"'{nameof(teamId)}' cannot be null or empty.", nameof(teamId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(questionMessageId))
            {
                throw new ArgumentException($"'{nameof(questionMessageId)}' cannot be null or empty.", nameof(questionMessageId));
            }

            if (string.IsNullOrEmpty(responseMessageId))
            {
                throw new ArgumentException($"'{nameof(responseMessageId)}' cannot be null or empty.", nameof(responseMessageId));
            }

            try
            {
                var message = await this.graphServiceClient
                    .Teams[teamId]
                    .Channels[channelId]
                    .Messages[questionMessageId]
                    .Replies[responseMessageId]
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                return PrepareQuestionResponse(message, questionMessageId);
            }
            catch (ServiceException exception)
            {
                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, $"{nameof(this.GetQuestionsMessageAsync)}: Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.", exception);
                }

                // Resource not found
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(exception, $"Question Response Message not found. Message Id: {responseMessageId}");
                    throw new QBotException(HttpStatusCode.NotFound, ErrorCode.Unknown, $"Resource not found.", exception);
                }

                this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch response message: {responseMessageId}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuestionResponse>> GetQuestionResponsesAsync(string teamId, string channelId, string messageId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                throw new ArgumentException($"'{nameof(teamId)}' cannot be null or empty.", nameof(teamId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException($"'{nameof(messageId)}' cannot be null or empty.", nameof(messageId));
            }

            try
            {
                var messages = await this.graphServiceClient
                    .Teams[teamId]
                    .Channels[channelId]
                    .Messages[messageId]
                    .Replies
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                var responses = messages
                    .Where(m => m.From.User != null) // Filter bot response.
                    .Select(m => PrepareQuestionResponse(m, messageId)).ToList();

                return responses;
            }
            catch (ServiceException exception)
            {
                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Make sure the app has permissions to read channel messages.", exception);
                }

                // Resource not found
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(exception, $"Question message not found. Message Id: {messageId}");
                    throw new QBotException(HttpStatusCode.NotFound, ErrorCode.Unknown, $"Resource not found.", exception);
                }

                this.logger.LogWarning(exception, $"Something went wrong. Failed to get responses for question: {messageId}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<string> PostCorrectAnswerAsync(Question question, Answer answer, IEnumerable<QBot.Domain.Models.User> users)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (answer is null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            if (users is null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            var conversationId = this.GetConversationId(question.ChannelId, question.MessageId);
            var attachment = this.messageFactory.CreateAnswerMessage(question, answer, users);
            var messageActivity = MessageFactory.Attachment(attachment);
            messageActivity.Summary = answer.Message;
            if (!string.IsNullOrEmpty(answer.MessageId))
            {
                messageActivity.Id = answer.MessageId;
            }

            return await this.PostMessageAsync(conversationId, messageActivity);
        }

        /// <inheritdoc/>
        public async Task<string> PostSelectAnswerAsync(Question question, IEnumerable<QBot.Domain.Models.User> users)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (users is null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            var conversationId = this.GetConversationId(question.ChannelId, question.MessageId);
            var attachment = this.messageFactory.CreateSelectAnswerMessage(question, users);
            var messageActivity = MessageFactory.Attachment(attachment);
            messageActivity.Summary = this.localizer.GetString("selectAnswer");
            return await this.PostMessageAsync(conversationId, messageActivity);
        }

        /// <inheritdoc/>
        public async Task<string> PostSuggestAnswerAsync(Question question, SuggestedAnswer suggestedAnswer)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (suggestedAnswer is null)
            {
                throw new ArgumentNullException(nameof(suggestedAnswer));
            }

            var conversationId = this.GetConversationId(question.ChannelId, question.MessageId);
            var attachment = this.messageFactory.CreateSuggestAnswerMessage(question, suggestedAnswer);
            var messageActivity = MessageFactory.Attachment(attachment);
            messageActivity.Summary = suggestedAnswer.Answer;
            return await this.PostMessageAsync(conversationId, messageActivity);
        }

        /// <inheritdoc/>
        public async Task<string> UpdateSuggestedAnswerAsync(Question question, Answer answer, QBot.Domain.Models.User user)
        {
            var conversationId = this.GetConversationId(answer.ChannelId, question.MessageId);
            var attachment = this.messageFactory.CreateNotHelpfulMessage(answer, user);
            var messageActivity = MessageFactory.Attachment(attachment);
            messageActivity.Id = answer.MessageId;
            messageActivity.Summary = answer.Message;
            return await this.PostMessageAsync(conversationId, messageActivity);
        }

        /// <inheritdoc/>
        public async Task NotifyQuestionAnsweredAsync(Course course, QBot.Domain.Models.Channel channel, Question question, Answer answer)
        {
            var topic = new TeamworkActivityTopic
            {
                Source = TeamworkActivityTopicSource.Text,
                Value = $"{course.Name} > {channel.Name}",
                WebUrl = this.deepLinkCreator.GetTeamsMessageDeepLink(course.TeamId, channel.Id, question.MessageId, answer.MessageId),
            };

            var previewText = new ItemBody
            {
                Content = question.GetSanitizedMessage(),
            };

            var recipient = new AadUserNotificationRecipient
            {
                UserId = question.AuthorId,
            };

            await this.SendActivityFeedNotificationAsync(course.TeamAadObjectId, topic, QuestionAnsweredActivityType, previewText, recipient);
        }

        /// <inheritdoc/>
        public async Task NotifyAnsweredAcceptedAsync(Course course, QBot.Domain.Models.Channel channel, Question question, Answer answer)
        {
            var topic = new TeamworkActivityTopic
            {
                Source = TeamworkActivityTopicSource.Text,
                Value = $"{course.Name} > {channel.Name}",
                WebUrl = this.deepLinkCreator.GetTeamsMessageDeepLink(course.TeamId, channel.Id, question.MessageId, answer.MessageId),
            };

            var previewText = new ItemBody
            {
                Content = answer.GetSanitizedMessage(),
            };

            var recipient = new AadUserNotificationRecipient
            {
                UserId = answer.AuthorId,
            };

            await this.SendActivityFeedNotificationAsync(course.TeamAadObjectId, topic, AnswerAcceptedActivityType, previewText, recipient);
        }

        /// <inheritdoc/>
        public async Task NotifyMemberToSetupCourseAsync(Course course, Member member)
        {
            var topic = new TeamworkActivityTopic
            {
                Source = TeamworkActivityTopicSource.Text,
                Value = $"{course.Name}",
                WebUrl = this.deepLinkCreator.GetPersonalTabConfigureCourseDeepLink(this.appSettings.ManifestAppId, "personalTab", course.TeamAadObjectId),
            };

            var previewText = new ItemBody
            {
                Content = this.localizer.GetString("setupCoursePreviewText"),
            };

            var recipient = new AadUserNotificationRecipient
            {
                UserId = member.AadId,
            };

            await this.SendActivityFeedNotificationAsync(course.TeamAadObjectId, topic, SetupCourseActivityType, previewText, recipient);
        }

        /// <inheritdoc/>
        public async Task<string> UpdateSelectAnswerMessageAsync(Course course, QBot.Domain.Models.Channel channel, Question question, Answer answer)
        {
            if (string.IsNullOrEmpty(question?.InitialResponseMessageId))
            {
                this.logger.LogWarning("UpdateSelectAnswerMessageAsync:: Skip update select answer as 'InitialResponseMessageId' is not set.");
                return string.Empty;
            }

            var conversationId = this.GetConversationId(answer.ChannelId, question.MessageId);
            var deepLink = this.deepLinkCreator.GetTeamsMessageDeepLink(course.TeamId, channel.Id, question.MessageId, answer.MessageId);
            var attachment = this.messageFactory.CreateQuestionAnsweredMessage(deepLink);
            var messageActivity = MessageFactory.Attachment(attachment);
            messageActivity.Id = question.InitialResponseMessageId;
            messageActivity.Summary = this.localizer.GetString("questionIsAnsweredMessage");
            return await this.PostMessageAsync(conversationId, messageActivity);
        }

        private static AsyncRetryPolicy GetRetryPolicy(int maxAttempts, ILogger log)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: maxAttempts);
            return Policy.Handle<ErrorResponseException>(e =>
            {
                var errorMessage = $"{e.GetType()}: {e.Message}";
                log.LogWarning(e, $"Exception thrown: {errorMessage}");

                // Handle throttling and internal server errors.
                var statusCode = e.Response.StatusCode;
                return statusCode == HttpStatusCode.TooManyRequests || ((int)statusCode >= 500 && (int)statusCode < 600);
            })
                .WaitAndRetryAsync(delay);
        }

        private static string GetQuestionMessage(ChatMessage message)
        {
            return message.Body.Content;
        }

        private static QuestionResponse PrepareQuestionResponse(ChatMessage message, string questionMessageId)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(questionMessageId))
            {
                throw new ArgumentException($"'{nameof(questionMessageId)}' cannot be null or empty.", nameof(questionMessageId));
            }

            // This is raw content in html.
            var messageContent = message.Attachments.FirstOrDefault() == null ? message.Body.Content : message.Attachments.FirstOrDefault().Content;
            return new QuestionResponse()
            {
                Id = message.Id,
                AuthorId = message.From.User?.Id ?? message.From.Application.Id, // either user or application.
                Message = messageContent,
                MessageId = message.Id,
                QuestionId = questionMessageId,
                TimeStamp = message.CreatedDateTime.Value,
            };
        }

        private async Task<string> PostMessageAsync(string conversationId, IMessageActivity messageActivity)
        {
            var messageId = string.Empty;
            var serviceUrl = await this.repository.GetServiceUrlAsync();
            await this.botAdaper.ContinueConversationAsync(
                botAppId: this.appSettings.BotAppId,
                reference: new ConversationReference()
                {
                    ServiceUrl = serviceUrl,
                    Conversation = new ConversationAccount()
                    {
                        Id = conversationId,
                    },
                },
                callback: async (turnContext, cancellationToken) =>
                {
                    try
                    {
                        // Send message.
                        await this.retryPolicy.ExecuteAsync(async () =>
                        {
                            if (string.IsNullOrEmpty(messageActivity.Id))
                            {
                                // Post new message.
                                var response = await turnContext.SendActivityAsync(messageActivity);
                                messageId = response.Id;
                            }
                            else
                            {
                                // Update existing message.
                                var response = await turnContext.UpdateActivityAsync(messageActivity);
                            }
                        });
                    }
                    catch (ErrorResponseException exception)
                    {
                        var errorMessage = $"Failed to send message. Conversation id: {conversationId}, Message id: {messageActivity.Id}.";
                        this.logger.LogWarning(exception, errorMessage);
                        throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, errorMessage);
                    }
                },
                cancellationToken: CancellationToken.None);
            return messageId;
        }

        private string GetConversationId(string channelId, string questionMessageId)
        {
            return string.Format(CultureInfo.InvariantCulture, ConversationIdFormat, channelId, questionMessageId);
        }

        private async Task SendActivityFeedNotificationAsync(string teamId, TeamworkActivityTopic topic, string activityType, ItemBody previewText, AadUserNotificationRecipient recipient)
        {
            try
            {
                await this.graphServiceClient
                    .Teams[teamId]
                    .SendActivityNotification(topic, activityType, null/*chainId*/, previewText, null, recipient)
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .PostAsync();
            }
            catch (ServiceException exception)
            {
                this.logger.LogWarning(exception, "Failed to send activity feed notification.");

                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    var message = "Make sure the app has permission (TeamsActivity.Send) to send activity feed notifications.";
                    this.logger.LogWarning(exception, message);
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, message, exception);
                }

                this.logger.LogWarning(exception, $"Something went wrong.");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
            }
        }
    }
}
