namespace Microsoft.Teams.Apps.Selfhelp.Helper
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.AdaptiveCard;
    using Microsoft.Teams.Selfhelp.Authentication.Model.Configuration;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// Teams message service.
    /// </summary>
    public class MessageService : IMessageService
    {
        /// <summary>
        /// It represents the microsoft application id.
        /// </summary>
        private readonly string microsoftAppId;

        /// <summary>
        /// A bot adapter details for proactive message in conversation.
        /// </summary>
        private readonly BotFrameworkHttpAdapter botAdapter;

        /// <summary>
        /// Holds the instance of memory cache which will be used to store and retrieve adaptive card payload.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Instance to application configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance to send logs to the application insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance learning repository details.
        /// </summary>
        private readonly IArticleRepository learningRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageService"/> class.
        /// </summary>
        /// <param name="botOptions">Entity represents bot options.</param>
        /// <param name="botAdapter">Entity represents bot adapter.</param>
        /// <param name="memoryCache">Entity represents memory chache.</param>
        /// <param name="learningRepository">Entity represents learning repository details.</param>
        /// <param name="configuration">Entity represents configuration instance.</param>
        public MessageService(
            IOptions<BotSettings> botOptions,
            BotFrameworkHttpAdapter botAdapter,
            IMemoryCache memoryCache,
            IArticleRepository learningRepository,
            IConfiguration configuration)
        {
            this.microsoftAppId = botOptions?.Value?.MicrosoftAppId ?? throw new ArgumentNullException(nameof(botOptions));
            this.botAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter));
            this.memoryCache = memoryCache;
            this.learningRepository = learningRepository;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Send the message response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="conversationId">Conversation id.</param>
        /// <param name="serviceUrl">Service url</param>
        /// <param name="maxAttempts">Maximum attempts.</param>
        /// <param name="log">Application insight logs.</param>
        /// <returns>Return the send message response.</returns>
        public async Task<SendMessageResponse> SendMessageAsync(
            IMessageActivity message,
            string conversationId,
            string serviceUrl,
            int maxAttempts, ILogger log)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentException($"'{nameof(conversationId)}' cannot be null or empty", nameof(conversationId));
            }

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new ArgumentException($"'{nameof(serviceUrl)}' cannot be null or empty", nameof(serviceUrl));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var conversationReference = new ConversationReference
            {
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount
                {
                    Id = conversationId,
                },
            };

            var response = new SendMessageResponse
            {
                TotalNumberOfSendThrottles = 0,
                AllSendStatusCodes = string.Empty,
            };

            await this.botAdapter.ContinueConversationAsync(
                botAppId: this.microsoftAppId,
                reference: conversationReference,
                callback: async (turnContext, cancellationToken) =>
                {
                    var policy = this.GetRetryPolicy(maxAttempts, log);
                    try
                    {
                        // Send message.
                        var activityResponse = await policy.ExecuteAsync(async () => await turnContext.SendActivityAsync(message));

                        // Success.
                        response.ActivityId = activityResponse.Id;
                        response.ResultType = SendMessageResult.Succeeded;
                        response.StatusCode = (int)HttpStatusCode.Created;
                        response.AllSendStatusCodes += $"{(int)HttpStatusCode.Created},";
                    }
                    catch (ErrorResponseException e)
                    {
                        var errorMessage = $"{e.GetType()}: {e.Message}";
                        response.StatusCode = (int)e.Response.StatusCode;
                        response.AllSendStatusCodes += $"{(int)e.Response.StatusCode},";
                        response.ErrorMessage = e.Response.Content;
                        switch (e.Response.StatusCode)
                        {
                            case HttpStatusCode.TooManyRequests:
                                response.ResultType = SendMessageResult.Throttled;
                                response.TotalNumberOfSendThrottles = maxAttempts;
                                break;

                            case HttpStatusCode.NotFound:
                                response.ResultType = SendMessageResult.RecipientNotFound;
                                break;

                            default:
                                response.ResultType = SendMessageResult.Failed;
                                break;
                        }
                    }
                },
                cancellationToken: CancellationToken.None);

            return response;
        }

        /// <summary>
        /// Get the user adaptive card.
        /// </summary>
        /// <param name="cardMessage">A message on card.</param>
        /// <param name="learningId">Id of learning content.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="sharedByUser">Content shared by user.</param>
        /// <returns>Returns the user adaptive card.</returns>
        public async Task<Attachment> GetUserAdaptiveCard(string cardMessage, string learningId, string environmentCurrentDirectory, string sharedByUser)
        {
            try
            {
                var learningDetails = await this.learningRepository.GetLearningContentAsync(learningId);
                var baseUrl = this.configuration.GetValue<string>("App:AppBaseUri") + "/view-image-content?id=" + learningId;

                if (learningDetails.ItemType == ItemType.Video)
                {
                    string cardTime = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    string cardDate = DateTime.Now.ToString("MM/dd/yyyy");
                    cardDate = cardDate.Replace("-", "/");
                    var newTaskCardOptions = new LearningAdaptiveCard
                    {
                        CardTitle = learningDetails.Title,
                        ProfileUrl = string.Empty,
                        ProfileName = sharedByUser,
                        CardTime = cardTime,
                        CardDate = cardDate,
                        LearningContent = learningDetails.Description,
                        LearningContentUrl = learningDetails.TileImageLink == "" ? this.configuration.GetValue<string>("App:AppBaseUri") + "/images/Card2.png" : learningDetails.TileImageLink,
                        CardMessage = cardMessage,
                        LearningId = learningId,
                        ItemType = learningDetails.ItemType,
                    };

                    var cardPayload = this.GetCardPayload("_task-complete-card-member", "\\videoAdaptiveCard.json", environmentCurrentDirectory);
                    var adaptiveCardTemplate = new AdaptiveCardTemplate(cardPayload);
                    var cardJson = adaptiveCardTemplate.Expand(newTaskCardOptions);
                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = AdaptiveCard.FromJson(cardJson).Card,
                    };

                    return adaptiveCardAttachment;
                }
                else
                {
                    string cardTime = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    string cardDate1 = DateTime.Now.ToString("MM/dd/yyyy");
                    cardDate1 = cardDate1.Replace("-", "/");
                    var newTaskCardOptions1 = new LearningAdaptiveCard
                    {
                        CardTitle = learningDetails.Title,
                        ProfileUrl = string.Empty,
                        ProfileName = sharedByUser,
                        CardTime = cardTime,
                        CardDate = cardDate1,
                        LearningContent = learningDetails.Description,
                        LearningContentUrl = learningDetails.TileImageLink,
                        CardMessage = cardMessage,
                        LearningId = learningId,
                        ItemType = learningDetails.ItemType,
                        AppId = this.microsoftAppId,
                        BaseUrl = baseUrl,
                    };

                    var cardPayload = this.GetCardPayload("_task-complete-card-member-image", "\\imageAdaptiveCard.json", environmentCurrentDirectory);
                    var adaptiveCardTemplate = new AdaptiveCardTemplate(cardPayload);
                    var cardJson = adaptiveCardTemplate.Expand(newTaskCardOptions1);
                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = AdaptiveCard.FromJson(cardJson).Card,
                    };

                    return adaptiveCardAttachment;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Send notification card to user.
        /// </summary>
        /// <param name="title">Title of content.</param>
        /// <param name="articleCheckBoxEntities">Article checkbox entities details.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="clientId">Unique id of client.</param>
        /// <returns>Returns the Send notification card to user.</returns>
        public async Task<Attachment> SendNotificationtoUsersCard(string title, List<ArticleCheckBoxEntity> articleCheckBoxEntities, string environmentCurrentDirectory, string clientId)
        {
            var knomMoreLink = $"{Constant.Bot.TabBaseRedirectURL}/{clientId}/{Constant.Bot.HomeTabEntityId}";
            List<checkBoxEntity> checkBoxEntities = new List<checkBoxEntity>();
            foreach (var articleCheckBoxEntity in articleCheckBoxEntities)
            {
                checkBoxEntity checkBoxEntity = new checkBoxEntity();
                checkBoxEntity.Title = articleCheckBoxEntity.Title;
                if (articleCheckBoxEntity.TileImageLink != "link")
                {
                    if (articleCheckBoxEntity.TileImageLink != "")
                    {
                        checkBoxEntity.TileImageLink = articleCheckBoxEntity.TileImageLink;
                    }
                    else
                    {
                        checkBoxEntity.TileImageLink = this.configuration.GetValue<string>("App:AppBaseUri") + "/images/Card2.png";
                    }
                }
                else
                {
                    checkBoxEntity.TileImageLink = this.configuration.GetValue<string>("App:AppBaseUri") + "/images/Card2.png";
                }

                checkBoxEntity.ItemType = articleCheckBoxEntity.ItemType.ToString();
                checkBoxEntity.RowKey = articleCheckBoxEntity.RowKey;
                checkBoxEntities.Add(checkBoxEntity);
            }

            var newSendNotificationTaskCardsOptions = new SendNotificationCards
            {
                CardTitle = title,
                learningEntity = checkBoxEntities,
                Count = checkBoxEntities.Count(),
                KnowMoreLink = knomMoreLink,
            };
            var cardPayload = this.GetCardPayload("_Send-Notification-User-Cards", "\\notificationAdaptivecard.json", environmentCurrentDirectory);
            var adaptiveCardTemplate = new AdaptiveCardTemplate(cardPayload);
            var cardJson = adaptiveCardTemplate.Expand(newSendNotificationTaskCardsOptions);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(cardJson).Card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Send Welcome card to the user.
        /// </summary>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="clientId">Unique id of client.</param>
        /// <returns>Returns send welcome card to user.</returns>
        public async Task<Attachment> SendWelcomeCardToUser(string environmentCurrentDirectory, string clientId)
        {
            var knomMoreLink = $"{Constant.Bot.TabBaseRedirectURL}/{clientId}/{Constant.Bot.HomeTabEntityId}";
            var newSendWelcomeCardToUser = new
            {
                KnowMoreLink = knomMoreLink,
                imageLink = this.configuration.GetValue<string>("App:AppBaseUri") + "/images/Welcome.png",
            };
            var cardPayload = this.GetCardPayload("_Send-Welcome-Card-To-User", "\\welcomeAdaptiveCard.json", environmentCurrentDirectory);
            var adaptiveCardTemplate = new AdaptiveCardTemplate(cardPayload);
            var cardJson = adaptiveCardTemplate.Expand(newSendWelcomeCardToUser);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(cardJson).Card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get the card payload details.
        /// </summary>
        /// <param name="NewTaskCardCacheKey">New task card cache key.</param>
        /// <param name="jsonTemplateFileName">Json template file name.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <returns>Returns the card payload details.</returns>
        private string GetCardPayload(string NewTaskCardCacheKey, string jsonTemplateFileName, string environmentCurrentDirectory)
        {
            bool isCacheEntryExists = this.memoryCache.TryGetValue(NewTaskCardCacheKey, out string cardPayload);

            if (!isCacheEntryExists)
            {
                var jsonTemplateFilePath = Path.Combine(environmentCurrentDirectory, $"Cards\\{jsonTemplateFileName}");
                cardPayload = System.IO.File.ReadAllText(jsonTemplateFilePath);

                this.memoryCache.Set(NewTaskCardCacheKey, cardPayload, TimeSpan.FromHours(1));
            }

            return cardPayload;
        }

        /// <summary>
        /// Get retry policy details.
        /// </summary>
        /// <param name="maxAttempts">Maximum attempts count.</param>
        /// <param name="log">Application insight logs.</param>
        /// <returns>Returns the get retry policy.</returns>
        private AsyncRetryPolicy GetRetryPolicy(int maxAttempts, ILogger log)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: maxAttempts);
            return Policy
                .Handle<ErrorResponseException>(e =>
                {
                    var errorMessage = $"{e.GetType()}: {e.Message}";
                    log.LogError(e, $"Exception thrown: {errorMessage}");

                    // Handle throttling and internal server errors.
                    var statusCode = e.Response.StatusCode;
                    return statusCode == HttpStatusCode.TooManyRequests || ((int)statusCode >= 500 && (int)statusCode < 600);
                })
                .WaitAndRetryAsync(delay);
        }
    }
}