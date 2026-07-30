// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples;

public class AdaptiveCardActionsBot : ActivityHandler
{
    private const string CommandString = "Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card";

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(MessageFactory.Text("Hello and Welcome!"), cancellationToken);
        await turnContext.SendActivityAsync(MessageFactory.Text(CommandString), cancellationToken);
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Text != null)
        {
            var text = turnContext.Activity.Text.ToLowerInvariant();

            if (text.Contains("card actions"))
            {
                await SendAdaptiveCardAsync(turnContext, cancellationToken, "AdaptiveCardActions.json");
            }
            else if (text.Contains("suggested actions"))
            {
                await turnContext.SendActivityAsync("Please Enter a color from the suggested action choices", cancellationToken: cancellationToken);
                await SendAdaptiveCardAsync(turnContext, cancellationToken, "SuggestedActions.json");
                await SendSuggestedActionsAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("togglevisibility"))
            {
                await SendAdaptiveCardAsync(turnContext, cancellationToken, "ToggleVisibleCard.json");
            }
            else if (text.Contains("red") || text.Contains("blue") || text.Contains("yellow"))
            {
                await turnContext.SendActivityAsync(ProcessInput(text), cancellationToken: cancellationToken);
                await SendSuggestedActionsAsync(turnContext, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(CommandString), cancellationToken);
            }
        }

        if (turnContext.Activity.Value != null)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Data Submitted: {turnContext.Activity.Value}"), cancellationToken);
        }
    }

    private static string ProcessInput(string text)
    {
        const string colorText = "is the best color, I agree.";
        var colorResponses = new Dictionary<string, string>
        {
            { "red", $"Red {colorText}" },
            { "yellow", $"Yellow {colorText}" },
            { "blue", $"Blue {colorText}" }
        };

        return colorResponses.TryGetValue(text, out var response) ? response : "Please select a color from the suggested action choices";
    }

    private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var reply = MessageFactory.Text("What is your favorite color?");
        reply.SuggestedActions = new SuggestedActions
        {
            Actions = new List<CardAction>
            {
                new() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                new() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                new() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" }
            },
            To = new List<string> { turnContext.Activity.From.Id }
        };

        await turnContext.SendActivityAsync(reply, cancellationToken);
    }

    private static async Task SendAdaptiveCardAsync(ITurnContext turnContext, CancellationToken cancellationToken, string cardFileName)
    {
        var adaptiveCard = CreateAdaptiveCardAttachment(
            Path.Combine(".", "Cards", cardFileName),
            turnContext.Activity.From.Name);
        await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCard), cancellationToken);
    }

    private static Attachment CreateAdaptiveCardAttachment(string filePath, string name = null)
    {
        var adaptiveCardJson = File.ReadAllText(filePath);
        var template = new AdaptiveCardTemplate(adaptiveCardJson);
        var cardJsonString = template.Expand(new { createdBy = name });

        return new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJsonString)
        };
    }
}