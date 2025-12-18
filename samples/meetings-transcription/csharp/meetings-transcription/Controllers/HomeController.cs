// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using meetings_transcription.Helpers;
using meetings_transcription.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace meetings_transcription.Controllers
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ConcurrentDictionary<string, string> transcriptsDictionary;

        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly GraphHelper graphHelper;

        public HomeController (ConcurrentDictionary<string, string> transcriptsDictionary, IOptions<AzureSettings> azureSettings)
        {
            this.transcriptsDictionary = transcriptsDictionary;
            this.azureSettings = azureSettings;
            graphHelper = new(azureSettings);
        }

        /// <summary>
        /// Returns view to be displayed in Task Module.
        /// </summary>
        /// <param name="meetingId">Id of the meeting.</param>
        /// <returns></returns>
        public async Task<IActionResult> Index([FromQuery] string meetingId)
        {
            ViewBag.Transcripts = "Transcript not found.";

            if (!string.IsNullOrEmpty(meetingId))
            {
                // Try to find in cache using the ENCODED meeting ID (key used by middleware)
                var isFound = transcriptsDictionary.TryGetValue(meetingId, out string transcripts);
                if (isFound)
                {
                    ViewBag.Transcripts = transcripts;
                }
                else
                {
                    // Fallback: Try to fetch from Graph API if not in cache
                    // Use the configured UserId or AzureAdUserId
                    var userId = this.azureSettings.Value.AzureAdUserId ?? this.azureSettings.Value.UserId;
                    
                    // If userId is a UPN, need to resolve it first
                    if (!string.IsNullOrEmpty(userId) && userId.Contains("@"))
                    {
                        userId = await this.graphHelper.GetUserIdFromUpnAsync(userId);
                    }
                    
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Use the ENCODED meeting ID when calling Graph API
                        var result = await this.graphHelper.GetMeetingTranscriptionsAsync(meetingId, userId);
                        if (!string.IsNullOrEmpty(result))
                        {
                            // Store with ENCODED meeting ID as key (to match middleware)
                            result = result.Replace("<v", "");
                            transcriptsDictionary.AddOrUpdate(meetingId, result, (key, newValue) => result);
                            ViewBag.Transcripts = result;
                        }
                    }
                }
            }
            return View();
        }
    }
}
