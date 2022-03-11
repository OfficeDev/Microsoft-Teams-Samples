// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUnfurlingStt.Bots
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

        // Get unfurling card.
        private AdaptiveCard GetUnfurlCard()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "The analytics details are",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(_applicationBaseUrl+"/report.png")
            });

            card.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Title = "Open tab",
                Url = new Uri($"https://teams.microsoft.com/l/entity/{_microsoftAppId}/tab?webUrl={_applicationBaseUrl}/tab")
            });

            return card;
        }
    }
}