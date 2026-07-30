// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.BotBuilderSamples.Bots;

public class LinkUnfurlingBot : TeamsActivityHandler
{
    protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(
        ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
    {
        var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));
        adaptiveCard.Body.Add(new AdaptiveTextBlock
        {
            Text = "Adaptive Card",
            Size = AdaptiveTextSize.ExtraLarge
        });
        adaptiveCard.Body.Add(new AdaptiveImage
        {
            Url = new Uri("https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png")
        });

        var preview = new MessagingExtensionAttachment
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
                Attachments =
                [
                    new MessagingExtensionAttachment
                    {
                        Content = adaptiveCard,
                        ContentType = AdaptiveCard.ContentType,
                        Preview = preview,
                    },
                ],
            },
        });
    }

    protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
        ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
    {
        return query.CommandId switch
        {
            "searchQuery" => Task.FromResult(CreateSearchQueryResponse()),
            _ => throw new NotImplementedException($"Invalid CommandId: {query.CommandId}"),
        };
    }

    private static MessagingExtensionResponse CreateSearchQueryResponse()
    {
        var card = new HeroCard
        {
            Title = "This is a Link Unfurling Sample",
            Subtitle = "It will unfurl links from *.BotFramework.com",
            Text = "This sample demonstrates how to handle link unfurling in Teams. Please review the readme for more information.",
        };

        return new MessagingExtensionResponse
        {
            ComposeExtension = new MessagingExtensionResult
            {
                AttachmentLayout = "list",
                Type = "result",
                Attachments =
                [
                    new MessagingExtensionAttachment
                    {
                        Content = card,
                        ContentType = HeroCard.ContentType,
                        Preview = card.ToAttachment(),
                    },
                ],
            },
        };
    }
}
