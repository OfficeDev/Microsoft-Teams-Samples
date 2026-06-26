// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace CommandsMenu.Bots;

public class CommandsMenuBot : TeamsActivityHandler
{
    private readonly string _flightsDetailsCardTemplate = Path.Combine(".", "Resources", "flightsDetails.json");
    private readonly string _searchHotelsCardTemplate = Path.Combine(".", "Resources", "searchHotels.json");

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        turnContext.Activity.RemoveRecipientMention();

        if (turnContext.Activity.Text != null)
        {
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("search flights"))
                await SendAdaptiveCardAsync(turnContext, _flightsDetailsCardTemplate, cancellationToken);
            else if (text.Contains("search hotels"))
                await SendAdaptiveCardAsync(turnContext, _searchHotelsCardTemplate, cancellationToken);
            else if (text.Contains("help"))
                await turnContext.SendActivityAsync(MessageFactory.Text("Displays this help message."), cancellationToken);
            else if (text.Contains("best time to fly"))
                await turnContext.SendActivityAsync(MessageFactory.Text("Best time to fly to London for a 5 day trip is summer."), cancellationToken);
        }
        else if (turnContext.Activity.Value != null)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Hotel search details are: " + turnContext.Activity.Value), cancellationToken);
        }
    }

    private async Task SendAdaptiveCardAsync(ITurnContext<IMessageActivity> turnContext, string cardTemplatePath, CancellationToken cancellationToken)
    {
        var cardJSON = await File.ReadAllTextAsync(cardTemplatePath, cancellationToken);
        var adaptiveCardAttachment = new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJSON),
        };

        await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
    }
}
