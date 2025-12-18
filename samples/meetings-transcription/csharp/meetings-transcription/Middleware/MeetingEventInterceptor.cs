using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using meetings_transcription.Helpers;
using meetings_transcription.Services;
using meetings_transcription.Models;
using meetings_transcription.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace meetings_transcription.Middleware
{
    /// <summary>
    /// Middleware to intercept meeting events and handle them BEFORE Teams SDK deserialization.
    /// This prevents JSON deserialization errors for meeting events with incomplete payloads.
    /// </summary>
    public class MeetingEventInterceptor
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MeetingEventInterceptor> _logger;

        public MeetingEventInterceptor(RequestDelegate next, ILogger<MeetingEventInterceptor> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IOptions<AzureSettings> azureSettings,
            ConcurrentDictionary<string, string> transcriptsDictionary,
            ICardFactory cardFactory)
        {
            // Only intercept POST requests to /api/messages
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

                    _logger.LogInformation($"Received activity: type={activityType}, name={activityName}");

                    // Intercept meeting events
                    if (activityType == "event")
                    {
                        if (activityName == "application/vnd.microsoft.meetingEnd")
                        {
                            _logger.LogInformation("Intercepting meeting end event");
                            await HandleMeetingEndEvent(jsonActivity, context, azureSettings, transcriptsDictionary);
                            return; // Don't pass to next middleware
                        }
                        else if (activityName == "application/vnd.microsoft.meetingStart")
                        {
                            _logger.LogInformation("Intercepting meeting start event");
                            // Return 200 OK to acknowledge
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{}");
                            return; // Don't pass to next middleware
                        }
                    }
                    // Intercept task/fetch
                    else if (activityType == "invoke" && activityName == "task/fetch")
                    {
                        _logger.LogInformation("Intercepting task/fetch");
                        await HandleTaskFetch(jsonActivity, context, azureSettings);
                        return; // Don't pass to next middleware
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"Failed to parse activity JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in meeting event interceptor: {ex.Message}");
                }
            }

            // Continue to next middleware
            await _next(context);
        }

        private async Task HandleMeetingEndEvent(
            JObject jsonActivity,
            HttpContext context,
            IOptions<AzureSettings> azureSettings,
            ConcurrentDictionary<string, string> transcriptsDictionary)
        {
            try
            {
                // Extract meeting ID from channelData
                var channelData = jsonActivity["channelData"];
                var meetingId = channelData?["meeting"]?["id"]?.ToString();

                if (string.IsNullOrEmpty(meetingId))
                {
                    _logger.LogWarning("Meeting ID not found in channel data");
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("{}");
                    return;
                }

                _logger.LogInformation($"Processing meeting end: encoded meetingId={meetingId}");

                // Log full event payload for debugging
                _logger.LogInformation($"?? Full event payload: {jsonActivity.ToString(Newtonsoft.Json.Formatting.None)}");

                // Extract meeting end time for transcript filtering
                DateTime? meetingEndTime = null;
                var endTimeValue = jsonActivity["value"]?["EndTime"]?.ToString();
                if (!string.IsNullOrEmpty(endTimeValue) && DateTime.TryParse(endTimeValue, out var parsedEndTime))
                {
                    meetingEndTime = parsedEndTime;
                    _logger.LogInformation($"Meeting end time extracted: {meetingEndTime}");
                }

                // MANDATORY DELAY: Wait 20 seconds after meeting ends before attempting to fetch transcript
                // This gives Microsoft's servers time to begin processing the transcript
                _logger.LogInformation("Waiting 20 seconds after meeting end before fetching transcript...");
                await Task.Delay(TimeSpan.FromSeconds(20));
                _logger.LogInformation("Initial delay complete. Starting transcript fetch attempts.");

                // Get meeting transcription  
                var graphHelper = new GraphHelper(azureSettings);
                
                // Retry logic: Transcript generation takes time after meeting ends
                // Add initial delay to give Microsoft servers time to process
                string result = null;
                int maxRetries = 6;
                int[] delaySeconds = { 10, 20, 40, 70, 110, 150 }; // Start with 10s delay, then progressive increases

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    _logger.LogInformation($"Waiting {delaySeconds[attempt]} seconds before attempt {attempt + 1}/{maxRetries}...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds[attempt]));

                    _logger.LogInformation($"Attempt {attempt + 1}/{maxRetries}: Fetching transcript using CallRecords API");
                    result = await graphHelper.GetMeetingTranscriptionsAsync(meetingId, null, meetingEndTime);

                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogInformation($"? Transcript found on attempt {attempt + 1}");
                        break;
                    }

                    if (attempt < maxRetries - 1)
                    {
                        _logger.LogInformation($"Transcript not ready yet (attempt {attempt + 1}/{maxRetries}). Will retry in {delaySeconds[attempt + 1]} seconds.");
                    }
                    else
                    {
                        _logger.LogInformation($"Transcript not ready after {maxRetries} attempts.");
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    // Clean up result
                    result = result.Replace("<v", "");

                    // Store transcript
                    transcriptsDictionary.AddOrUpdate(meetingId, result, (key, oldValue) => result);
                    _logger.LogInformation($"? Transcript stored for meeting {meetingId} ({result.Length} characters)");

                    // Extract conversation details for posting card
                    var conversationId = jsonActivity["conversation"]?["id"]?.ToString();
                    var serviceUrl = jsonActivity["serviceUrl"]?.ToString();
                    var tenantId = channelData?["tenant"]?["id"]?.ToString();

                    if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(serviceUrl))
                    {
                        _logger.LogInformation($"?? Attempting to post Adaptive Card to conversation {conversationId}");
                        await PostTranscriptCardToChat(
                            conversationId,
                            serviceUrl,
                            tenantId,
                            meetingId,
                            azureSettings.Value.MicrosoftAppId,
                            azureSettings.Value.MicrosoftAppPassword);
                    }
                    else
                    {
                        _logger.LogWarning($"?? Missing conversation details - cannot post card");
                        _logger.LogInformation($"?? Users can view the transcript through the bot's task module interface");
                    }
                }
                else
                {
                    _logger.LogWarning($"?? No transcript found after {maxRetries} attempts");
                }

                // Acknowledge the event
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling meeting end event: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"{{\"error\":\"{ex.Message}\"}}");
            }
        }

        private async Task PostTranscriptCardToChat(
            string conversationId,
            string serviceUrl,
            string tenantId,
            string meetingId,
            string appId,
            string appPassword)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Get OAuth token for Bot Framework API (use tenant-specific endpoint)
                _logger.LogInformation("?? Requesting Bot Framework OAuth token...");
                _logger.LogInformation($"?? Using tenant ID: {tenantId}");
                var tokenResponse = await httpClient.PostAsync(
                    $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", appId),
                        new KeyValuePair<string, string>("client_secret", appPassword),
                        new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default")
                    }));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"? Failed to obtain Bot Framework token: {tokenResponse.StatusCode} - {errorContent}");
                    return;
                }

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var token = JObject.Parse(tokenJson)["access_token"]?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("? Token response did not contain access_token");
                    return;
                }

                _logger.LogInformation("? Bot Framework token obtained successfully");

                // Create Adaptive Card payload
                var cardPayload = new
                {
                    type = "message",
                    from = new { id = appId },
                    conversation = new { id = conversationId },
                    channelData = new { tenant = new { id = tenantId } },
                    attachments = new[]
                    {
                        new
                        {
                            contentType = "application/vnd.microsoft.card.adaptive",
                            content = new
                            {
                                type = "AdaptiveCard",
                                version = "1.5",
                                body = new[]
                                {
                                    new
                                    {
                                        type = "TextBlock",
                                        text = "Here is the last transcript details of the meeting.",
                                        weight = "Bolder",
                                        size = "Large"
                                    }
                                },
                                actions = new[]
                                {
                                    new
                                    {
                                        type = "Action.Submit",
                                        title = "View Transcript",
                                        data = new
                                        {
                                            msteams = new { type = "task/fetch" },
                                            meetingId = meetingId
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // Send message to conversation
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var conversationUrl = $"{serviceUrl.TrimEnd('/')}/v3/conversations/{conversationId}/activities";
                _logger.LogInformation($"?? Posting card to: {conversationUrl}");

                var response = await httpClient.PostAsJsonAsync(conversationUrl, cardPayload);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"? Adaptive Card posted successfully to chat for meeting {meetingId}");
                    _logger.LogInformation($"?? Response: {responseContent}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"? Failed to post card: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error posting card to chat: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task HandleTaskFetch(
            JObject jsonActivity,
            HttpContext context,
            IOptions<AzureSettings> azureSettings)
        {
            try
            {
                // Extract meeting ID from value.data.meetingId
                var meetingId = jsonActivity["value"]?["data"]?["meetingId"]?.ToString();

                var taskModuleUrl = !string.IsNullOrEmpty(meetingId)
                    ? $"{azureSettings.Value.AppBaseUrl}/home?meetingId={meetingId}"
                    : $"{azureSettings.Value.AppBaseUrl}/home";

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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling task/fetch: {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"{{\"error\":\"{ex.Message}\"}}");
            }
        }
    }

    /// <summary>
    /// Extension method to register the middleware.
    /// </summary>
    public static class MeetingEventInterceptorExtensions
    {
        public static IApplicationBuilder UseMeetingEventInterceptor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MeetingEventInterceptor>();
        }
    }
}
