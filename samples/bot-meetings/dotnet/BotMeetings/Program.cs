// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<TeamsSettings>(builder.Configuration.GetSection("Teams"));

// Add transcript storage
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Get services
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("BotMeetings");
var transcriptsDictionary = app.Services.GetRequiredService<ConcurrentDictionary<string, string>>();
var meetingStartTimes = new ConcurrentDictionary<string, DateTime>();
var teamsSettingsOptions = app.Services.GetRequiredService<IOptions<TeamsSettings>>();

// MIDDLEWARE: Intercept meeting events BEFORE they reach the endpoint

app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/messages"))
    {
        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        try
        {
            var jsonActivity = JObject.Parse(body);
            var activityType = jsonActivity["type"]?.ToString();
            var activityName = jsonActivity["name"]?.ToString();

            // Intercept meeting events
            if (activityType == "event")
            {
                if (activityName == "application/vnd.microsoft.meetingEnd")
                {

                    // --- Meeting End Card ---
                    var meetingEndValue = jsonActivity["value"];
                    var conversationIdEnd = jsonActivity["conversation"]?["id"]?.ToString();
                    var serviceUrlEnd = jsonActivity["serviceUrl"]?.ToString();
                    var tenantIdEnd = jsonActivity["channelData"]?["tenant"]?["id"]?.ToString();
                    var titleEnd = meetingEndValue?["Title"]?.ToString() ?? "Meeting";
                    var endTimeStr = meetingEndValue?["EndTime"]?.ToString();

                    // Calculate duration
                    var meetingDurationText = "N/A";
                    if (!string.IsNullOrEmpty(conversationIdEnd) &&
                        meetingStartTimes.TryRemove(conversationIdEnd, out var storedStartTime) &&
                        !string.IsNullOrEmpty(endTimeStr) &&
                        DateTime.TryParse(endTimeStr, out var endTimeParsed))
                    {
                        var duration = endTimeParsed - storedStartTime;
                        meetingDurationText = duration.TotalMinutes >= 1
                            ? $"{(int)duration.TotalMinutes}min {duration.Seconds}s"
                            : $"{(int)duration.TotalSeconds}s";
                    }

                    var endCard = GetAdaptiveCardForMeetingEnd(titleEnd, endTimeStr ?? "", meetingDurationText);
                    if (!string.IsNullOrEmpty(conversationIdEnd) && !string.IsNullOrEmpty(serviceUrlEnd))
                    {
                        await SendAdaptiveCardToConversationAsync(conversationIdEnd, serviceUrlEnd, tenantIdEnd, endCard, teamsSettingsOptions.Value, logger);
                    }

                    // --- Transcript fetch ---
                    var channelData = jsonActivity["channelData"];
                    var meetingId = channelData?["meeting"]?["id"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(meetingId))
                    {
                        DateTime? meetingEndTime = null;
                        var endTimeValue = jsonActivity["value"]?["EndTime"]?.ToString();
                        if (!string.IsNullOrEmpty(endTimeValue) && DateTime.TryParse(endTimeValue, out var parsed))
                        {
                            meetingEndTime = parsed;
                        }

                        _ = Task.Run(async () =>
                        {
                            await FetchAndStoreTranscriptAsync(
                                meetingId,
                                meetingEndTime,
                                jsonActivity["conversation"]?["id"]?.ToString(),
                                jsonActivity["serviceUrl"]?.ToString(),
                                channelData?["tenant"]?["id"]?.ToString(),
                                teamsSettingsOptions.Value,
                                transcriptsDictionary,
                                logger);
                        });
                    }
                    
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{}");
                    return;
                }
                else if (activityName == "application/vnd.microsoft.meetingStart")
                {

                    var meetingStartValue = jsonActivity["value"];
                    var conversationIdStart = jsonActivity["conversation"]?["id"]?.ToString();
                    var serviceUrlStart = jsonActivity["serviceUrl"]?.ToString();
                    var tenantIdStart = jsonActivity["channelData"]?["tenant"]?["id"]?.ToString();

                    // Store start time for duration calculation
                    var startTimeStr = meetingStartValue?["StartTime"]?.ToString();
                    if (!string.IsNullOrEmpty(conversationIdStart) &&
                        !string.IsNullOrEmpty(startTimeStr) &&
                        DateTime.TryParse(startTimeStr, out var startTimeParsed))
                    {
                        meetingStartTimes[conversationIdStart] = startTimeParsed;
                    }

                    // Send meeting start card
                    var titleStart = meetingStartValue?["Title"]?.ToString() ?? "Meeting";
                    var joinUrl = meetingStartValue?["JoinUrl"]?.ToString();
                    var startCard = GetAdaptiveCardForMeetingStart(titleStart, startTimeStr ?? "", joinUrl);

                    if (!string.IsNullOrEmpty(conversationIdStart) && !string.IsNullOrEmpty(serviceUrlStart))
                    {
                        await SendAdaptiveCardToConversationAsync(conversationIdStart, serviceUrlStart, tenantIdStart, startCard, teamsSettingsOptions.Value, logger);
                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{}");
                    return;
                }
                else if (activityName == "application/vnd.microsoft.meetingParticipantJoin")
                {
                    var membersJoin = jsonActivity["value"]?["members"] as JArray;
                    if (membersJoin != null)
                    {
                        foreach (var member in membersJoin)
                        {
                            var userName = member?["user"]?["name"]?.ToString();
                            // Skip participants with no name (e.g., transcription service bots)
                            if (string.IsNullOrEmpty(userName) || userName == "Unknown")
                                continue;

                            var convId = jsonActivity["conversation"]?["id"]?.ToString();
                            var svcUrl = jsonActivity["serviceUrl"]?.ToString();
                            var tId = jsonActivity["channelData"]?["tenant"]?["id"]?.ToString();

                            var joinCard = GetAdaptiveCardForParticipantEvent(userName, " has joined the meeting.");
                            if (!string.IsNullOrEmpty(convId) && !string.IsNullOrEmpty(svcUrl))
                            {
                                await SendAdaptiveCardToConversationAsync(convId, svcUrl, tId, joinCard, teamsSettingsOptions.Value, logger);
                            }
                        }
                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{}");
                    return;
                }
                else if (activityName == "application/vnd.microsoft.meetingParticipantLeave")
                {
                    var membersLeave = jsonActivity["value"]?["members"] as JArray;
                    if (membersLeave != null)
                    {
                        foreach (var member in membersLeave)
                        {
                            var userName = member?["user"]?["name"]?.ToString();
                            // Skip participants with no name (e.g., transcription service bots)
                            if (string.IsNullOrEmpty(userName) || userName == "Unknown")
                                continue;

                            var convId = jsonActivity["conversation"]?["id"]?.ToString();
                            var svcUrl = jsonActivity["serviceUrl"]?.ToString();
                            var tId = jsonActivity["channelData"]?["tenant"]?["id"]?.ToString();

                            var leaveCard = GetAdaptiveCardForParticipantEvent(userName, " left the meeting.");
                            if (!string.IsNullOrEmpty(convId) && !string.IsNullOrEmpty(svcUrl))
                            {
                                await SendAdaptiveCardToConversationAsync(convId, svcUrl, tId, leaveCard, teamsSettingsOptions.Value, logger);
                            }
                        }
                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{}");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error in meeting event interceptor: {ex.Message}");
        }
    }

    await next(context);
});

// Transcript view page endpoint
app.MapGet("/home", (HttpContext context) =>
{
    var meetingId = context.Request.Query["meetingId"].ToString();
    var transcriptContent = "Transcript not found.";

    if (!string.IsNullOrEmpty(meetingId) && transcriptsDictionary.TryGetValue(meetingId, out var transcript))
    {
        transcriptContent = transcript;
    }
    else if (!string.IsNullOrEmpty(meetingId))
    {
        logger.LogWarning($"Transcript not found for meetingId '{meetingId}'");
    }

    var encodedContent = System.Web.HttpUtility.HtmlEncode(transcriptContent);
    
    var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Meeting Transcript</title>
    <script src=""https://statics.teams.cdn.office.net/sdk/v1.11.0/js/MicrosoftTeams.min.js""></script>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            padding: 20px;
            margin: 0;
            background-color: #f5f5f5;
        }
        h2 {
            color: #333;
            margin-bottom: 15px;
        }
        #transcription {
            white-space: pre-wrap;
            word-wrap: break-word;
            background-color: #fff;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 15px;
            max-height: 450px;
            overflow-y: auto;
            font-size: 14px;
            line-height: 1.5;
        }
    </style>
</head>
<body>
    <h2>Transcription Details</h2>
    <pre id=""transcription"">" + encodedContent + @"</pre>
    <script>
        microsoftTeams.initialize();
    </script>
</body>
</html>";

    context.Response.ContentType = "text/html";
    return context.Response.WriteAsync(html);
});

