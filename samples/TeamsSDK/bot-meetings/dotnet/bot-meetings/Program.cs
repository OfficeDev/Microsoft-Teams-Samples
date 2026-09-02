// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Meetings;
using Microsoft.Teams.Apps.Schema;
using Microsoft.Teams.Cards;

// Initialize the Teams bot application - auth comes from the AzureAd/BotFramework configuration sections
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTeamsBotApplication();
var webApp = builder.Build();
var teamsApp = webApp.UseTeamsBotApplication();

// AZURE_* credentials must be set to use secrets.
Environment.SetEnvironmentVariable("AZURE_TENANT_ID", builder.Configuration["AzureAd:TenantId"] ?? "");
Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", builder.Configuration["AzureAd:ClientId"] ?? "");
Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", builder.Configuration["AzureAd:ClientCredentials:0:ClientSecret"] ?? "");

var credential = new DefaultAzureCredential();
var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

// Resolve the Graph onlineMeeting id from the meeting's join URL
async Task<string> GetOnlineMeetingIdAsync(string userId, string joinWebUrl)
{
    var escapedJoinUrl = joinWebUrl.Replace("'", "''", StringComparison.Ordinal);

    var meetings = await graphClient.Users[userId]
        .OnlineMeetings
        .GetAsync(config =>
        {
            config.QueryParameters.Filter = $"JoinWebUrl eq '{escapedJoinUrl}'";
        });

    return meetings?.Value?.FirstOrDefault()?.Id ?? string.Empty;
}

// Method to retrieve meeting transcript
async Task<string> GetMeetingTranscriptAsync(string meetingResourceId, string userId)
{
    // Retrieve metadata for all the transcripts
    var transcriptsMetadata = await graphClient.Users[userId]
        .OnlineMeetings[meetingResourceId]
        .Transcripts
        .GetAsync();

    if (transcriptsMetadata?.Value == null || transcriptsMetadata.Value.Count == 0)
    {
        return string.Empty;
    }

    // Get the latest transcript
    var latestTranscript = transcriptsMetadata.Value
        .OrderByDescending(t => t.CreatedDateTime)
        .FirstOrDefault();

    if (latestTranscript?.Id == null)
    {
        return string.Empty;
    }

    var transcriptId = latestTranscript.Id;

    // Retrieve the transcript content in VTT format
    var content = await graphClient.Users[userId]
        .OnlineMeetings[meetingResourceId]
        .Transcripts[transcriptId]
        .Content
        .GetAsync(requestConfiguration: config =>
        {
            config.Headers.Add("Accept", "text/vtt");
        });

    if (content == null)
    {
        return string.Empty;
    }

    using var reader = new StreamReader(content);
    return await reader.ReadToEndAsync();
}

// Convert a WebVTT transcript to 'Speaker: text' lines
string ParseVtt(string vtt)
{
    var lines = new List<string>();
    var vttLines = vtt.Split('\n');

    foreach (var line in vttLines)
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrEmpty(trimmedLine) || 
            trimmedLine.StartsWith("WEBVTT") || 
            trimmedLine.Contains("-->"))
        {
            continue;
        }

        // Replace <v Speaker Name>text with Speaker Name: text
        var processedLine = Regex.Replace(trimmedLine, @"<v ([^>]+)>", "$1: ");
        
        // Strip any remaining VTT tags like </v>, <c>, etc.
        processedLine = Regex.Replace(processedLine, @"<[^>]+>", "").Trim();

        if (!string.IsNullOrEmpty(processedLine))
        {
            lines.Add(processedLine);
        }
    }

    return string.Join("\n", lines);
}


// Serialize an adaptive card into an attachment that can be sent with a message activity
MessageActivityInput BuildCardMessage(AdaptiveCard card) =>
    new MessageActivityInput().WithAdaptiveCardAttachment(JsonSerializer.SerializeToElement(card));

