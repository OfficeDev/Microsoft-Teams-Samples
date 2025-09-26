// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscription.Helpers
{
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using MeetingTranscription.Models.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Net.Http;
    using MeetingTranscription.Models;
    using Newtonsoft.Json.Linq;
    using System.Net;

    public class GraphHelper
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        public GraphHelper(IOptions<AzureSettings> azureSettings)
        {
            this.azureSettings = azureSettings;
        }

        /// <summary>
        /// Gets application token.
        /// </summary>
        /// <returns>Application token.</returns>
        public async Task<string> GetToken()
        {
            // Validate configuration before attempting to get token
            if (string.IsNullOrEmpty(this.azureSettings.Value.MicrosoftAppId) || 
                this.azureSettings.Value.MicrosoftAppId.Contains("<<") ||
                string.IsNullOrEmpty(this.azureSettings.Value.MicrosoftAppPassword) || 
                this.azureSettings.Value.MicrosoftAppPassword.Contains("<<") ||
                string.IsNullOrEmpty(this.azureSettings.Value.MicrosoftAppTenantId) || 
                this.azureSettings.Value.MicrosoftAppTenantId.Contains("<<"))
            {
                throw new InvalidOperationException("Azure configuration values are missing or contain placeholder values. Please update appsettings.json with actual values.");
            }

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(this.azureSettings.Value.MicrosoftAppId)
                .WithClientSecret(this.azureSettings.Value.MicrosoftAppPassword)
                .WithAuthority($"https://login.microsoftonline.com/{this.azureSettings.Value.MicrosoftAppTenantId}")
                .WithRedirectUri("https://daemon")
                .Build();

            // TeamsAppInstallation.ReadWriteForChat.All Chat.Create User.Read.All TeamsAppInstallation.ReadWriteForChat.All
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        /// <summary>
        /// Gets the meeting transcript's content for the passed meeting Id.
        /// </summary>
        /// <param name="meetingId">Id of meeting.</param>
        /// <returns>Meeting transcript if any otherwise return empty string.</returns>
        public async Task<string> GetMeetingTranscriptionsAsync(string meetingId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(meetingId))
                {
                    Console.WriteLine("Meeting ID is null or empty");
                    return string.Empty;
                }

                // Validate configuration
                if (string.IsNullOrEmpty(this.azureSettings.Value.UserId) || 
                    this.azureSettings.Value.UserId.Contains("<<"))
                {
                    Console.WriteLine("UserId configuration is missing or contains placeholder values");
                    return string.Empty;
                }

                string access_Token = await GetToken();
                var getAllTranscriptsEndpoint = $"{this.azureSettings.Value.GraphApiEndpoint}/users/{this.azureSettings.Value.UserId}/onlineMeetings/{meetingId}/transcripts";
                var getAllTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getAllTranscriptsEndpoint);
                getAllTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_Token);

                var client = new HttpClient();
                var response = await client.SendAsync(getAllTranscriptReq);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Graph API call failed with status {response.StatusCode}: {errorContent}");
                    return string.Empty;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Validate content before parsing
                if (string.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("Graph API returned empty content");
                    return string.Empty;
                }

                // Additional validation for JSON content
                if (!content.TrimStart().StartsWith("{") && !content.TrimStart().StartsWith("["))
                {
                    Console.WriteLine($"Graph API returned non-JSON content: {content}");
                    return string.Empty;
                }

                JObject jsonResponse;
                try
                {
                    jsonResponse = JObject.Parse(content);
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    Console.WriteLine($"Failed to parse JSON response: {ex.Message}. Content: {content}");
                    return string.Empty;
                }

                // Check if the response has the expected structure
                if (jsonResponse["value"] == null)
                {
                    Console.WriteLine($"Graph API response missing 'value' property. Response: {content}");
                    return string.Empty;
                }

                var transcripts = jsonResponse["value"].ToObject<List<Transcripts>>();

                if (transcripts != null && transcripts.Count > 0)
                {
                    var getTranscriptEndpoint = $"{getAllTranscriptsEndpoint}/{transcripts.FirstOrDefault().Id}/content?$format=text/vtt";

                    var getTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getTranscriptEndpoint);
                    getTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_Token);
                    var transcriptResponse = await client.SendAsync(getTranscriptReq);

                    if (transcriptResponse.IsSuccessStatusCode)
                    {
                        return await transcriptResponse.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var transcriptErrorContent = await transcriptResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to get transcript content with status {transcriptResponse.StatusCode}: {transcriptErrorContent}");
                        return string.Empty;
                    }
                }
                else
                {
                    Console.WriteLine("No transcripts found for the meeting");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMeetingTranscriptionsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return string.Empty; // Return empty string instead of throwing to prevent bot crashes
            }
        }
    }
}
