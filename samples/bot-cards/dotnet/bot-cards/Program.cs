// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api;
using Microsoft.Teams.Samples.BotCards.Handlers;

// Initialize Teams App - automatically uses CLIENT_ID and CLIENT_SECRET from environment variables
var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

// Handles card action submissions
teamsApp.OnAdaptiveCardAction(async context =>
{
    var data = context.Activity.Value?.Action?.Data;
    var name = data?["name"]?.ToString() ?? "";
    await context.Send($"Data Submitted: {name}");
});

// Handles incoming messages and routes to appropriate functions based on message content
teamsApp.OnMessage(async context =>
{
    var text = (context.Activity.Text ?? "").Trim().ToLower();

    if (text.Contains("card actions"))
    {
        await Cards.SendAdaptiveCardActions(context);
    }
    else if (text.Contains("toggle visibility"))
    {
        await Cards.SendToggleVisibilityCard(context);
    }
    else
    {
        await SendWelcomeMessage(context);
    }
});

// Starts the Teams bot application and listens for incoming requests
webApp.Run();

// Sends a welcome message
async Task SendWelcomeMessage<T>(IContext<T> context) where T : IActivity
{
    await context.Send("Welcome to the Cards Bot! To interact with me, send one of the following commands: 'card actions' or 'toggle visibility'");
}
