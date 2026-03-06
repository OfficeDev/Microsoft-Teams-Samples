// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Entities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();

var app = builder.Build();
var teams = app.UseTeams();

// Handle message events
teams.OnMessage(async context =>
{
    var text = context.Activity.Text?.Trim().ToLower() ?? "";

    if (text.Contains("label"))
        await SendAILabel(context);
    else if (text.Contains("feedback"))
        await SendFeedbackButtons(context);
    else if (text.Contains("sensitivity"))
        await SendSensitivityLabel(context);
    else if (text.Contains("citation"))
        await SendCitations(context);
    else if (text.Contains("aitext"))
        await SendAIMessage(context);
    else
        await SendWelcomeCard(context);
});

// Handle feedback invoke activities (thumbs up/down + text feedback)
teams.OnActivity(async context =>
{
    var activity = context.Activity;
    var activityType = activity.Type?.ToString() ?? "";

    if (!activityType.Equals("invoke", StringComparison.OrdinalIgnoreCase))
        return;

    var json = activity.ToString();
    using var doc = JsonDocument.Parse(json!);
    var root = doc.RootElement;

    if (!root.TryGetProperty("name", out var nameProp) ||
        nameProp.GetString() != "message/submitAction")
        return;

    var reaction = "No reaction";
    var feedbackText = "No feedback";

    if (root.TryGetProperty("value", out var valueProp) &&
        valueProp.TryGetProperty("actionValue", out var actionValue))
    {
        if (actionValue.TryGetProperty("reaction", out var reactionProp))
            reaction = reactionProp.GetString() ?? "No reaction";

        if (actionValue.TryGetProperty("feedback", out var feedbackProp))
        {
            var feedbackJsonStr = feedbackProp.GetString();
            if (!string.IsNullOrEmpty(feedbackJsonStr))
            {
                using var feedbackDoc = JsonDocument.Parse(feedbackJsonStr);
                if (feedbackDoc.RootElement.TryGetProperty("feedbackText", out var ftProp))
                    feedbackText = ftProp.GetString() ?? "No feedback";
            }
        }
    }

    await context.Send($"Provided reaction: {reaction}\nFeedback: {feedbackText}");
});

app.Run("http://localhost:3978");

// --- Helper methods ---

static async Task SendAILabel(IContext<MessageActivity> context)
{
    await context.Send(new MessageActivity
    {
        Text = "Hey, I'm a friendly AI bot. This message is generated via AI."
    }.AddAIGenerated());
}

static async Task SendFeedbackButtons(IContext<MessageActivity> context)
{
    await context.Send(new MessageActivity("This is an example for Feedback buttons that helps to provide feedback for a bot message.")
        .AddAIGenerated()
        .AddFeedback(true));
}

static async Task SendSensitivityLabel(IContext<MessageActivity> context)
{
    await context.Send(new MessageActivity
    {
        Text = "This is an example for sensitivity label that helps users identify the confidentiality of a message."
    }.AddAIGenerated()
     .AddSensitivityLabel("Confidential \\ Contoso FTE", "Please be mindful of sharing outside of your team", null));
}

static async Task SendCitations(IContext<MessageActivity> context)
{
    var message = new MessageActivity
    {
        Text = "Hey I'm a friendly AI bot. This message is generated through AI [1]"
    }.AddAIGenerated();

    message.AddCitation(1, new CitationAppearance
    {
        Name = "AI Bot",
        Url = "https://example.com/claim-1",
        Abstract = "Excerpt description",
        Keywords = new List<string> { "keyword 1", "keyword 2", "keyword 3" },
        UsageInfo = new SensitiveUsageEntity
        {
            Name = "Confidential \\ Contoso FTE",
            Description = "Only accessible to Contoso FTE"
        }
    });

    await context.Send(message);
}

static async Task SendAIMessage(IContext<MessageActivity> context)
{
    var message = new MessageActivity
    {
        Text = "Hey I'm a friendly AI bot. This message is generated via AI [1]"
    }
    .AddAIGenerated()
    .AddFeedback(true)
    .AddSensitivityLabel("Confidential \\ Contoso FTE", "Sensitivity description", null);

    message.AddCitation(1, new CitationAppearance
    {
        Name = "Some secret citation",
        Url = "https://example.com/claim-1",
        Abstract = "Excerpt",
        Keywords = new List<string> { "Keyword1 - 1", "Keyword1 - 2", "Keyword1 - 3" },
        UsageInfo = new SensitiveUsageEntity
        {
            Name = "Sensitivity title",
            Description = "Sensitivity description"
        }
    });

    await context.Send(message);
}

static async Task SendWelcomeCard(IContext<MessageActivity> context)
{
    await context.Send("Welcome to Bot AI!");
}