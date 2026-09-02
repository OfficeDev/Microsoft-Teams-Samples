// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Samples.BotCards.Handlers;

// Initialize Teams App - reads Entra credentials from the AzureAd configuration section
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTeamsBotApplication();
var webApp = builder.Build();
TeamsBotApplication teamsApp = webApp.UseTeamsBotApplication();

// Handles card action submissions
teamsApp.OnAdaptiveCardAction(async (context, cancellationToken) =>
{
    var data = context.Activity.Value?.Action?.Data;

    if (data is null || !data.TryGetValue("name", out var nameValue))
    {
        return AdaptiveCardResponse.CreateMessageResponse("No data specified", 200);
    }

    var name = nameValue is JsonElement element ? element.GetString() : nameValue?.ToString();
    await context.SendAsync($"Data Submitted: {name}", cancellationToken);

    return AdaptiveCardResponse.CreateMessageResponse("Action processed successfully");
});

// Handles incoming messages and routes to appropriate functions based on message content
teamsApp.OnMessage(async (context, cancellationToken) =>
{
    var text = (context.Activity.Text ?? "").Trim().ToLower();

    if (text.Contains("card actions"))
    {
        await context.SendAsync(Cards.CreateAdaptiveCardActionsActivity(), cancellationToken);
    }
    else if (text.Contains("toggle visibility"))
    {
        await context.SendAsync(Cards.CreateToggleVisibilityActivity(), cancellationToken);
    }
    else
    {
        await context.SendAsync(
            "Welcome to the Cards Bot! To interact with me, send one of the following commands: 'card actions' or 'toggle visibility'",
            cancellationToken);
    }
});

// Starts the Teams bot application and listens for incoming requests
webApp.Run();

