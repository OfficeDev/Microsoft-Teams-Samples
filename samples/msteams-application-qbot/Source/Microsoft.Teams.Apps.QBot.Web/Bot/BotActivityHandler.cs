// <copyright file="BotActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Domain.Questions;
    using Microsoft.Teams.Apps.QBot.Infrastructure;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Teams Bot Activity Handler.
    /// </summary>
    public sealed class BotActivityHandler : TeamsActivityHandler
    {
        private readonly IQBotTeamInfo qBotTeamInfo;
        private readonly ICourseReader courseReader;
        private readonly ICourseSetup courseSetup;
        private readonly IQBotService qBotService;
        private readonly IUrlProvider urlProvider;
        private readonly IMessageFactory messageFactory;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IAppSettings appSettings;
        private readonly ILogger<BotActivityHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotActivityHandler"/> class.
        /// </summary>
        /// <param name="qBotTeamInfo">QBot Team info.</param>
        /// <param name="courseReader">Course reader.</param>
        /// <param name="courseSetup">Couse setup.</param>
        /// <param name="qBotService">QBotService.</param>
        /// <param name="urlProvider">Url Provider.</param>
        /// <param name="messageFactory">Teams message factory.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="appSettings">App settings.</param>
        /// <param name="logger">Logger.</param>
        public BotActivityHandler(
            IQBotTeamInfo qBotTeamInfo,
            ICourseReader courseReader,
            ICourseSetup courseSetup,
            IQBotService qBotService,
            IUrlProvider urlProvider,
            IMessageFactory messageFactory,
            IStringLocalizer<Strings> localizer,
            IAppSettings appSettings,
            ILogger<BotActivityHandler> logger)
        {
            this.qBotTeamInfo = qBotTeamInfo ?? throw new ArgumentNullException(nameof(qBotTeamInfo));
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.courseSetup = courseSetup ?? throw new ArgumentNullException(nameof(courseSetup));
            this.qBotService = qBotService ?? throw new ArgumentNullException(nameof(qBotService));
            this.urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        /// <inheritdoc/>
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            // Handle Teams scope only.
            if (turnContext.Activity.Conversation.ConversationType != BotConstants.Channel)
            {
                this.logger.LogWarning($"App handles messages/events for Channel conversation only. Received a message in {turnContext.Activity.Conversation.ConversationType}.");
                return new TaskModuleResponse(this.GetErrorMessageBoxResponse(BotErrorCodes.CommandContextNotSupported));
            }

            var channelId = turnContext.Activity.TeamsGetChannelId();
            var courseId = turnContext.Activity.TeamsGetTeamInfo().Id;
            var data = taskModuleRequest.Data as JObject;
            data = JObject.FromObject(data);
            string questionId = data["questionId"].ToString(); // Note: this logic could be improved.
            var senderId = turnContext.Activity.From.AadObjectId;

            try
            {
                // check if quesiton is already answered.
                var question = await this.qBotService.GetQuestionAsync(courseId, channelId, questionId);
                if (!string.IsNullOrEmpty(question.AnswerId))
                {
                    return new TaskModuleResponse(this.GetErrorMessageBoxResponse(BotErrorCodes.QuestionMarkedAsAnswered));
                }

                var member = await this.courseReader.GetMemberAsync(courseId, senderId);
                if (!this.IsUserAuthorizedToPostAnswer(question, member))
                {
                    this.logger.LogInformation("User is not authorized to select an answer.");
                    return new TaskModuleResponse(this.GetErrorMessageBoxResponse(BotErrorCodes.ForbidenToSelectAnswer));
                }
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.QuestionNotFound)
                {
                    this.logger.LogInformation($"Question {questionId} not found.");
                    return new TaskModuleResponse(this.GetErrorMessageBoxResponse(BotErrorCodes.QuestionNotFound));
                }
                else
                {
                    this.logger.LogWarning(exception, "Something went wrong.");
                    return new TaskModuleResponse(this.GetErrorMessageBoxResponse(BotErrorCodes.Unknown));
                }
            }

            var url = await this.urlProvider.GetSelectAnswerPageUrlAsync(courseId, channelId, questionId);
            return new TaskModuleResponse(this.GetTaskModuleContinueResponse(this.localizer.GetString("selectAnswerDialogTitle"), url));
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Handle Teams scope only.
            if (turnContext.Activity.Conversation.ConversationType != BotConstants.Channel)
            {
                this.logger.LogWarning($"App handles messages/events for Channel conversation only. Received a message in {turnContext.Activity.Conversation.ConversationType}.");
                return;
            }

            // Check if the Bot is added to a new team
            foreach (var member in teamsMembersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await this.BotAddedToTeamAsync(teamInfo, turnContext, cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            // else update team members for an existing team.
            await this.TeamsMembersAddedAsync(teamsMembersAdded, teamInfo, turnContext, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> teamsMembersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Handle Teams scope only.
            if (turnContext.Activity.Conversation.ConversationType != BotConstants.Channel)
            {
                this.logger.LogWarning($"App handles messages/events for Channel conversation only. Received a message in {turnContext.Activity.Conversation.ConversationType}.");
                return;
            }

            // Check if the Bot is removed from a team.
            foreach (var member in teamsMembersRemoved)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await this.BotRemovedFromTeamAsync(teamInfo).ConfigureAwait(false);
                    return;
                }
            }

            // else update team members for an existing team.
            await this.TeamsMembersRemovedAsync(teamsMembersRemoved, teamInfo, turnContext, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Handle Teams scope only.
            if (turnContext.Activity.Conversation.ConversationType != BotConstants.Channel)
            {
                this.logger.LogWarning($"App handles messages/events for Channel conversation only. Received a message in {turnContext.Activity.Conversation.ConversationType}.");
                await base.OnMessageActivityAsync(turnContext, cancellationToken);
                return;
            }

            // Handle messageback actions.
            if (BotConstants.SuggestAnswerActionMessage.Equals(turnContext.Activity.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.SuggestAnswerActionAsync(turnContext);
                return;
            }

            // Make sure QBot is tagged. (handle ambient listening)
            if (!turnContext.Activity.MentionsId($"28:{this.appSettings.BotAppId}"))
            {
                this.logger.LogInformation($"App only handles messages where QBot is mentioned.");
                return;
            }

            // Handle root messages only. Reply to other messages shouldn't be tracked as questions.
            var rootMessageId = turnContext.Activity.Conversation.Id.Split('=')[1];
            var messageId = turnContext.Activity.Id;
            if (!messageId.Equals(rootMessageId, StringComparison.InvariantCulture))
            {
                this.logger.LogInformation("App handles root messages only.");

                // Post a message asking users to create a new thread to ask a quesiton.
                var attachment = this.messageFactory.CreateNewThreadMessage();
                var activity = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(activity, cancellationToken);
                return;
            }

            // move to different class.
            await this.NewQuestionPostedAsync(turnContext.Activity.TeamsGetTeamInfo(), turnContext, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected async override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // Handle Teams scope only.
            if (turnContext.Activity.Conversation.ConversationType != BotConstants.Channel)
            {
                this.logger.LogWarning($"App handles messages/events for Channel conversation only. Received a message in {turnContext.Activity.Conversation.ConversationType}.");
                return this.GetMessagingExtensionErrorResponse(BotErrorCodes.CommandContextNotSupported);
            }

            // Bot only supports "message" actions.
            if (!BotConstants.MessageCommandContext.Equals(action.CommandContext, StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.LogWarning($"App handles message command context only. Command Context: {action.CommandContext}");
                return this.GetMessagingExtensionErrorResponse(BotErrorCodes.CommandContextNotSupported);
            }

            // We only support "select this answer" command.
            if (!BotConstants.SelectThisAnswerCommandId.Equals(action.CommandId, StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.LogWarning($"App handles 'select this answer' command only. Command Id: {action.CommandId}");
                return this.GetMessagingExtensionErrorResponse(BotErrorCodes.CommandNotSupported);
            }

            // check if the bot is installed to this team.
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            try
            {
                var cachedCourse = await this.courseReader.GetCourseAsync(teamInfo.Id);
                return await this.SelectAnswerAsync(teamInfo, turnContext, action);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to fetch course details: {teamInfo.Id}.");
                if (exception.Code == ErrorCode.CourseNotFound)
                {
                    // Show error with appropriate information to add the app and tag the bot so that question can be tracked.
                    // Alternatively we can install the app, but since we won't have any question mapped to this message, this action would fail anyway.
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.CourseNotFound);
                }
                else
                {
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.Unknown);
                }
            }
        }

        #region Teams events

        /// <inheritdoc/>
        protected override async Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Read updated course
                var course = await this.qBotTeamInfo.GetCourseAsync(teamInfo, turnContext, cancellationToken);

                // Update course.
                await this.courseSetup.UpdateCourseAsync(course);
            }
            catch (QBotException exception)
            {
                this.logger.LogError(exception, $"Failed to update course. ErrorCode: {exception.Code}, HttpResponseCode: {exception.StatusCode}");
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsTeamDeletedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Delete course.
                await this.courseSetup.DeleteCourseAsync(teamInfo.Id /*courseId*/);
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.CourseNotFound)
                {
                    this.logger.LogInformation($"Course not found. CourseId: {teamInfo.Id}.");
                    return;
                }

                this.logger.LogError(exception, $"Failed to delete course: {teamInfo.Id}. ErrorCode: {exception.Code}, StatusCode: {exception.StatusCode}");
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsTeamHardDeletedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Delete course.
                await this.courseSetup.DeleteCourseAsync(teamInfo.Id /*courseId*/);
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.CourseNotFound)
                {
                    this.logger.LogInformation($"Course not found. CourseId: {teamInfo.Id}.");
                    return;
                }

                this.logger.LogError(exception, $"Failed to delete course: {teamInfo.Id}. ErrorCode: {exception.Code}, StatusCode: {exception.StatusCode}");
            }
        }

        #endregion

        #region Channels events

        /// <inheritdoc/>
        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Channel.
                var courseId = teamInfo.Id;
                var channel = this.qBotTeamInfo.ConvertToChannel(channelInfo, teamInfo);

                // Add channel
                await this.courseSetup.AddChannelAsync(courseId, channel).ConfigureAwait(false);
            }
            catch (QBotException exception)
            {
                this.logger.LogError(exception, $"Failed to add new channel: {channelInfo.Id} to course: {teamInfo.Id}. ErrorCode: {exception.Code}, HttpResponseCode: {exception.StatusCode}");
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Channel info
            var courseId = teamInfo.Id;
            var channelId = channelInfo.Id;

            // Delete channel
            try
            {
                await this.courseSetup.DeleteChannelAsync(courseId, channelId).ConfigureAwait(false);
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.ChannelNotFound)
                {
                    // expected incase we get duplicate event.
                    this.logger.LogWarning($"Channel not found! Course Id: {teamInfo.Id}, Channel Id: {channelInfo.Id}");
                    return;
                }

                this.logger.LogError(exception, $"Failed delete an existing channel: {channelInfo.Id} in course: {teamInfo.Id}. ErrorCode: {exception.Code}, HttpResponseCode: {exception.StatusCode}");
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Updated channel.
            var courseId = teamInfo.Id;
            var channel = this.qBotTeamInfo.ConvertToChannel(channelInfo, teamInfo);
            try
            {
                // Update channel.
                await this.courseSetup.UpdateChannelAsync(courseId, channel).ConfigureAwait(false);
            }
            catch (QBotException exception)
            {
                this.logger.LogError(exception, $"Failed update an existing channel: {channelInfo.Id} in course: {teamInfo.Id}. ErrorCode: {exception.Code}, HttpResponseCode: {exception.StatusCode}");
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsChannelRestoredAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // No action on channel restore.
            await base.OnTeamsChannelRestoredAsync(channelInfo, teamInfo, turnContext, cancellationToken);
        }

        #endregion

        #region Bot Lifecycle events

        /// <summary>
        /// When the bot is added/installed to a new team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task BotAddedToTeamAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Bot added to a team : {teamInfo.Id}.");

            try
            {
                // Read Course
                var course = await this.qBotTeamInfo.GetCourseAsync(teamInfo, turnContext, cancellationToken);

                // Read all the channels
                var channels = await this.qBotTeamInfo.GetChannelsAsync(teamInfo, turnContext, cancellationToken);

                // Read all the course members
                var members = await this.qBotTeamInfo.GetCourseMembersAsync(teamInfo, turnContext, cancellationToken).ConfigureAwait(false);

                // Add new course
                await this.courseSetup.AddNewCourseAsync(course, channels, members).ConfigureAwait(false);
            }
            catch (QBotException exception)
            {
                this.logger.LogError(exception, $"Failed to add course. CourseId: {teamInfo.Id}.");
            }
        }

        /// <summary>
        /// When the bot is removed/uninstalled from an existing team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task BotRemovedFromTeamAsync(TeamInfo teamInfo)
        {
            this.logger.LogInformation($"Bot removed from a team : {teamInfo.Id}.");

            try
            {
                // Delete course
                await this.courseSetup.DeleteCourseAsync(teamInfo.Id/*courseId*/);
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.CourseNotFound)
                {
                    // expected when an event is sent twice.
                    this.logger.LogInformation($"Course not found. CourseId: {teamInfo.Id}.");
                    return;
                }

                this.logger.LogError(exception, $"Failed delete an existing course : {teamInfo.Id}. ErrorCode: {exception.Code}, HttpResponseCode: {exception.StatusCode}");
            }
        }

        #endregion

        #region Team Members events

        /// <summary>
        /// New members are added to an existing team where the bot is installed.
        /// </summary>
        /// <param name="teamsMembersAdded">A list of all the members added to the channel, as described by the conversation update activity</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task TeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"New members added to course: {teamInfo.Id}.");

            var courseId = teamInfo.Id;
            var members = this.qBotTeamInfo.ConvertToCourseMembers(teamsMembersAdded, teamInfo, turnContext, cancellationToken);

            // Add members to the course.
            await this.courseSetup.AddMembersAsync(courseId, members);
        }

        /// <summary>
        /// Existing member(s) are removed from a team where the bot is installed.
        /// </summary>
        /// <param name="teamsMembersRemoved">A list of all the members added to the channel, as described by the conversation update activity</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task TeamsMembersRemovedAsync(IList<TeamsChannelAccount> teamsMembersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var courseId = teamInfo.Id;
            var members = this.qBotTeamInfo.ConvertToCourseMembers(teamsMembersRemoved, teamInfo, turnContext, cancellationToken);

            // Remove members from a couse.
            await this.courseSetup.RemoveMembersAsync(courseId, members);
        }

        #endregion

        #region Questions posted.

        private async Task NewQuestionPostedAsync(TeamInfo teamInfo, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"New Question posted in course: {teamInfo.Id}.");

            try
            {
                var question = await this.qBotTeamInfo.GetQuestionAsync(teamInfo, turnContext, cancellationToken);
                await this.qBotService.AddQuestionAsync(question);
            }
            catch (QBotException exception)
            {
                this.logger.LogError(exception, $"Failed to add a new question. Error Code: {exception.Code}, StatusCode: {exception.StatusCode}");
            }
        }

        #endregion

        #region Message Action - Select an answer.

        private async Task<MessagingExtensionActionResponse> SelectAnswerAsync(TeamInfo teamInfo, ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var courseId = teamInfo.Id;
            var channelId = turnContext.Activity.TeamsGetChannelId();
            var questionMessageId = turnContext.Activity.Conversation.Id.Split('=')[1];
            var messageId = action.MessagePayload.Id;

            var senderId = turnContext.Activity.From.AadObjectId;
            try
            {
                // check if question is already answered.
                var question = await this.qBotService.GetQuestionAsync(courseId, channelId, questionMessageId);
                if (!string.IsNullOrEmpty(question.AnswerId))
                {
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.QuestionMarkedAsAnswered);
                }

                // Check if user is authorized to select an answer.
                // Note: We do not allow global admins to select answer.
                var member = await this.courseReader.GetMemberAsync(courseId, senderId);
                if (!this.IsUserAuthorizedToPostAnswer(question, member))
                {
                    this.logger.LogInformation("User is not authorized to select an answer.");
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.ForbidenToSelectAnswer);
                }
            }
            catch (QBotException exception)
            {
                if (exception.Code == ErrorCode.QuestionNotFound)
                {
                    this.logger.LogInformation($"Question {questionMessageId} not found.");
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.QuestionNotFound);
                }
                else
                {
                    this.logger.LogWarning(exception, "Something went wrong.");
                    return this.GetMessagingExtensionErrorResponse(BotErrorCodes.Unknown);
                }
            }

            this.logger.LogInformation($"Select answer for questionMessageId: {questionMessageId} in course : {courseId}.");

            // If "select this answer" action is performed on quesiton message, we will show all the responses to choose an answer.
            string url;
            if (messageId == questionMessageId)
            {
                url = await this.urlProvider.GetSelectAnswerPageUrlAsync(courseId, channelId, questionMessageId);
            }
            else
            {
                url = await this.urlProvider.GetSelectThisAnswerPageUrlAsync(courseId, channelId, questionMessageId, messageId);
            }

            return this.GetMessagingExtensionActionResponse(this.localizer.GetString("selectAnswerDialogTitle"), url);
        }

        private async Task SuggestAnswerActionAsync(ITurnContext<IMessageActivity> turnContext)
        {
            var valueObject = JObject.Parse(turnContext.Activity.Value.ToString());
            valueObject.TryGetValue("questionId", out var questionId);
            valueObject.TryGetValue("action", out var action);
            valueObject.TryGetValue("suggestedAnswerMessage", out var answerMessage);

            var courseId = turnContext.Activity.TeamsGetTeamInfo().Id;
            var channelId = turnContext.Activity.TeamsGetChannelId();
            var messageId = turnContext.Activity.ReplyToId;

            // Fetch Question.
            if (BotConstants.HelpfulActionText.Equals(action?.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                valueObject.TryGetValue("suggestedAnswerId", out var suggestedAnswerId);

                // Post answer.
                var answer = new Answer
                {
                    CourseId = courseId,
                    ChannelId = channelId,
                    AcceptedById = turnContext.Activity.From.AadObjectId,
                    AuthorId = turnContext.Activity.Recipient.Id,
                    MessageId = messageId,
                    Id = messageId,
                    QuestionId = questionId.ToString(),
                    TimeStamp = turnContext.Activity.Timestamp.Value,
                    Message = answerMessage.ToString(),
                };

                // Suggested Answer
                var suggestedAnswer = new SuggestedAnswer()
                {
                    Answer = answerMessage.ToString(),
                    Id = int.Parse(suggestedAnswerId.ToString(), CultureInfo.InvariantCulture),
                };

                await this.qBotService.PostSuggestedAnswerAsync(answer, suggestedAnswer);
            }
            else if (BotConstants.NotHelpfulActionText.Equals(action?.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                // Prepare suggested answer object.
                var answer = new Answer
                {
                    CourseId = courseId,
                    ChannelId = channelId,
                    AcceptedById = turnContext.Activity.From.AadObjectId,
                    AuthorId = turnContext.Activity.Recipient.Id,
                    MessageId = messageId,
                    Id = messageId,
                    QuestionId = questionId.ToString(),
                    TimeStamp = turnContext.Activity.Timestamp.Value,
                    Message = answerMessage.ToString(),
                };

                await this.qBotService.MarkSuggestedAnswerNotHelpfulAsync(answer, answer.AcceptedById);
            }
            else
            {
                this.logger.LogWarning($"Unexpected action type: {action?.ToString()}");
            }
        }
        #endregion

        private MessagingExtensionActionResponse GetMessagingExtensionActionResponse(string title, string url)
        {
            return new MessagingExtensionActionResponse()
            {
                Task = this.GetTaskModuleContinueResponse(title, url),
            };
        }

        private TaskModuleContinueResponse GetTaskModuleContinueResponse(string title, string url)
        {
            return new TaskModuleContinueResponse()
            {
                Value = new TaskModuleTaskInfo()
                {
                    Title = title,
                    Url = url,
                    FallbackUrl = url,
                    Height = "large",
                    Width = "medium",
                },
            };
        }

        private MessagingExtensionActionResponse GetMessagingExtensionErrorResponse(BotErrorCodes errorCode)
        {
            return new MessagingExtensionActionResponse()
            {
                Task = this.GetErrorMessageBoxResponse(errorCode),
            };
        }

        private TaskModuleContinueResponse GetErrorMessageBoxResponse(BotErrorCodes errorCode)
        {
            var title = this.GetErrorTitle(errorCode);
            var message = this.GetErrorMessage(errorCode);
            return new TaskModuleContinueResponse()
            {
                Value = new TaskModuleTaskInfo()
                {
                    Card = this.messageFactory.CreateErrorMessage(message),
                    Title = title,
                    Width = "small",
                },
            };
        }

        private string GetErrorMessage(BotErrorCodes errorCode)
        {
            switch (errorCode)
            {
                case BotErrorCodes.CourseNotFound:
                    return this.localizer.GetString("courseNotFoundErrorMessage").Value;

                case BotErrorCodes.CommandContextNotSupported:
                    return this.localizer.GetString("commandContextNotSupportedErrorMessage").Value;

                case BotErrorCodes.CommandNotSupported:
                    return this.localizer.GetString("commandNotSupportedErrorMessage").Value;

                case BotErrorCodes.ForbidenToSelectAnswer:
                    return this.localizer.GetString("forbiddenToSelectAnswerErrorMessage").Value;

                case BotErrorCodes.QuestionNotFound:
                    return this.localizer.GetString("questionNotFoundErrorMessage").Value;

                case BotErrorCodes.QuestionMarkedAsAnswered:
                    return this.localizer.GetString("questionMarkedAsAnsweredErrorMessage").Value;

                case BotErrorCodes.Unknown:
                    return this.localizer.GetString("unknownErrorMessage").Value;

                default:
                    this.logger.LogWarning($"Add error message for ErrorCode: {errorCode}");
                    return this.localizer.GetString("unknownErrorMessage").Value;
            }
        }

        private string GetErrorTitle(BotErrorCodes errorCode)
        {
            switch (errorCode)
            {
                case BotErrorCodes.CourseNotFound:
                    return this.localizer.GetString("courseNotFoundErrorTitle").Value;

                case BotErrorCodes.CommandContextNotSupported:
                    return this.localizer.GetString("commandContextNotSupportedErrorTitle").Value;

                case BotErrorCodes.CommandNotSupported:
                    return this.localizer.GetString("commandNotSupportedErrorTitle").Value;

                case BotErrorCodes.ForbidenToSelectAnswer:
                    return this.localizer.GetString("forbiddenToSelectAnswerErrorTitle").Value;

                case BotErrorCodes.QuestionNotFound:
                    return this.localizer.GetString("questionNotFoundErrorTitle").Value;

                case BotErrorCodes.QuestionMarkedAsAnswered:
                    return this.localizer.GetString("questionMarkedAsAnsweredErrorTitle").Value;

                case BotErrorCodes.Unknown:
                    return this.localizer.GetString("unknownErrorTitle").Value;

                default:
                    this.logger.LogWarning($"Add error message for ErrorCode: {errorCode}");
                    return this.localizer.GetString("unknownErrorTitle").Value;
            }
        }

        private bool IsUserAuthorizedToPostAnswer(Question question, Member member)
        {
            // Question author
            if (member.AadId == question.AuthorId)
            {
                return true;
            }

            // Educator / Tutor
            return member.Role != MemberRole.Student;
        }
    }
}
