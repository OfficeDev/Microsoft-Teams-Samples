// <copyright file="TeamsBot.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Bots
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Samples.PeoplePicker.Entities;
    using Microsoft.Teams.Samples.PeoplePicker.Services;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Teams Bot Activity Handler.
    /// </summary>
    public class TeamsBot : TeamsActivityHandler
    {
        private readonly string taskModuleContextWideTemplate = Path.Combine(".", "Resources", "TaskModuleContextWideTemplate.json");
        private readonly string taskModuleOrgWideTemplate = Path.Combine(".", "Resources", "TaskModuleOrgWideTemplate.json");
        private readonly string defaultCardTemplate = Path.Combine(".", "Resources", "DefaultCardTemplate.json");
        private readonly string optionCardTemplate = Path.Combine(".", "Resources", "OptionCardTemplate.json");
        private readonly string messageExtensionContextWideTemplate = Path.Combine(".", "Resources", "MessageExtensionContextWideTemplate.json");
        private readonly string messageExtensionOrgWideTemplate = Path.Combine(".", "Resources", "MessageExtensionOrgWideTemplate.json");
        private readonly string botAdaptiveCardTemplate = Path.Combine(".", "Resources", "BotAdaptiveCardTemplate.json");

        private readonly ICardFactory cardFactory;
        private readonly ILogger<TeamsBot> logger;
        private readonly ITaskModuleResponseFactory taskModuleResponseFactory;
        private readonly IConversationService conversationService;
        private readonly IOptions<AppSettings> appSettingsOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsBot"/> class.
        /// </summary>
        /// <param name="cardFactory">cardFactory.</param>
        /// <param name="logger">logger.</param>
        /// <param name="appSettingsOptions">appSettingsOptions.</param>
        /// <param name="taskModuleResponseFactory">taskModuleResponseFactory.</param>
        /// <param name="conversationService">conversationService.</param>
        public TeamsBot(
            ICardFactory cardFactory,
            ILogger<TeamsBot> logger,
            IOptions<AppSettings> appSettingsOptions,
            ITaskModuleResponseFactory taskModuleResponseFactory,
            IConversationService conversationService)
        {
            this.cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.taskModuleResponseFactory = taskModuleResponseFactory ?? throw new ArgumentNullException(nameof(taskModuleResponseFactory));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            this.appSettingsOptions = appSettingsOptions ?? throw new ArgumentNullException(nameof(appSettingsOptions));
        }

        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text == "task")
            {
                var attachment = this.cardFactory.CreateAdaptiveCardAttachment(this.optionCardTemplate);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
            else if (text == "card")
            {
                var attachment = this.cardFactory.CreateAdaptiveCardAttachment(this.botAdaptiveCardTemplate);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("Use 'task' for task module demo. Use 'card' for bot card demo. Use ... message overflow for Message Extension.");
            }
        }

        /// <inheritdoc/>
        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            string response;

            try
            {
                var adaptiveCardData = JObject.FromObject(invokeValue.Action.Data);

                if (adaptiveCardData["choice"].ToString() == OptionsCardPayload.CurrentContext.ToString())
                {
                    adaptiveCardData["people-picker"] = adaptiveCardData["people-picker-current"];
                }
                else
                {
                    adaptiveCardData["people-picker"] = adaptiveCardData["people-picker-org"];
                }

                response = await this.CreateConversation(adaptiveCardData, turnContext);
            }
            catch (Exception exception)
            {
                response = exception.Message;
            }

            return new AdaptiveCardInvokeResponse
            {
                StatusCode = 200,
                Value = response,
                Type = "application/vnd.microsoft.activity.message",
            };
        }

        /// <inheritdoc/>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var taskModuleData = JObject.FromObject(taskModuleRequest.Data);
            var choice = taskModuleData["data"].ToString();

            if (choice == OptionsCardPayload.CurrentContext.ToString())
            {
                // If user selects Current context then render Current Context Adaptive card as Task Module Response.
                return Task.FromResult(
                    this.taskModuleResponseFactory.CreateTaskModuleCardResponse(
                        this.cardFactory.CreateAdaptiveCardAttachment(this.taskModuleContextWideTemplate)));
            }
            else
            {
                // Else render Org Wide Context Adaptive card as Task Module Response.
                return Task.FromResult(
                    this.taskModuleResponseFactory.CreateTaskModuleCardResponse(
                        this.cardFactory.CreateAdaptiveCardAttachment(this.taskModuleOrgWideTemplate)));
            }
        }

        /// <inheritdoc/>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            string response;

            try
            {
                var taskModuleData = JObject.FromObject(taskModuleRequest.Data);
                response = await this.CreateConversation(taskModuleData, turnContext);
            }
            catch (Exception exception)
            {
                response = exception.Message;
            }

            return this.taskModuleResponseFactory.CreateTaskModuleMessageResponse(response);
        }

        /// <inheritdoc/>
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            string text;

            try
            {
                text = action.MessagePayload.Body.Content.ToString();
            }
            catch (Exception)
            {
                text = "Enter your message";
            }

            switch (action.CommandId)
            {
                case "CurrentContext":
                    return Task.FromResult(new MessagingExtensionActionResponse
                    {
                        Task = this.taskModuleResponseFactory.CreateTaskModuleContinueResponse(
                                this.cardFactory.CreateAdaptiveCardAttachement(
                                    this.messageExtensionContextWideTemplate,
                                    new { text = text })),
                    });

                case "OrgWideContext":
                    return Task.FromResult(new MessagingExtensionActionResponse
                    {
                        Task = this.taskModuleResponseFactory.CreateTaskModuleContinueResponse(
                                this.cardFactory.CreateAdaptiveCardAttachement(
                                    this.messageExtensionOrgWideTemplate,
                                    new { text = text })),
                    });

                default:
                    return Task.FromResult(new MessagingExtensionActionResponse
                    {
                        Task = this.taskModuleResponseFactory.CreateTaskModuleContinueResponse(
                                this.cardFactory.CreateAdaptiveCardAttachement(
                                    this.defaultCardTemplate,
                                    new { text = "Unable to Submit Data." })),
                    });
            }
        }

        /// <inheritdoc/>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            string response;

            try
            {
                var messageExtensionData = JObject.FromObject(action.Data);
                response = await this.CreateConversation(messageExtensionData, turnContext);
            }
            catch (Exception exception)
            {
                response = exception.Message;
            }

            return new MessagingExtensionActionResponse
            {
                Task = this.taskModuleResponseFactory.CreateTaskModuleContinueResponse(
                        this.cardFactory.CreateAdaptiveCardAttachement(this.defaultCardTemplate, new { text = response })),
            };
        }

        /// <summary>
        /// Consumes Conversation Service to create a group conversation.
        /// </summary>
        /// <param name="data">JObject is sent from the calling method containing Title, Message, Users.</param>
        /// <returns>Returns Message to be returned back.</returns>
        private async Task<string> CreateConversation(JObject data, ITurnContext<IInvokeActivity> turnContext)
        {
            string response;

            // Parse data from Adaptive card into Conversation Context
            ConversationContext conversationContext = new ConversationContext();
            conversationContext.Title = data["title"].ToString() ?? throw new ArgumentNullException(nameof(data));
            conversationContext.Message = data["message"].ToString() ?? throw new ArgumentNullException(nameof(data));
            conversationContext.ConversationId = string.Empty;

            // People Picker Returns AAD-ID of users as String, comma seperated for multiple users.
            // Split the user ids from "," and store in an array.
            conversationContext.Users = data["people-picker"].ToString().Split(",") ?? throw new ArgumentNullException(nameof(data));

            try
            {
                // If less than 2 people selected, then cannot create group conversation
                if (conversationContext.Users.Length < 2)
                {
                    throw new Exception("Please Select 2 or more users");
                }

                // Create conversation thread using Conversation Service and return conversation ID.
                conversationContext.ConversationId = await this.conversationService.CreateConversationAsync(conversationContext);

                // If conversation id returned from above is empty/null then the coversation is not created.
                // else the conversation id is returned. Then add the app to conversation and send proactive message.
                if (string.IsNullOrEmpty(conversationContext.ConversationId))
                {
                    throw new Exception("Unable to create Group Conversation");
                }

                await this.conversationService.AddAppToConversationAsync(conversationContext);
                await this.conversationService.SendProactiveMessageAsync(turnContext, conversationContext);
                response = "Group Conversation created successfully";
            }
            catch (Exception e)
            {
                response = e.Message;
            }

            return response;
        }
    }
}