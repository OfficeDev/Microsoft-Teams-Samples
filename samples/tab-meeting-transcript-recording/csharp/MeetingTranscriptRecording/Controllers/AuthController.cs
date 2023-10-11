// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using MeetingTranscriptRecording.Helper;
using MeetingTranscriptRecording.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace MeetingTranscriptRecording.Controllers
{
    public class AuthController : Controller
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        public readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get user access token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetLoginUserInformation")]
        public async Task<JsonResult> GetLoginUserInformation()
        {
            try
            {
                var CardResults = new List<CardData>();

                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                string graphApiEndpointEvents = $"https://graph.microsoft.com/beta/me/events";

                var responseBody = await AuthHelper.GetApiData(graphApiEndpointEvents, accessToken);

                var responseData = JsonConvert.DeserializeObject<ResponseEventData>(responseBody);

                if (responseData != null)
                {
                    if (responseData.Value.Count > 0)
                    {
                        var allEvents = responseData.Value;
                        foreach (EventData element in allEvents)
                        {
                            if (element.isOnlineMeeting == true)
                            {
                                var Obj = new CardData();

                                Obj.subject = element.subject;
                                Obj.start = element.start.dateTime.ToString("MMM dd h:mm tt");
                                Obj.end = element.end.dateTime.ToString("MMM dd h:mm tt");
                                Obj.organizer = element.organizer.emailAddress.name;

                                //---------------Get Join URL---------------
                                string joinUrl = element.onlineMeeting.joinUrl;

                                string graphApiEndpointJoinUrl = $"https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl%20eq%20'" + joinUrl + "'";

                                var responseBodyJoinUrl = await AuthHelper.GetApiData(graphApiEndpointJoinUrl, accessToken);

                                var responseJoinUrlData = JsonConvert.DeserializeObject<JoinUrlData>(responseBodyJoinUrl);

                                if (responseJoinUrlData != null)
                                {
                                    if (responseJoinUrlData.Value.Count > 0)
                                    {
                                        foreach (JoinWebUrl JoinWebUrlData in responseJoinUrlData.Value)
                                        {
                                            Obj.onlineMeetingId = JoinWebUrlData.id;

                                            //----------- Get OnlineMeetingId---------------
                                            string onlineMeetingId = JoinWebUrlData.id;

                                            string graphApiEndpointOnlineTranscripts = $"https://graph.microsoft.com/beta/me/onlineMeetings/" + onlineMeetingId + "/transcripts";

                                            var responseBodyTranscripts = await AuthHelper.GetApiData(graphApiEndpointOnlineTranscripts, accessToken);

                                            var responseTranscriptsData = JsonConvert.DeserializeObject<transcriptsData>(responseBodyTranscripts);

                                            if (responseTranscriptsData != null)
                                            {
                                                if (responseTranscriptsData.Value.Count > 0)
                                                {
                                                    foreach (transcriptsId TranscriptsData in responseTranscriptsData.Value)
                                                    {
                                                        Obj.transcriptsId = TranscriptsData.id;

                                                        //-------------Get transcripts Id--------------
                                                        string TranscriptsId = TranscriptsData.id;

                                                        string graphApiEndpointOnlineRecordings = $"https://graph.microsoft.com/beta/me/onlineMeetings/" + onlineMeetingId + "/recordings";

                                                        var responseBodyRecordings = await AuthHelper.GetApiData(graphApiEndpointOnlineRecordings, accessToken);

                                                        var responseRecordingsData = JsonConvert.DeserializeObject<RecordingData>(responseBodyRecordings);

                                                        if (responseRecordingsData != null)
                                                        {
                                                            if (responseRecordingsData.Value.Count > 0)
                                                            {
                                                                foreach (RecordingId RecordingsData in responseRecordingsData.Value)
                                                                {
                                                                    Obj.recordingId = RecordingsData.id;

                                                                    //-------------Get recordings Id--------------
                                                                    string RecordingId = RecordingsData.id;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                CardResults.Add(Obj);
                            }
                        }
                    }
                }
                return Json(CardResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        [Route("TestEvent")]
        [HttpPost]
        public async Task<IActionResult> TestEvent([FromBody] Event events)
        {
            try
            {
                if (events != null)
                {

                }
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}