// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Events;
using Microsoft.Teams.Cards;

// Initialize Teams App - automatically uses CLIENT_ID, CLIENT_SECRET, and TENANT_ID from environment variables
var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

// Authenticate the app, needed for the transcript API
var tenantId = builder.Configuration["Teams:TenantId"] ?? Environment.GetEnvironmentVariable("TENANT_ID") ?? "";
var clientId = builder.Configuration["Teams:ClientId"] ?? Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
var clientSecret = builder.Configuration["Teams:ClientSecret"] ?? Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";

var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
var graphClient = new GraphServiceClient(credential);

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


// Register meeting participant join handler
teamsApp.OnMeetingJoin(async context =>
{
    var activity = context.Activity.Value;
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

    await context.Send(card);
});

// Register meeting start handler
teamsApp.OnMeetingStart(async context =>
{
    var activity = context.Activity.Value;

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
            new OpenUrlAction(activity.JoinUrl)
            {
                Title = "Join Meeting"
            }
        }
    };

    await context.Send(card);
});

// Register meeting end handler with transcript support
teamsApp.OnMeetingEnd(async context =>
{
    var activity = context.Activity.Value;
    var meetingId = activity.Id;
    
    // Get meeting info from API
    var meetingInfo = await context.Api.Meetings.GetByIdAsync(meetingId);

    // Retrieve the user ID of the organizer for the transcript API
    var userId = "";
    if (meetingInfo?.Organizer != null)
    {
        userId = meetingInfo.Organizer.AadObjectId ?? "";
    }

    // Get MS Graph Resource ID from meeting details
    var msGraphResourceId = meetingInfo?.Details?.MSGraphResourceId;

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

    await context.Send(card);
});

// Register meeting participant leave handler
teamsApp.OnMeetingLeave(async context =>
{
    var activity = context.Activity.Value;
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

    await context.Send(card);
});

// Starts the Teams bot application and listens for incoming requests
webApp.Run();
