// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Extensions.Teams.Compat;
using Microsoft.Agents.Extensions.Teams.Models;
using System.Text.Json;

namespace MsgextUnfurlingAcLoop.Bots;

public class MsgextUnfurlingAcLoopComponents : TeamsActivityHandler
{
    /// <summary>
    /// Reads the adaptive card JSON from the Resources folder.
    /// </summary>
    private static string GetAdaptiveCardJson()
    {
        var paths = new[] { ".", "Resources", "adaptiveCard.json" };
        return File.ReadAllText(Path.Combine(paths));
    }

    /// <summary>
    /// Invoked when an app based link query activity is received from the connector.
    /// </summary>
    protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(
        ITurnContext<IInvokeActivity> turnContext,
        AppBasedLinkQuery query,
        CancellationToken cancellationToken)
    {
        AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

        var attachments = new MessagingExtensionAttachment
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
                    new()
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
    /// Invoked when an adaptive card action invoke activity is received.
    /// </summary>
    protected override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(
        ITurnContext<IInvokeActivity> turnContext,
        AdaptiveCardInvokeValue adaptiveCardInvokeValue,
        CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Name == "adaptiveCard/action")
        {
            var paths = new[] { ".", "Resources", "adaptiveCardSuccess.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            return Task.FromResult(new AdaptiveCardInvokeResponse
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = JsonSerializer.Deserialize<JsonElement>(adaptiveCardJson)
            });
        }

        return Task.FromResult<AdaptiveCardInvokeResponse>(null!);
    }

    /// <summary>
    /// Invoked when the user is searching in the messaging extension query.
    /// </summary>
    protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
        ITurnContext<IInvokeActivity> turnContext,
        MessagingExtensionQuery query,
        CancellationToken cancellationToken)
    {
        var card = new ThumbnailCard
        {
            Title = "Adaptive Card-based Loop component",
            Text = "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
        };

        AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

        return Task.FromResult(new MessagingExtensionResponse
        {
            ComposeExtension = new MessagingExtensionResult
            {
                AttachmentLayout = "list",
                Type = "result",
                Attachments = new List<MessagingExtensionAttachment>
                {
                    new()
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