// Bot messages endpoint
app.MapPost("/api/messages", async (HttpContext context) =>
{
    var teamsSettings = teamsSettingsOptions.Value;
    
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    try
    {
        var activity = JObject.Parse(body);
        var activityType = activity["type"]?.ToString();
        var activityName = activity["name"]?.ToString();

        // Meeting events are handled by middleware above - skip them here
        if (activityType == "event")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{}");
            return;
        }
        // Handle task/fetch invoke
        else if (activityType == "invoke" && activityName == "task/fetch")
        {
            var meetingId = activity["value"]?["data"]?["meetingId"]?.ToString();
            
            // Fallback: try value.meetingId directly
            if (string.IsNullOrEmpty(meetingId))
                meetingId = activity["value"]?["meetingId"]?.ToString();
            
            var taskModuleUrl = !string.IsNullOrEmpty(meetingId)
                ? $"{teamsSettings.AppBaseUrl}/home?meetingId={Uri.EscapeDataString(meetingId)}"
                : $"{teamsSettings.AppBaseUrl}/home";

            var response = new
            {
                task = new
                {
                    type = "continue",
                    value = new
                    {
                        title = "Meeting Transcript",
                        height = 600,
                        width = 600,
                        url = taskModuleUrl
                    }
                }
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
            return;
        }
        // Handle regular messages
        else if (activityType == "message")
        {
            var text = activity["text"]?.ToString()?.ToLowerInvariant() ?? "";
            var conversationId = activity["conversation"]?["id"]?.ToString();
            var serviceUrl = activity["serviceUrl"]?.ToString();
            var tenantId = activity["channelData"]?["tenant"]?["id"]?.ToString();

            string replyText;
            if (text.Contains("transcript") || text.Contains("help"))
            {
                replyText = "I'm the Bot Meetings sample!\n\n" +
                    "I automatically capture meeting transcripts when transcription is enabled.\n\n" +
                    "After a meeting ends, I'll post a card that lets you view the transcript.";
            }
            else
            {
                replyText = $"You said: {activity["text"]}\n\nType 'help' to learn about this bot.";
            }

            // Send reply
            if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(serviceUrl))
            {
                await SendReplyAsync(conversationId, serviceUrl, tenantId, replyText, teamsSettings, logger);
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{}");
            return;
        }

        // Default response
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{}");
    }
    catch (Exception ex)
    {
        logger.LogError($"Error processing activity: {ex.Message}");
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("{}");
    }
});

