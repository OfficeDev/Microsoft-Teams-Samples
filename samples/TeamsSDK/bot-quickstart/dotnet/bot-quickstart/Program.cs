// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Schema;

// Initialize Teams App - credentials are read from the AzureAd configuration section
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTeamsBotApplication();

var webApp = builder.Build();
TeamsBotApplication teamsApp = webApp.UseTeamsBotApplication();

// Handle conversation update events (when bot is added or members join)
teamsApp.OnMembersAdded(async (context, cancellationToken) =>
{
    var membersAdded = context.Activity.MembersAdded;
    if (membersAdded != null)
    {
        foreach (var member in membersAdded)
        {
            // Check if bot was added to the conversation
            if (member.Id == context.Activity.Recipient?.Id)
            {
                await SendWelcomeMessage(context, cancellationToken);
            }
        }
    }
});

// Handles incoming messages and routes to appropriate functions based on message content
teamsApp.OnMessage(async (context, cancellationToken) =>
{
    // Get message text and normalize it
    var text = (context.Activity.Text ?? "").Trim().ToLower();

    // Handle mention me command
    if (text.Contains("mentionme") || text.Contains("mention me"))
    {
        await MentionUser(context, cancellationToken);
    }
    // Handle whoami command
    else if (text.Contains("whoami"))
    {
        await GetSingleMember(context, cancellationToken);
    }
    // Handle welcome command
    else if (text.Contains("welcome"))
    {
        await SendWelcomeMessage(context, cancellationToken);
    }
    // Echo greeting messages
    else if (text.Contains("hi") || text.Contains("hello"))
    {
        await EchoMessage(context, text, cancellationToken);
    }
    else
    {
        await SendWelcomeMessage(context, cancellationToken);
    }
});

// Sends a welcome message
async Task SendWelcomeMessage<T>(Context<T> context, CancellationToken cancellationToken) where T : TeamsActivity
{
    await context.SendAsync("Welcome to the Teams Quickstart Bot!", cancellationToken);
}

// Echo back the user's message
async Task EchoMessage(Context<MessageActivity> context, string text, CancellationToken cancellationToken)
{
    await context.SendAsync($"**Echo :** {text}", cancellationToken);
}

// Retrieves and displays information about the current user
async Task GetSingleMember(Context<MessageActivity> context, CancellationToken cancellationToken)
{
    await context.SendAsync($"You are: {context.Activity.From?.Name}", cancellationToken);
}

// Mention a user in a message
async Task MentionUser(Context<MessageActivity> context, CancellationToken cancellationToken)
{
    var member = context.Activity.From;
    if (member is null)
    {
        return;
    }

    var mentionText = $"<at>{member.Name}</at>";
    var activity = new MessageActivityInput()
        .WithText($"Hello {mentionText}")
        .AddMention(member, addText: false);

    await context.SendAsync(activity, cancellationToken);
}

// Starts the Teams bot application and listens for incoming requests
webApp.Run();