// Register meeting participant join handler
teamsApp.OnMeetingJoin(async (context, cancellationToken) =>
{
    var activity = context.Activity.Value;
    if (activity is null || string.IsNullOrEmpty(activity.Members[0].User?.AadObjectId)) return;

    var member = activity.Members[0].User.Name;
    var role = activity.Members[0].Meeting?.Role ?? "a participant";

    var card = new AdaptiveCard
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = new List<CardElement>
        {
            new TextBlock($"{member} has joined the meeting as {role}.")
            {
                Wrap = true,
                Weight = TextWeight.Bolder
            }
        }
    };

    await context.SendAsync(BuildCardMessage(card), cancellationToken);
});

// Register meeting start handler
teamsApp.OnMeetingStart(async (context, cancellationToken) =>
{
    var activity = context.Activity.Value;
    if (activity is null) return;

    var card = new AdaptiveCard
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = new List<CardElement>
        {
            new TextBlock("The meeting has started.")
            {
                Wrap = true,
                Weight = TextWeight.Bolder,
                Size = TextSize.Large
            },
            new TextBlock($"**Title:** {activity.Title}")
            {
                Wrap = true
            },
            new TextBlock($"**Start Time:** {activity.StartTime}")
            {
                Wrap = true
            }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new OpenUrlAction(activity.JoinUrl?.ToString() ?? string.Empty)
            {
                Title = "Join Meeting"
            }
        }
    };

    await context.SendAsync(BuildCardMessage(card), cancellationToken);
});

// Register meeting end handler with transcript support
teamsApp.OnMeetingEnd(async (context, cancellationToken) =>
{
    var activity = context.Activity.Value;
    if (activity is null) return;

    var meetingId = activity.Id;

    // Get meeting info from API
    var meetingInfo = await context.Api.Meetings.GetByIdAsync(meetingId, cancellationToken);

    // Retrieve the user ID of the organizer for the transcript API
    var userId = TeamsChannelAccount.FromChannelAccount(meetingInfo?.Organizer)?.AadObjectId ?? "";

    // Look up the Graph onlineMeeting that matches this meeting's join URL
    var joinUrl = (activity.JoinUrl ?? meetingInfo?.Details?.JoinUrl)?.ToString();
    var msGraphResourceId = "";
    if (!string.IsNullOrEmpty(joinUrl) && !string.IsNullOrEmpty(userId))
    {
        msGraphResourceId = await GetOnlineMeetingIdAsync(userId, joinUrl);
    }

    // Wait 30 seconds for the transcript to become available
    await Task.Delay(30000, cancellationToken);

    // Retrieve transcript
    var transcript = "";
    if (!string.IsNullOrEmpty(msGraphResourceId) && !string.IsNullOrEmpty(userId))
    {
        var vttTranscript = await GetMeetingTranscriptAsync(msGraphResourceId, userId);
        if (!string.IsNullOrEmpty(vttTranscript))
        {
            transcript = ParseVtt(vttTranscript);
        }
    }

    // Build card body with transcript
    var cardBody = new List<CardElement>
    {
        new TextBlock("The meeting has ended.")
        {
            Wrap = true,
            Weight = TextWeight.Bolder,
            Size = TextSize.Large
        },
        new TextBlock($"**End Time:** {activity.EndTime}")
        {
            Wrap = true
        },
        new TextBlock("**Transcript:**")
        {
            Wrap = true,
            Weight = TextWeight.Bolder
        }
    };

    // Add transcript lines or fallback message
    if (!string.IsNullOrEmpty(transcript))
    {
        var transcriptLines = transcript.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in transcriptLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                cardBody.Add(new TextBlock(line) { Wrap = true });
            }
        }
    }
    else
    {
        cardBody.Add(new TextBlock("Transcript not available for this meeting.") { Wrap = true });
    }

    var card = new AdaptiveCard
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = cardBody
    };

    await context.SendAsync(BuildCardMessage(card), cancellationToken);
});

// Register meeting participant leave handler
teamsApp.OnMeetingLeave(async (context, cancellationToken) =>
{
    var activity = context.Activity.Value;
    if (activity is null) return;

    var member = activity.Members[0].User.Name;

    var card = new AdaptiveCard
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = new List<CardElement>
        {
            new TextBlock($"{member} has left the meeting.")
            {
                Wrap = true,
                Weight = TextWeight.Bolder
            }
        }
    };

    await context.SendAsync(BuildCardMessage(card), cancellationToken);
});

// Starts the Teams bot application and listens for incoming requests
webApp.Run();
