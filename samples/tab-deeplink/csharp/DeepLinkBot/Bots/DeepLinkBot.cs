// <copyright file="DeepLinkBot.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using AdaptiveCards;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.AgentSamples.Services;
using System.Linq;
using System.Text.Json;

namespace Microsoft.AgentSamples.Bots;

/// <summary>
/// DeepLink Agent that demonstrates Teams deep linking functionality.
/// Migrated from Bot Builder SDK to Microsoft 365 Agents SDK.
/// </summary>
public class DeepLinkBot : AgentApplication
{
    private readonly IConfiguration _configuration;
    private readonly DeeplinkHelper _deeplinkHelper;

    public static string ChannelId { get; set; } = "<AddYourTeamsChannelId>";
    private const string TeamsUrl = "https://teams.microsoft.com/l/entity/";

    public DeepLinkBot(AgentApplicationOptions options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _deeplinkHelper = new DeeplinkHelper();

        // Register message activity handler
        OnActivity(ActivityTypes.Message, OnMessageActivityAsync);

        // Register conversation update handler for welcoming new members
        OnConversationUpdate(ConversationUpdateEvents.MembersAdded, OnMembersAddedAsync);
    }

    /// <summary>
    /// Handles incoming message activities.
    /// </summary>
    private async Task OnMessageActivityAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var userName = turnContext.Activity.From?.Name ?? "User";
        var attachment = CreateAdaptiveDeepLinkCard(userName, turnContext);
        await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
    }

    /// <summary>
    /// Handles members added to the conversation.
    /// </summary>
    private async Task OnMembersAddedAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var welcomeText = "Hello and welcome!";

        foreach (var member in turnContext.Activity.MembersAdded ?? Enumerable.Empty<ChannelAccount>())
        {
            if (member.Id != turnContext.Activity.Recipient?.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Creates an adaptive card with deep link buttons.
    /// </summary>
    private Attachment CreateAdaptiveDeepLinkCard(string userName, ITurnContext turnContext)
    {
        string tabUrlTask1;
        string tabUrlTask2;
        string tabUrlTask3;
        string extendedDeepLink;
        string? sidePanelLink = null;

        var conversationType = turnContext.Activity.Conversation?.ConversationType;

        if (conversationType == "channel")
        {
            var conversationId = turnContext.Activity.Conversation?.Id ?? string.Empty;
            ChannelId = conversationId.Contains(';') ? conversationId.Split(';')[0] : conversationId;

            var appId = _configuration["MicrosoftAppId"] ?? string.Empty;
            var baseUrl = _configuration["BaseURL"] ?? string.Empty;
            var channelEntityId = _configuration["ChannelEntityId"] ?? string.Empty;

            tabUrlTask1 = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, ChannelId, channelEntityId, "bot1");
            tabUrlTask2 = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, ChannelId, channelEntityId, "bot2");
            tabUrlTask3 = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, ChannelId, channelEntityId, "bot3");
            extendedDeepLink = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, ChannelId, channelEntityId, "");
        }
        else
        {
            var teamsAppId = _configuration["TeamsAppId"] ?? string.Empty;
            var tabEntityId = _configuration["TabEntityId"] ?? string.Empty;
            var baseUrl = _configuration["BaseURL"] ?? string.Empty;
            var channelEntityId = _configuration["ChannelEntityId"] ?? string.Empty;
            var chatId = turnContext.Activity.Conversation?.Id ?? string.Empty;

            tabUrlTask1 = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, teamsAppId, tabEntityId, "topic1");
            tabUrlTask2 = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, teamsAppId, tabEntityId, "topic2");
            tabUrlTask3 = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, teamsAppId, tabEntityId, "topic3");
            extendedDeepLink = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, teamsAppId, tabEntityId, "");
            sidePanelLink = _deeplinkHelper.GetDeepLinkToMeetingSidePanel(TeamsUrl, teamsAppId, baseUrl, channelEntityId, chatId, "chat");
        }

        // Create action buttons for deep links
        var actions = new List<AdaptiveAction>
        {
            new AdaptiveOpenUrlAction
            {
                Title = "Bots in Teams",
                Url = new Uri(tabUrlTask1)
            },
            new AdaptiveOpenUrlAction
            {
                Title = "Bot Framework SDK",
                Url = new Uri(tabUrlTask2)
            },
            new AdaptiveOpenUrlAction
            {
                Title = "Teams Apps",
                Url = new Uri(tabUrlTask3)
            },
            new AdaptiveOpenUrlAction
            {
                Title = "Extended Deeplink features",
                Url = new Uri(extendedDeepLink)
            }
        };

        // Add side panel link if available
        if (!string.IsNullOrEmpty(sidePanelLink))
        {
            actions.Add(new AdaptiveOpenUrlAction
            {
                Title = "Side Panel Deeplink",
                Url = new Uri(sidePanelLink)
            });
        }

        var deepLinkCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = $"Hey {userName}! Please click on below buttons to navigate to a tab!",
                    Size = AdaptiveTextSize.Large,
                    Wrap = true,
                    Weight = AdaptiveTextWeight.Bolder
                }
            },
            Actions = actions
        };

        // Serialize the AdaptiveCard to JSON and then deserialize to JsonElement
        // This ensures proper serialization when sending to Teams
        var cardJson = deepLinkCard.ToJson();
        var cardObject = JsonSerializer.Deserialize<JsonElement>(cardJson);

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = cardObject
        };
    }
}
