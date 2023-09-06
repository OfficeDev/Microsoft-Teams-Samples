// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class LinkUnfurlingBot : TeamsActivityHandler
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static String GetAdaptiveCardJson()
        {
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            return File.ReadAllText(Path.Combine(paths));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

            var attachments = new MessagingExtensionAttachment()
            {
                Content = result.Card,
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
                             Content = result.Card,
                             ContentType = AdaptiveCard.ContentType,
                             Preview = attachments,

                        },
                    },
                },
            });
        }


        /// <summary>
        ///  Invoked when an invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext,AdaptiveCardInvokeValue adaptiveCardInvokeValue,  CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                var paths = new[] { ".", "Resources", "adaptiveCardSuccess.json" };
                var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
                var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                {
                    StatusCode = 200,
                    Type = AdaptiveCard.ContentType,
                    Value = JsonConvert.DeserializeObject(adaptiveCardJson)
                };

                return adaptiveCardResponse;
            }
            return null;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var card = new ThumbnailCard
            {
                Title = "Adaptive Card-based Loop component",
                Text = "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
            };

            AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

            var attachments = new MessagingExtensionAttachment()
            {
                Content = result.Card,
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
                            Content = result.Card,
                            ContentType = AdaptiveCard.ContentType,
                            Preview = card.ToAttachment(),
                        },
                    },
                },
            });
        }
    }
}
