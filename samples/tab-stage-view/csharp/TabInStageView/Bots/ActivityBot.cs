// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using TabInStageView.Models;

namespace TabInStageView.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _appId;
        private readonly string _applicationBaseURL;
        public static string _threadId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityBot"/> class.
        /// </summary>
        /// <param name="configuration">configuration of application.</param>
        public ActivityBot(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"] ?? throw new NullReferenceException("MicrosoftAppId");
            _applicationBaseURL = configuration["ApplicationBaseURL"] ?? throw new NullReferenceException("ApplicationBaseURL");
        }

        // <summary>
        /// Overriding to send welcome card once Bot/ME is installed in team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Welcome card  when bot is added first time by user.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";

            // Set thread Id
            _threadId = turnContext.Activity.Conversation.Id;

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForStageView()));
        }

        /// </summary>
        /// Overriding to invoke when an app based link query activity is received.
        /// </summary>
        protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.5"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Click the button to open the Url in tab stage view",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "View via card action",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "invoke",
                                Value = new TabInfoAction
                                {
                                    Type = "tab/tabInfoAction",
                                    TabInfo = new TabInfo
                                    {
                                        ContentUrl = $"{_applicationBaseURL}",
                                        WebsiteUrl = $"{_applicationBaseURL}",
                                        Name = "Stage view",
                                        EntityId = "entityId"
                                    }
                                }
                            },
                        },
                    }
                },
            };

            var attachments = new MessagingExtensionAttachment()
            {
                Content = card,
                ContentType = AdaptiveCard.ContentType
            };
            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new List<MessagingExtensionAttachment>
                    {
                        new MessagingExtensionAttachment
                        {
                             Content = card,
                             ContentType = AdaptiveCard.ContentType,
                             Preview = attachments,
                        },
                    },
                },
            });
        }

        /// <summary>
        /// Sample Adaptive card for Stage view.
        /// </summary>
        private Attachment GetAdaptiveCardForStageView()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Click the button to open the Url in tab stage view",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "View via card action",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "invoke",
                                Value = new TabInfoAction
                                {
                                    Type = "tab/tabInfoAction",
                                    TabInfo = new TabInfo
                                    {
                                        ContentUrl = $"{_applicationBaseURL}",
                                        WebsiteUrl = $"{_applicationBaseURL}",
                                        Name = "Stage view",
                                        EntityId = "entityId"
                                    }
                                }
                            },
                        },
                    },
                    new AdaptiveOpenUrlAction
                    {
                        Title = "View via deeplink",
                        Url = new Uri(GetDeeplinkForStageView()),
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Create deeplink for stage view.
        /// </summary>
        private string GetDeeplinkForStageView()
        {
            var deepLinkUrl = "https://teams.microsoft.com/l/stage/" + _appId + "/0?context=" + HttpUtility.UrlEncode("{\"contentUrl\":\""+_applicationBaseURL+"\",\"websiteUrl\":\""+_applicationBaseURL+"\",\"name\":\"DemoStageView\"}");
            return deepLinkUrl;
        }
    }
}