app.Run();

// Helper Methods

async Task SendReplyAsync(string conversationId, string serviceUrl, string? tenantId, string text, TeamsSettings settings, ILogger logger)
{
    try
    {
        using var client = new HttpClient();
        var token = await GetBotFrameworkTokenAsync(tenantId, settings, client);
        if (string.IsNullOrEmpty(token)) return;

        var payload = new
        {
            type = "message",
            from = new { id = settings.ClientId },
            conversation = new { id = conversationId },
            text
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await client.PostAsJsonAsync($"{serviceUrl.TrimEnd('/')}/v3/conversations/{conversationId}/activities", payload);
    }
    catch (Exception ex)
    {
        logger.LogError($"Error sending reply: {ex.Message}");
    }
}

async Task FetchAndStoreTranscriptAsync(string meetingId, DateTime? meetingEndTime, string? conversationId, string? serviceUrl, string? tenantId, TeamsSettings settings, ConcurrentDictionary<string, string> transcripts, ILogger logger)
{
    await Task.Delay(TimeSpan.FromSeconds(20));

    int[] delaySeconds = { 10, 20, 40, 70, 110, 150 };
    string? result = null;

    for (int attempt = 0; attempt < delaySeconds.Length; attempt++)
    {
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds[attempt]));

        result = await GetMeetingTranscriptAsync(meetingId, meetingEndTime, settings, logger);

        if (!string.IsNullOrEmpty(result))
        {
            logger.LogInformation($"Transcript found on attempt {attempt + 1}");
            break;
        }
    }

    if (!string.IsNullOrEmpty(result))
    {
        result = result.Replace("<v", "");
        transcripts.AddOrUpdate(meetingId, result, (_, _) => result);
        logger.LogInformation($"Transcript stored ({result.Length} chars)");

        if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(serviceUrl))
        {
            await PostTranscriptCardAsync(conversationId, serviceUrl, tenantId, meetingId, settings, logger);
        }
    }
    else
    {
        logger.LogWarning("No transcript found after all attempts");
    }
}

