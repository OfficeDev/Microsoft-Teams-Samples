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
        private readonly string _applicationBaseUrl;

        public ActivityBot(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"] ?? throw new NullReferenceException("MicrosoftAppId");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForStageView()));
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
                        Title = "view via card action",
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
                                        ContentUrl = $"{_applicationBaseUrl}/home",
                                        WebsiteUrl = $"{_applicationBaseUrl}/home",
                                        Name = "Stage view",
                                        EntityId = "entityId"
                                    }
                                }
                            },
                        },
                    },
                    new AdaptiveOpenUrlAction
                    {
                        Title = "view via deeplink",
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
            var contextUrl = HttpUtility.UrlEncode("{" + $"“contentUrl”:”{_applicationBaseUrl}/home”,“websiteUrl”:”{_applicationBaseUrl}/home”,“name”:”Stage View”" + "}");
            return $"https://teams.microsoft.com/l/stage/{_appId}/0?context={contextUrl}";
        }
    }
}