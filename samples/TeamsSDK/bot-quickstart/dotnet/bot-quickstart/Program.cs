// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Clients;

// Initialize Teams App - automatically uses CLIENT_ID and CLIENT_SECRET from environment variables
var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

// Handle conversation update events (when bot is added or members join)
teamsApp.OnConversationUpdate(async context =>
{
    var membersAdded = context.Activity.MembersAdded;
    if (membersAdded != null)
    {
        foreach (var member in membersAdded)
        {
            // Check if bot was added to the conversation
            if (member.Id == context.Activity.Recipient?.Id)
            {
                await SendWelcomeMessage(context);
            }
        }
    }
});

// Handles incoming messages and routes to appropriate functions based on message content
teamsApp.OnMessage(async context =>
{
    // Get message text and normalize it
    var text = (context.Activity.Text ?? "").Trim().ToLower();

    // Handle mention me command
    if (text.Contains("mentionme") || text.Contains("mention me"))
    {
        await MentionUser(context);
    }
    // Handle whoami command
    else if (text.Contains("whoami"))
    {
        await GetSingleMember(context);
    }
    // Handle welcome command
    else if (text.Contains("welcome"))
    {
        await SendWelcomeMessage(context);
    }
    // Echo greeting messages
    else if (text.Contains("hi") || text.Contains("hello"))
    {
        await EchoMessage(context, text);
    }
    else
    {
        await SendWelcomeMessage(context);
    }
});

// Sends a welcome message
async Task SendWelcomeMessage<T>(IContext<T> context) where T : IActivity
{
    await context.Send("Welcome to the Teams Quickstart Bot!");
}

// Echo back the user's message
async Task EchoMessage(IContext<MessageActivity> context, string text)
{
    await context.Send($"**Echo :** {text}");
}

// Retrieves and displays information about the current user
async Task GetSingleMember(IContext<MessageActivity> context)
{
    await context.Send($"You are: {context.Activity.From.Name}");
}

// Mention a user in a message
async Task MentionUser(IContext<MessageActivity> context)
{
    var member = context.Activity.From;
    var mentionText = $"<at>{member.Name}</at>";
    var activity = new MessageActivity()
        .WithText($"Hello {mentionText}")
        .AddMention(member, addText: false);

    await context.Send(activity);
}

// Starts the Teams bot application and listens for incoming requests
webApp.Run();