async Task<string?> GetBotFrameworkTokenAsync(string? tenantId, TeamsSettings settings, HttpClient client)
{
    var tokenResponse = await client.PostAsync(
        $"https://login.microsoftonline.com/{tenantId ?? settings.TenantId}/oauth2/v2.0/token",
        new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", settings.ClientId),
            new KeyValuePair<string, string>("client_secret", settings.ClientSecret),
            new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default")
        }));

    if (!tokenResponse.IsSuccessStatusCode) return null;
    
    var json = await tokenResponse.Content.ReadAsStringAsync();
    return JObject.Parse(json)["access_token"]?.ToString();
}

async Task<string?> GetMeetingTranscriptAsync(string encodedMeetingId, DateTime? meetingEndTime, TeamsSettings settings, ILogger logger)
{
    try
    {
        var confidentialClient = ConfidentialClientApplicationBuilder
            .Create(settings.ClientId)
            .WithClientSecret(settings.ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{settings.TenantId}")
            .Build();

        var tokenResult = await confidentialClient
            .AcquireTokenForClient(["https://graph.microsoft.com/.default"])
            .ExecuteAsync();

        var accessToken = tokenResult.AccessToken;
        var client = new HttpClient();

        var decodedBytes = Convert.FromBase64String(encodedMeetingId);
        var decodedId = Encoding.UTF8.GetString(decodedBytes);
        
        var threadMatch = Regex.Match(decodedId, @"19:meeting_[^@]+@thread\.v2");
        if (!threadMatch.Success)
        {
            logger.LogWarning($"Could not extract thread ID from: {decodedId}");
            return null;
        }
        
        var threadId = threadMatch.Value;

        var callRecordsReq = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/communications/callRecords");
        callRecordsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var callRecordsResponse = await client.SendAsync(callRecordsReq);
        if (!callRecordsResponse.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to get call records: {await callRecordsResponse.Content.ReadAsStringAsync()}");
            return null;
        }

        var callRecords = JObject.Parse(await callRecordsResponse.Content.ReadAsStringAsync())["value"];
        if (callRecords == null) return null;

        foreach (var record in callRecords)
        {
            var callId = record["id"]?.ToString();
            if (string.IsNullOrEmpty(callId)) continue;

            var detailsReq = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/v1.0/communications/callRecords/{callId}");
            detailsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var detailsResponse = await client.SendAsync(detailsReq);
            if (!detailsResponse.IsSuccessStatusCode) continue;

            var details = JObject.Parse(await detailsResponse.Content.ReadAsStringAsync());
            var joinUrl = details["joinWebUrl"]?.ToString();
            if (string.IsNullOrEmpty(joinUrl)) continue;

            if (!Uri.UnescapeDataString(joinUrl).Contains(threadId.Replace("@thread.v2", ""))) continue;

            var organizerId = details["organizer"]?["user"]?["id"]?.ToString();
            if (string.IsNullOrEmpty(organizerId)) continue;

            var meetingsReq = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/beta/users/{organizerId}/onlineMeetings?$filter=JoinWebUrl eq '{Uri.EscapeDataString(joinUrl)}'");
            meetingsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var meetingsResponse = await client.SendAsync(meetingsReq);
            if (!meetingsResponse.IsSuccessStatusCode)
            {
                logger.LogError($"Failed to get meetings: {await meetingsResponse.Content.ReadAsStringAsync()}");
                continue;
            }

            var meetings = JObject.Parse(await meetingsResponse.Content.ReadAsStringAsync())["value"];
            var meeting = meetings?.FirstOrDefault(m => m["joinWebUrl"]?.ToString() == joinUrl);
            if (meeting == null) continue;

            var graphMeetingId = meeting["id"]?.ToString();

            var transcriptsReq = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/beta/users/{organizerId}/onlineMeetings/{graphMeetingId}/transcripts");
            transcriptsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var transcriptsResponse = await client.SendAsync(transcriptsReq);
            if (!transcriptsResponse.IsSuccessStatusCode) return null;

            var transcriptsArray = JObject.Parse(await transcriptsResponse.Content.ReadAsStringAsync())["value"] as JArray;
            if (transcriptsArray == null || !transcriptsArray.Any()) return null;

            var targetTranscript = transcriptsArray.OrderByDescending(t => t["createdDateTime"]?.ToObject<DateTime>()).FirstOrDefault();
            if (targetTranscript == null) return null;

            var contentReq = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/beta/users/{organizerId}/onlineMeetings/{graphMeetingId}/transcripts/{targetTranscript["id"]}/content?$format=text/vtt");
            contentReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var contentResponse = await client.SendAsync(contentReq);
            if (contentResponse.IsSuccessStatusCode)
                return await contentResponse.Content.ReadAsStringAsync();

            break;
        }

        return null;
    }
    catch (Exception ex)
    {
        logger.LogError($"Error fetching transcript: {ex.Message}");
        return null;
    }
}

async Task PostTranscriptCardAsync(string conversationId, string serviceUrl, string? tenantId, string meetingId, TeamsSettings settings, ILogger logger)
{
    try
    {
        using var client = new HttpClient();
        var token = await GetBotFrameworkTokenAsync(tenantId, settings, client);
        if (string.IsNullOrEmpty(token)) return;

        var cardPayload = new
        {
            type = "message",
            from = new { id = settings.ClientId },
            conversation = new { id = conversationId },
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new
                    {
                        type = "AdaptiveCard",
                        version = "1.5",
                        body = new object[]
                        {
                            new { type = "TextBlock", text = "Meeting Transcript Ready", weight = "Bolder", size = "Large" },
                            new { type = "TextBlock", text = "Click below to view the transcript.", wrap = true }
                        },
                        actions = new[]
                        {
                            new
                            {
                                type = "Action.Submit",
                                title = "View Transcript",
                                data = new { msteams = new { type = "task/fetch" }, meetingId }
                            }
                        }
                    }
                }
            }
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync($"{serviceUrl.TrimEnd('/')}/v3/conversations/{conversationId}/activities", cardPayload);

        if (response.IsSuccessStatusCode)
            logger.LogInformation("Transcript card posted to chat");
        else
            logger.LogError($"Failed to post card: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        logger.LogError($"Error posting card: {ex.Message}");
    }
}

// Meeting Events: Adaptive Card Builders

object GetAdaptiveCardForMeetingStart(string title, string startTime, string? joinUrl)
{
    var card = new
    {
        type = "AdaptiveCard",
        version = "1.4",
        body = new object[]
        {
            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = $"{title} - started" },
            new
            {
                type = "ColumnSet",
                spacing = "Medium",
                columns = new object[]
                {
                    new
                    {
                        type = "Column",
                        width = "1",
                        items = new object[]
                        {
                            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = "Start Time : " }
                        }
                    },
                    new
                    {
                        type = "Column",
                        width = "3",
                        items = new object[]
                        {
                            new { type = "TextBlock", size = "Medium", text = !string.IsNullOrEmpty(startTime) && DateTime.TryParse(startTime, out var st) ? st.ToLocalTime().ToString() : startTime }
                        }
                    }
                }
            }
        },
        actions = !string.IsNullOrEmpty(joinUrl)
            ? new object[] { new { type = "Action.OpenUrl", title = "Join meeting", url = joinUrl } }
            : Array.Empty<object>()
    };
    return card;
}

