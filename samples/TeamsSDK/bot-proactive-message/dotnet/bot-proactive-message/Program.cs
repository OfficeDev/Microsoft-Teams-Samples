// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();

var webApp = builder.Build();
var teams = webApp.UseTeams();

// This would be some persistent storage in a real app. It maps a user's
// Microsoft Entra object id to the conversation id we can message them on.
var conversationIdStore = new ConcurrentDictionary<string, string>();

// Installation is just one place to get the conversation id. Every activity
// carries the conversation id, so any handler can capture it.
teams.OnInstall(async (context, cancellationToken) =>
{
    RememberConversation(context.Activity.From.AadObjectId, context.Activity.Conversation.Id);

    await context.Send("Hi! I am going to remind you to say something to me soon!", cancellationToken);

    // Queue up a proactive notification to be sent in 10 seconds.
    ScheduleProactiveNotification(context.Activity.From.AadObjectId, TimeSpan.FromSeconds(10));
});

teams.OnMessage(async (context, cancellationToken) =>
{
    var userId = context.Activity.From.AadObjectId;
    RememberConversation(userId, context.Activity.Conversation.Id);

    var text = context.Activity.Text?.Trim().ToLowerInvariant() ?? string.Empty;

    if (text.Contains("remind"))
    {
        await context.Send("Got it. I will send you a proactive message in 10 seconds.", cancellationToken);
        ScheduleProactiveNotification(userId, TimeSpan.FromSeconds(10));
    }
    else if (text.Contains("notify"))
    {
        await SendProactiveNotification(userId);
    }
    else
    {
        await context.Send(
            "Welcome to the proactive message bot! Send 'notify' to receive a proactive message right away, " +
            "or 'remind' to receive one in 10 seconds.",
            cancellationToken);
    }
});

// Saves the conversation id so it can be used for proactive messaging later.
void RememberConversation(string? userId, string conversationId)
{
    if (string.IsNullOrEmpty(userId))
    {
        return;
    }

    conversationIdStore[userId] = conversationId;
}

// A stand-in for a real notification queue / background worker.
void ScheduleProactiveNotification(string? userId, TimeSpan delay)
{
    if (string.IsNullOrEmpty(userId))
    {
        return;
    }

    _ = Task.Run(async () =>
    {
        try
        {
            await Task.Delay(delay);
            await SendProactiveNotification(userId);
        }
        catch (Exception error)
        {
            Console.WriteLine($"[PROACTIVE] Failed to deliver notification: {error.Message}");
        }
    });
}

// Retrieve the conversation id from storage and use it to send the message.
async Task SendProactiveNotification(string? userId)
{
    if (string.IsNullOrEmpty(userId) || !conversationIdStore.TryGetValue(userId, out var conversationId))
    {
        return;
    }

    await teams.Send(conversationId, new MessageActivity("Hey! It's been a while. How are you?"));
}

webApp.Run();
