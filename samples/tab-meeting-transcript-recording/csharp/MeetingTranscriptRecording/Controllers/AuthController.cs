// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using Azure.Core;
using MeetingTranscriptRecording.Helper;
using MeetingTranscriptRecording.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.SecurityNamespace;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Net.Http.Headers;

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

                            string joinUrl = element.onlineMeeting.joinUrl;

                            //-------------------------------------------------------------------------------------------------------------------------------------
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
                                        string onlineMeetingId = JoinWebUrlData.id;

                                        //-----------------------------------------------------------------------------------------------------------------------

                                        string graphApiEndpointOnlineTranscripts = $"https://graph.microsoft.com/beta/me/onlineMeetings/" + onlineMeetingId + "/transcripts";

                                        var responseBodyTranscripts = await AuthHelper.GetApiData(graphApiEndpointOnlineTranscripts, accessToken);

                                        var responseTranscriptsData = JsonConvert.DeserializeObject<transcriptsID>(responseBodyTranscripts);

                                        if (responseTranscriptsData != null)
                                        {
                                            if (responseTranscriptsData.Value.Count > 0)
                                            {
                                                foreach (transcriptsIDs TranscriptsData in responseTranscriptsData.Value)
                                                {
                                                    Obj.TranscriptsId = TranscriptsData.id;
                                                    string TranscriptsId = TranscriptsData.id;

                                                    //------------------------------------------------------------------------------------------------------------------
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