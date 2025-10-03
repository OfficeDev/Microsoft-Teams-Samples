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
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Bot.Schema;

    public class GraphHelper
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        /// <summary>
        /// Cache for user discovery to avoid repeated API calls.
        /// </summary>
        private static readonly Dictionary<string, string> UserCache = new Dictionary<string, string>();

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
        /// Get meeting transcripts using a specific user ID.
        /// </summary>
        /// <param name="meetingId">Meeting ID.</param>
        /// <param name="userId">User ID who has access to the meeting.</param>
        /// <returns>Meeting transcripts.</returns>
        public async Task<string> GetMeetingTranscriptionsAsync(string meetingId, string userId=null)
        {
            try
            {
                string accessToken = await GetToken();
                
                Console.WriteLine($"Attempting to get transcripts for meeting {meetingId} using user {userId}");
                
                var getAllTranscriptsEndpoint = $"{this.azureSettings.Value.GraphApiEndpoint}/users/{userId}/onlineMeetings/{meetingId}/transcripts";
                var getAllTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getAllTranscriptsEndpoint);
                getAllTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var client = new HttpClient();
                var response = await client.SendAsync(getAllTranscriptReq);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get transcripts: {response.StatusCode} - {errorContent}");
                    return string.Empty;
                }

                var content = await response.Content.ReadAsStringAsync();
                var transcripts = (JObject.Parse(content)["value"]).ToObject<List<Transcripts>>();

                if (transcripts != null && transcripts.Count > 0)
                {
                    var transcriptId = transcripts.FirstOrDefault()?.Id;
                    var getTranscriptEndpoint = $"{getAllTranscriptsEndpoint}/{transcriptId}/content?$format=text/vtt";

                    var getTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getTranscriptEndpoint);
                    getTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var transcriptResponse = await client.SendAsync(getTranscriptReq);

                    if (transcriptResponse.IsSuccessStatusCode)
                    {
                        var transcriptContent = await transcriptResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Successfully retrieved transcript content ({transcriptContent.Length} characters)");
                        return transcriptContent;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get transcript content: {transcriptResponse.StatusCode}");
                        return string.Empty;
                    }
                }
                else
                {
                    Console.WriteLine("No transcripts found for this meeting.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting transcripts: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// TEAMS-SPECIFIC: Get meeting organizer using Teams context and member information.
        /// This is the most reliable approach for Teams bots.
        /// </summary>
        /// <param name="turnContext">Teams turn context from the bot.</param>
        /// <returns>Meeting organizer user ID.</returns>
        public async Task<string> GetMeetingOrganizerFromTeamsContextAsync(ITurnContext turnContext)
        {
            try
            {
                Console.WriteLine("Getting meeting organizer from Teams context...");

                // Get meeting information from Teams context
                var meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext);
                if (meetingInfo?.Organizer != null)
                {
                    var organizerAadId = meetingInfo.Organizer.AadObjectId;
                    Console.WriteLine($"Found meeting organizer from meeting info: {organizerAadId}");
                    return organizerAadId;
                }

                // Last fallback: use the current user from turn context
                if (turnContext.Activity?.From?.AadObjectId != null)
                {
                    Console.WriteLine($"Using current user as fallback organizer: {turnContext.Activity.From.AadObjectId}");
                    return turnContext.Activity.From.AadObjectId;
                }

                Console.WriteLine("No meeting organizer found from Teams context");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting meeting organizer from Teams context: {ex.Message}");
                return string.Empty;
            }
        }
    }
}