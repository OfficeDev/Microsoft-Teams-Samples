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

            var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

            var CardResults = new List<CardData>();

            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                // Replace with your Graph API endpoint and access token
                string graphApiEndpoint = $"https://graph.microsoft.com/beta/me/events";

                // Set the base address for the Graph API
                client.BaseAddress = new Uri(graphApiEndpoint);

                // Set the authorization header with the access token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make a GET request to retrieve user data
                HttpResponseMessage response = await client.GetAsync(graphApiEndpoint);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and display the response content
                    string responseBody = await response.Content.ReadAsStringAsync();

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

                                    //CardResults.Add(new CardData
                                    //{
                                    //    subject = element.subject
                                    //});

                                    string joinUrl = element.onlineMeeting.joinUrl;

                                    //-------------------------------------------------------------------------------------------------------------------------------------
                                    string graphApiEndpointJoinUrl = $"https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl%20eq%20'" + joinUrl + "'";

                                    using (HttpClient clientJoinUrl = new HttpClient())
                                    {

                                        // Set the base address for the Graph API
                                        clientJoinUrl.BaseAddress = new Uri(graphApiEndpointJoinUrl);

                                        // Set the authorization header with the access token
                                        clientJoinUrl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                                        // Make a GET request to retrieve user data
                                        HttpResponseMessage responseJoinUrl = await clientJoinUrl.GetAsync(graphApiEndpointJoinUrl);

                                        // Check if the request was successful
                                        if (responseJoinUrl.IsSuccessStatusCode)
                                        {
                                            // Read and display the responseJoinUrl content
                                            string responseBodyJoinUrl = await responseJoinUrl.Content.ReadAsStringAsync();

                                            var responseJoinUrlData = JsonConvert.DeserializeObject<JoinUrlData>(responseBodyJoinUrl);

                                            if (responseJoinUrlData != null)
                                            {
                                                if (responseJoinUrlData.Value.Count > 0)
                                                {
                                                    foreach (JoinWebUrl JoinWebUrlData in responseJoinUrlData.Value)
                                                    {
                                                        Obj.onlineMeetingId = JoinWebUrlData.id;

                                                        //CardResults.Add(new CardData
                                                        //{
                                                        //    onlineMeetingId = JoinWebUrlData.id
                                                        //});

                                                        string onlineMeetingId = JoinWebUrlData.id;

                                                        //-----------------------------------------------------------------------------------------------------------------------

                                                        string graphApiEndpointOnlineTranscripts = $"https://graph.microsoft.com/beta/me/onlineMeetings/" + onlineMeetingId + "/transcripts";

                                                        using (HttpClient clientTranscripts = new HttpClient())
                                                        {
                                                            // Set the base address for the Graph API
                                                            clientTranscripts.BaseAddress = new Uri(graphApiEndpointOnlineTranscripts);

                                                            // Set the authorization header with the access token
                                                            clientTranscripts.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                                                            // Make a GET request to retrieve user data
                                                            HttpResponseMessage responseTranscripts = await clientTranscripts.GetAsync(graphApiEndpointOnlineTranscripts);

                                                            if (responseTranscripts.IsSuccessStatusCode)
                                                            {
                                                                // Read and display the responseTranscripts content
                                                                string responseBodyTranscripts = await responseTranscripts.Content.ReadAsStringAsync();

                                                                var responseTranscriptsData = JsonConvert.DeserializeObject<transcriptsID>(responseBodyTranscripts);

                                                                if (responseTranscriptsData != null)
                                                                {
                                                                    if (responseTranscriptsData.Value.Count > 0)
                                                                    {
                                                                        foreach (transcriptsIDs TranscriptsData in responseTranscriptsData.Value)
                                                                        {
                                                                            //CardResults.Add(new CardData
                                                                            //{
                                                                            //    TranscriptsId = TranscriptsData.id
                                                                            //});

                                                                            Obj.TranscriptsId = TranscriptsData.id;

                                                                            string TranscriptsId = TranscriptsData.id;

                                                                            //------------------------------------------------------------------------------------------------------------------
                                                                        }
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

                            };
                        }
                    }
                }
                return Json(CardResults);
            }
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