object GetAdaptiveCardForMeetingEnd(string title, string endTime, string meetingDurationText)
{
    var card = new
    {
        type = "AdaptiveCard",
        version = "1.4",
        body = new object[]
        {
            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = $"{title} - ended" },
            new
            {
                type = "ColumnSet",
                spacing = "Medium",
                columns = new object[]
                {
                    new
                    {
                        type = "Column",
                        width = "1",
                        items = new object[]
                        {
                            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = "End Time : " },
                            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = "Total Duration : " }
                        }
                    },
                    new
                    {
                        type = "Column",
                        width = "3",
                        items = new object[]
                        {
                            new { type = "TextBlock", size = "Medium", text = !string.IsNullOrEmpty(endTime) && DateTime.TryParse(endTime, out var et) ? et.ToLocalTime().ToString() : endTime },
                            new { type = "TextBlock", size = "Medium", text = meetingDurationText }
                        }
                    }
                }
            }
        }
    };
    return card;
}

object GetAdaptiveCardForParticipantEvent(string userName, string action)
{
    var card = new
    {
        type = "AdaptiveCard",
        version = "1.4",
        body = new object[]
        {
            new
            {
                type = "RichTextBlock",
                spacing = "Medium",
                inlines = new object[]
                {
                    new { type = "TextRun", text = userName, weight = "Bolder", size = "Default" },
                    new { type = "TextRun", text = action, weight = "Default", size = "Default" }
                }
            }
        }
    };
    return card;
}

async Task SendAdaptiveCardToConversationAsync(string conversationId, string serviceUrl, string? tenantId, object cardContent, TeamsSettings settings, ILogger logger)
{
    try
    {
        using var client = new HttpClient();
        var token = await GetBotFrameworkTokenAsync(tenantId, settings, client);
        if (string.IsNullOrEmpty(token)) return;

        var payload = new
        {
            type = "message",
            from = new { id = settings.ClientId },
            conversation = new { id = conversationId },
            attachments = new[]
            {
                new { contentType = "application/vnd.microsoft.card.adaptive", content = cardContent }
            }
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync($"{serviceUrl.TrimEnd('/')}/v3/conversations/{conversationId}/activities", payload);

        if (response.IsSuccessStatusCode)
            logger.LogInformation("Meeting event card posted to chat");
        else
            logger.LogError($"Failed to post meeting event card: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }
    catch (Exception ex)
    {
        logger.LogError($"Error sending meeting event card: {ex.Message}");
    }
}

// Configuration

public class TeamsSettings
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string AppBaseUrl { get; set; } = "";
}