// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using MeetingTranscription.Helpers;
using MeetingTranscription.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MeetingTranscription.Controllers
{
    public class HomeController : Controller
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
                var isFound = transcriptsDictionary.TryGetValue(meetingId, out string transcripts);
                if (isFound)
                {
                    ViewBag.Transcripts = $"Format: {transcripts}";
                }
                else
                {
                    var result = await this.graphHelper.GetMeetingTranscriptionsAsync(meetingId);
                    if (!string.IsNullOrEmpty(meetingId))
                    {
                        transcriptsDictionary.AddOrUpdate(meetingId, result, (key, newValue) => result);
                        ViewBag.Transcripts = $"Format: {result}";
                    }
                }
            }
            return View();
        }
    }
}