// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class LinkUnfurlingBot : TeamsActivityHandler
    {
        protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));
            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Adaptive Card",
                Size = AdaptiveTextSize.ExtraLarge
            });
            adaptiveCard.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png")
            });
            var attachments = new MessagingExtensionAttachment()
            {
                Content = adaptiveCard,
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
                             Content = adaptiveCard,
                             ContentType = AdaptiveCard.ContentType,
                             Preview = attachments,
                                   
                        },       
                    },
                },
            }); 
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            //Note: The Teams manifest.json for this sample also includes a Search Query, in order to enable installing from App Studio.

            switch (query.CommandId)
            {
                // These commandIds are defined in the Teams App Manifest.
                case "searchQuery":
                    var card = new HeroCard
                    {
                        Title = "This is a Link Unfurling Sample",
                        Subtitle = "It will unfurl links from *.BotFramework.com",
                        Text = "This sample demonstrates how to handle link unfurling in Teams.  Please review the readme for more information.",
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
                                ContentType = HeroCard.ContentType,
                                Preview = card.ToAttachment(),
                            },
                        },
                        },
                    });

                default:
                    throw new NotImplementedException($"Invalid CommandId: {query.CommandId}");
            }
        }
    }
}
