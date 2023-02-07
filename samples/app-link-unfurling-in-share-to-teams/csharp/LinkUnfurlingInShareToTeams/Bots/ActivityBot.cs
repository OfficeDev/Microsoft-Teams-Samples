// <copyright file="ActivityBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using AdaptiveCards;
using LinkUnfurlingInShareToTeams.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUnfurlingInShareToTeams.Bots
{
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        private readonly string _microsoftAppId;

        public ActivityBot(IConfiguration configuration)
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            _microsoftAppId= configuration["MicrosoftAppId"] ?? throw new NullReferenceException("MicrosoftAppId");
        }

        /// <summary>
        /// Invoked when an app based link query activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">The invoke request body type for app-based link query</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var preview = new MessagingExtensionAttachment(
                                contentType: HeroCard.ContentType,
                                contentUrl: null,
                                content: GetUnfurlCard());

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = AttachmentLayoutTypes.List,
                    Attachments = new List<MessagingExtensionAttachment>() {
                        new MessagingExtensionAttachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = GetUnfurlCard(),
                            Preview = preview,
                        }
                    }
                },
            };
        }

        /// <summary>
        /// Invoked when link is unfurled.
        /// <returns> Adaptive card for link unfurling.</returns>
        private AdaptiveCard GetUnfurlCard()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Analytics details:",
                        Size = AdaptiveTextSize.Default
                    },
                    new AdaptiveImage()
                    {
                        Url = new Uri(_applicationBaseUrl+"/report.png")
                    }
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
                                        ContentUrl = $"{_applicationBaseUrl}/tab?openInTeams=true",
                                        WebsiteUrl = $"{_applicationBaseUrl}/tab?openInTeams=false",
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
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{_microsoftAppId}/tab?webUrl={_applicationBaseUrl}/tab?openInTeams=true"),
                    },
                },
            };

            return card;
        }
    }
}