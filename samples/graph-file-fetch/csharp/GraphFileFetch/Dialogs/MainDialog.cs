// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using GraphFileFetch.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GraphFileFetch.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        public readonly IConfiguration _configuration;
        protected readonly IStatePropertyAccessor<TokenState> _conversationState;

        public MainDialog(IConfiguration configuration, ConversationState conversationState)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;
            this._conversationState = conversationState.CreateProperty<TokenState>(nameof(TokenState));

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Conversation.ConversationType != "personal")
                {
                    var token = await this._conversationState.GetAsync(stepContext.Context, () => new TokenState());
                    token.AccessToken = tokenResponse.Token;
                    await this._conversationState.SetAsync(stepContext.Context, token);

                    // Handle different activity types
                    if (stepContext.Context.Activity.Type == ActivityTypes.Message)
                    {
                        var messageId = stepContext.Context.Activity.Id;
                        var graphClient = GetAuthenticatedClient(token.AccessToken);
                        var conversationType = stepContext.Context.Activity.Conversation.ConversationType;
                        var attachmentUrl = "";

                        if (conversationType == "groupChat")
                        {
                            var chatId = stepContext.Context.Activity.Conversation.Id;
                            // Only cast when we know it's a message activity
                            var messageContext = stepContext.Context.Activity.Type == ActivityTypes.Message ? 
                                (ITurnContext<IMessageActivity>)stepContext.Context : null;
                            
                            if (messageContext != null)
                            {
                                attachmentUrl = await GetGroupChatAttachment(messageContext, graphClient, chatId, messageId, cancellationToken);
                            }
                        }
                        else if (conversationType == "channel")
                        {
                            var teamsChannelData = stepContext.Context.Activity.GetChannelData<TeamsChannelData>();
                            var channelId = teamsChannelData.Channel.Id;
                            TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(stepContext.Context, stepContext.Context.Activity.TeamsGetTeamInfo().Id, cancellationToken);
                            var teamId = teamDetails.AadGroupId;

                            var messageContext = stepContext.Context.Activity.Type == ActivityTypes.Message ? 
                                (ITurnContext<IMessageActivity>)stepContext.Context : null;
                            
                            if (messageContext != null)
                            {
                                attachmentUrl = await GetTeamsChannelAttachment(messageContext, graphClient, teamId, channelId, messageId, cancellationToken);
                            }
                        }

                        if (!string.IsNullOrEmpty(attachmentUrl))
                        {
                            try
                            {
                                var attachment = new HeroCard
                                {
                                    Title = "Download File",
                                    Buttons = new List<CardAction>
                                    {
                                        new CardAction
                                        {
                                            Title = "Download",
                                            Type = ActionTypes.OpenUrl,
                                            Value = attachmentUrl
                                        }
                                    }
                                }.ToAttachment();

                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                            }
                            catch (ServiceException ex)
                            {
                                await stepContext.Context.SendActivityAsync(
                                    MessageFactory.Text($"Error accessing Graph API: {ex.Message}"),
                                    cancellationToken);
                            }
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(
                                MessageFactory.Text("No attachments found in the message."),
                                cancellationToken);
                        }
                    }
                    else if (stepContext.Context.Activity.Type == ActivityTypes.Invoke)
                    {
                        // Handle invoke activity (like Teams sign-in verification)
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Authentication successful. Please send the file again"), cancellationToken);
                    }

                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                await stepContext.Context.SendActivityAsync("Login successfully", cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        protected async Task<string> GetGroupChatAttachment(ITurnContext<IMessageActivity> turnContext, GraphServiceClient graphClient, string chatId, string messageID, CancellationToken cancellationToken)
        {
            try
            {
                // Get recent messages from the chat
                var messages = await graphClient.Chats[chatId].Messages[messageID]
                    .GetAsync();

                // Find first message with attachments and get hosted content URL
                var messageWithAttachment = messages.Attachments;

                if (messageWithAttachment.Count != 0)
                {
                    var attachment = messageWithAttachment[0];
                    return attachment.ContentUrl;
                }

                return null;

            }
            catch (ServiceException ex)
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"Error getting attachment: {ex.Message}"),
                    cancellationToken);
                throw;
            }
        }

        protected async Task<string> GetTeamsChannelAttachment(ITurnContext<IMessageActivity> turnContext, GraphServiceClient graphClient, string teamId, string channelId, string messageID, CancellationToken cancellationToken)
        {
            try
            {
                // Get recent messages from the team channel
                var messages = await graphClient.Teams[teamId].Channels[channelId].Messages[messageID]
                    .GetAsync();

                // Find first message with attachments and get hosted content URL
                var messageWithAttachment = messages.Attachments;

                if (messageWithAttachment != null)
                {
                    var attachment = messageWithAttachment[0];
                    return attachment.ContentUrl;
                }

                return null;
            }
            catch (ServiceException ex)
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"Error getting team attachment: {ex.Message}"),
                    cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Generates an authenticated GraphServiceClient using a token.
        /// </summary>
        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public SimpleAccessTokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> context = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }

        private GraphServiceClient GetAuthenticatedClient(string token)
        {
            var tokenProvider = new SimpleAccessTokenProvider(token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }
    }
}
