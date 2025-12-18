// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace meetings_transcription.Helpers
{
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using meetings_transcription.Models.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Net.Http;
    using meetings_transcription.Models;
    using Newtonsoft.Json.Linq;

    public class GraphHelper
    {
        private readonly IOptions<AzureSettings> azureSettings;

        public GraphHelper(IOptions<AzureSettings> azureSettings)
        {
            this.azureSettings = azureSettings;
        }

        public async Task<string> GetToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(this.azureSettings.Value.MicrosoftAppId)
                .WithClientSecret(this.azureSettings.Value.MicrosoftAppPassword)
                .WithAuthority($"https://login.microsoftonline.com/{this.azureSettings.Value.MicrosoftAppTenantId}")
                .WithRedirectUri("https://daemon")
                .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task<string> GetMeetingTranscriptionsAsync(string encodedMeetingId, string userId = null, DateTime? meetingEndTime = null)
        {
            try
            {
                string accessToken = await GetToken();
                var client = new HttpClient();

                // Decode the base64 meeting ID to get the thread ID
                var decodedBytes = Convert.FromBase64String(encodedMeetingId);
                var decodedId = System.Text.Encoding.UTF8.GetString(decodedBytes);

                // Extract the thread ID (format: 19:meeting_<guid>@thread.v2)
                var threadMatch = System.Text.RegularExpressions.Regex.Match(decodedId, @"19:meeting_[^@]+@thread\.v2");
                if (!threadMatch.Success)
                {
                    return string.Empty;
                }

                string threadId = threadMatch.Value;

                // Query call records to find the matching call
                var callRecordsEndpoint = "https://graph.microsoft.com/v1.0/communications/callRecords";
                var callRecordsReq = new HttpRequestMessage(HttpMethod.Get, callRecordsEndpoint);
                callRecordsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var callRecordsResponse = await client.SendAsync(callRecordsReq);
                if (!callRecordsResponse.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                var callRecordsContent = await callRecordsResponse.Content.ReadAsStringAsync();
                var callRecords = JObject.Parse(callRecordsContent)["value"];

                if (callRecords == null || !callRecords.Any())
                {
                    return string.Empty;
                }

                // Search through call records to find the one matching our thread ID
                foreach (var record in callRecords)
                {
                    string callId = record["id"]?.ToString();
                    if (string.IsNullOrEmpty(callId)) continue;

                    // Get full call record details
                    var callDetailsEndpoint = $"https://graph.microsoft.com/v1.0/communications/callRecords/{callId}";
                    var callDetailsReq = new HttpRequestMessage(HttpMethod.Get, callDetailsEndpoint);
                    callDetailsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var callDetailsResponse = await client.SendAsync(callDetailsReq);
                    if (!callDetailsResponse.IsSuccessStatusCode) continue;

                    var callDetailsContent = await callDetailsResponse.Content.ReadAsStringAsync();
                    var callDetails = JObject.Parse(callDetailsContent);

                    // Check if the joinWebUrl contains our thread ID
                    string joinUrl = callDetails["joinWebUrl"]?.ToString();
                    if (string.IsNullOrEmpty(joinUrl)) continue;

                    string decodedUrl = Uri.UnescapeDataString(joinUrl);
                    string threadIdWithoutSuffix = threadId.Replace("@thread.v2", "");

                    if (decodedUrl.Contains(threadIdWithoutSuffix))
                    {
                        // Get organizer ID
                        string organizerId = callDetails["organizer"]?["user"]?["id"]?.ToString();
                        if (string.IsNullOrEmpty(organizerId))
                        {
                            continue;
                        }

                        // Query organizer's meetings with joinWebUrl filter
                        string encodedJoinUrl = Uri.EscapeDataString(joinUrl);
                        var organizerMeetingsEndpoint = $"{this.azureSettings.Value.GraphApiEndpoint}/users/{organizerId}/onlineMeetings?$filter=JoinWebUrl eq '{encodedJoinUrl}'";
                        var organizerMeetingsReq = new HttpRequestMessage(HttpMethod.Get, organizerMeetingsEndpoint);
                        organizerMeetingsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        var meetingsResponse = await client.SendAsync(organizerMeetingsReq);
                        if (!meetingsResponse.IsSuccessStatusCode)
                        {
                            continue;
                        }

                        var meetingsContent = await meetingsResponse.Content.ReadAsStringAsync();
                        var meetings = JObject.Parse(meetingsContent)["value"];

                        if (meetings == null || !meetings.Any())
                        {
                            continue;
                        }

                        // Find the meeting that matches the joinWebUrl exactly
                        var matchedMeeting = meetings.FirstOrDefault(m => m["joinWebUrl"]?.ToString() == joinUrl);
                        if (matchedMeeting == null)
                        {
                            continue;
                        }

                        string graphMeetingId = matchedMeeting["id"]?.ToString();

                        // Get transcripts using the correct meeting ID
                        var transcriptsEndpoint = $"{this.azureSettings.Value.GraphApiEndpoint}/users/{organizerId}/onlineMeetings/{graphMeetingId}/transcripts";
                        var transcriptsReq = new HttpRequestMessage(HttpMethod.Get, transcriptsEndpoint);
                        transcriptsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        var transcriptsResponse = await client.SendAsync(transcriptsReq);
                        if (!transcriptsResponse.IsSuccessStatusCode)
                        {
                            return string.Empty;
                        }

                        var transcriptsContent = await transcriptsResponse.Content.ReadAsStringAsync();
                        var transcriptsArray = JObject.Parse(transcriptsContent)["value"] as JArray;

                        var transcripts = transcriptsArray?.ToObject<List<Transcripts>>();

                        if (transcripts != null && transcripts.Count > 0)
                        {
                            // CRITICAL: Filter by meeting end time if provided
                            Transcripts targetTranscript = null;

                            if (meetingEndTime.HasValue)
                            {
                                // Tolerance: only consider transcripts within 5 minutes of meeting end time
                                const double toleranceMinutes = 5.0;

                                // Find transcripts within tolerance
                                var matchingTranscripts = transcripts
                                    .Where(t => t.EndDateTime.HasValue &&
                                               Math.Abs((t.EndDateTime.Value - meetingEndTime.Value).TotalMinutes) <= toleranceMinutes)
                                    .ToList();

                                if (matchingTranscripts.Any())
                                {
                                    // Select the closest match
                                    targetTranscript = matchingTranscripts
                                        .OrderBy(t => Math.Abs((t.EndDateTime.Value - meetingEndTime.Value).TotalSeconds))
                                        .FirstOrDefault();
                                }
                                else
                                {
                                    // Return empty to trigger retry - DO NOT use fallback
                                    return string.Empty;
                                }
                            }
                            else
                            {
                                // No end time provided - use most recent
                                targetTranscript = transcripts.OrderByDescending(t => t.CreatedDateTime).FirstOrDefault();
                            }

                            // If we have a target transcript, fetch its content
                            if (targetTranscript != null)
                            {
                                var transcriptId = targetTranscript.Id;
                                var getTranscriptEndpoint = $"{transcriptsEndpoint}/{transcriptId}/content?$format=text/vtt";

                                var getTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getTranscriptEndpoint);
                                getTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                                var transcriptResponse = await client.SendAsync(getTranscriptReq);

                                if (transcriptResponse.IsSuccessStatusCode)
                                {
                                    var transcriptContent = await transcriptResponse.Content.ReadAsStringAsync();
                                    return transcriptContent;
                                }
                            }
                        }

                        break;
                    }
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GetMeetingOrganizerFromUserId(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string DecodeMeetingId(string encodedMeetingId)
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(encodedMeetingId);
                var decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);

                var cleanedId = decodedString;

                if (cleanedId.Contains("#"))
                {
                    var parts = cleanedId.Split('#');
                    if (parts.Length >= 2)
                    {
                        cleanedId = parts[1];
                    }
                }

                return cleanedId;
            }
            catch (Exception)
            {
                return encodedMeetingId;
            }
        }

        public async Task<string> GetUserIdFromUpnAsync(string userPrincipalName)
        {
            try
            {
                string accessToken = await GetToken();

                var getUserEndpoint = $"{this.azureSettings.Value.GraphApiEndpoint}/users/{userPrincipalName}?$select=id";
                var getUserReq = new HttpRequestMessage(HttpMethod.Get, getUserEndpoint);
                getUserReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var client = new HttpClient();
                var response = await client.SendAsync(getUserReq);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JObject.Parse(content);
                    var userId = user["id"]?.ToString();
                    return userId ?? string.Empty;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
