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
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Web;
    using Newtonsoft.Json;
    using Microsoft.Bot.Schema;
    using MeetingTranscription.Models;
    using Newtonsoft.Json.Linq;
    using Microsoft.Bot.Builder;
    using System.Collections.Concurrent;

    public class GraphHelper
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        const string graphBetaEndpoint = "https://graph.microsoft.com/beta";
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

        public async Task<string> GetMeetingTranscriptionsAsync(string meetingId, string userId)
        {
            try
            {
                string access_Token = await GetToken();
                var getAllTranscriptsEndpoint = $"{graphBetaEndpoint}/users/{userId}/onlineMeetings/{meetingId}/transcripts";
                var getAllTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getAllTranscriptsEndpoint);
                getAllTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_Token);

                var client = new HttpClient();
                var response = await client.SendAsync(getAllTranscriptReq);

                var content = await response.Content.ReadAsStringAsync();
                var transcripts = (JObject.Parse(content)["value"]).ToObject<List<Transcripts>>();

                if (transcripts.Count > 0)
                {
                    var getTranscriptEndpoint = $"{getAllTranscriptsEndpoint}/{transcripts.FirstOrDefault().Id}/content?$format=text/vtt";

                    var getTranscriptReq = new HttpRequestMessage(HttpMethod.Get, getTranscriptEndpoint);
                    getTranscriptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_Token);
                    var transcriptResponse = await client.SendAsync(getTranscriptReq);

                    return await transcriptResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    return "Transcripts not found";
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
        }
    }
}
