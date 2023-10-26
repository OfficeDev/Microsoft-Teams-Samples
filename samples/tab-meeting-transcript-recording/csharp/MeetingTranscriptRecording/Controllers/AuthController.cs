// <copyright file="AuthController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>
using MeetingTranscriptRecording.Helper;
using MeetingTranscriptRecording.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Concurrent;

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

        private static ConcurrentDictionary<string, CardData> TranscriptRecordingEventDetails = new ConcurrentDictionary<string, CardData>();

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
        /// This method retrieves information about the event details.
        /// It obtains an access token and makes requests to the Microsoft Graph API to fetch events, online meetings, transcripts, and recordings.
        /// It constructs a JSON response with data related to online meetings.
        /// Error handling is included in case of exceptions.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEventInformation")]
        public async Task<JsonResult> GetEventInformation()
        {
            try
            {
                var CardResults = new List<CardData>();

                var eventCount = 0;

                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                var BaseURL = _configuration["AzureAd:BaseURL"];

                string graphApiEndpointEvents = BaseURL + $"me/events?$orderby=start/dateTime desc";

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
                                if (eventCount <= 9)
                                {
                                    var Obj = new CardData();
                                    Obj.subject = element.subject;
                                    Obj.start = element.start.dateTime.ToString("MMM dd h:mm tt");
                                    Obj.end = element.end.dateTime.ToString("MMM dd h:mm tt");
                                    Obj.organizer = element.organizer.emailAddress.name;

                                    eventCount++;

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

                                                if (onlineMeetingId != null)
                                                {

                                                    string graphApiEndpointOnlineTranscripts = BaseURL + $"me/onlineMeetings/" + onlineMeetingId + "/transcripts";

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
                                                            }
                                                        }
                                                        else
                                                        {
                                                            await CreateTranscriptsSubscription(onlineMeetingId, accessToken);
                                                        }
                                                    }

                                                    string graphApiEndpointOnlineRecordings = BaseURL + $"me/onlineMeetings/" + onlineMeetingId + "/recordings";

                                                    var responseBodyRecordings = await AuthHelper.GetApiData(graphApiEndpointOnlineRecordings, accessToken);

                                                    var responseRecordingsData = JsonConvert.DeserializeObject<RecordingData>(responseBodyRecordings);

                                                    if (responseRecordingsData != null)
                                                    {
                                                        if (responseRecordingsData.Value.Count > 0)
                                                        {
                                                            foreach (RecordingId RecordingsData in responseRecordingsData.Value)
                                                            {
                                                                Obj.recordingId = RecordingsData.id;
                                                                Obj.condition = true;

                                                                //-------------Get recordings Id--------------
                                                                string RecordingId = RecordingsData.id;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            await CreateRecordingsSubscription(onlineMeetingId, accessToken);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    return null;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                    TranscriptRecordingEventDetails.TryAdd(Obj.onlineMeetingId, Obj);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                return Json(TranscriptRecordingEventDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// This method is used to fetch meeting transcripts.
        /// It constructs a Graph API endpoint based on the provided meeting and transcript IDs.
        /// The method retrieves and returns the content of the transcripts in VTT format.
        /// Error handling is included for exceptions.
        /// </summary>
        /// <param name="MeetingTranscriptsIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getMeetingTranscripts")]
        public async Task<JsonResult> getMeetingTranscripts([FromBody] TranscriptsRequestBody MeetingTranscriptsIds)
        {
            try
            {
                var accessToke = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                string graphApiEndpointOnlineTranscriptsData = _configuration["AzureAd:BaseURL"] + $"me/onlineMeetings/" + MeetingTranscriptsIds.meetingId + "/transcripts/" + MeetingTranscriptsIds.transcriptsId + "/content?$format=text/vtt";

                var responseBody = await AuthHelper.GetApiData(graphApiEndpointOnlineTranscriptsData, accessToke);

                if (responseBody != null)
                {
                    return Json(responseBody);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


        /// <summary>
        /// This method fetches meeting recordings.
        /// It constructs the Graph API endpoint to get the content of a specific meeting recording.
        /// It downloads the recording content and returns it as a video file (MP4 format).
        /// Error handling is included for exceptions.
        /// </summary>
        /// <param name="MeetingRecordingIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getMeetingRecording")]
        public async Task<IActionResult> getMeetingRecording([FromBody] RecordingRequestBody MeetingRecordingIds)
        {
            try
            {
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                string graphApiEndpointOnlineRecordData = _configuration["AzureAd:BaseURL"] + $"me/onlineMeetings/" + MeetingRecordingIds.meetingId + "/recordings/" + MeetingRecordingIds.recordingId + "/content";

                using (HttpClient clientRecording = new HttpClient())
                {
                    // Set the base address for the Graph API
                    clientRecording.BaseAddress = new Uri(graphApiEndpointOnlineRecordData);

                    // Set the authorization header with the access token
                    clientRecording.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Make a GET request to retrieve user data
                    HttpResponseMessage response = await clientRecording.GetAsync(graphApiEndpointOnlineRecordData);

                    // Check the status code
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            var videoContent = await response.Content.ReadAsByteArrayAsync();
                            return File(videoContent, "video/mp4");
                        }
                    }
                }
                return null;
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
        /// <param name="onlineMeetingId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>

        public async Task<IActionResult> CreateTranscriptsSubscription(string onlineMeetingId, string accessToken)
        {
            try
            {
                IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;

                var graphClient = GraphClient.GetGraphClient(accessToken);
                try
                {
                    existingSubscriptions = await graphClient.Subscriptions.Request().GetAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }

                var notificationUrl = _configuration["AzureAd:BaseUrlNgrok"] + "/CreateSubscriptionTranscripts";

                var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == "communications/onlineMeetings/" + onlineMeetingId + "/transcripts");

                if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
                {
                    await DeleteSubscription(existingSubscription);
                    existingSubscription = null;
                }
                // Get the current time
                DateTime currentDateTime = DateTime.UtcNow;

                // Subtract 1 hour from the current time
                DateTime expirationDateTime = currentDateTime + TimeSpan.FromHours(1);

                // Format the "expirationDateTime" as an ISO 8601 string with "Z" indicating UTC
                string expirationDateTimeString = expirationDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

                if (existingSubscription == null)
                {
                    var sub = new Subscription
                    {
                        Resource = "communications/onlineMeetings/" + onlineMeetingId + "/transcripts",
                        ChangeType = "created",
                        NotificationUrl = notificationUrl,
                        ClientState = "ClientState",
                        ExpirationDateTime = DateTimeOffset.Parse(expirationDateTimeString).UtcDateTime
                    };
                    try
                    {
                        existingSubscription = await graphClient.Subscriptions.Request().AddAsync(sub);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return Ok(); // Return an HTTP 200 OK response to indicate a successful subscription creation.
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onlineMeetingId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreateRecordingsSubscription(string onlineMeetingId, string accessToken)
        {
            try
            {
                IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;

                var graphClient = GraphClient.GetGraphClient(accessToken);
                try
                {
                    existingSubscriptions = await graphClient.Subscriptions.Request().GetAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }

                var notificationUrl = _configuration["AzureAd:BaseUrlNgrok"] + "/CreateSubscriptionRecordings";

                var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == "communications/onlineMeetings/" + onlineMeetingId + "/recordings");

                if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
                {
                    await DeleteSubscription(existingSubscription);
                    existingSubscription = null;
                }
                // Get the current time
                DateTime currentDateTime = DateTime.UtcNow;

                // Subtract 1 hour from the current time
                DateTime expirationDateTime = currentDateTime + TimeSpan.FromHours(1);

                // Format the "expirationDateTime" as an ISO 8601 string with "Z" indicating UTC
                string expirationDateTimeString = expirationDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

                if (existingSubscription == null)
                {
                    var sub = new Subscription
                    {
                        Resource = "communications/onlineMeetings/" + onlineMeetingId + "/recordings",
                        ChangeType = "created",
                        NotificationUrl = notificationUrl,
                        ClientState = "ClientState",
                        ExpirationDateTime = DateTimeOffset.Parse(expirationDateTimeString).UtcDateTime
                    };
                    try
                    {
                        existingSubscription = await graphClient.Subscriptions.Request().AddAsync(sub);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return Ok(); // Return an HTTP 200 OK response to indicate a successful subscription creation.
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>

        private async Task DeleteSubscription(Subscription subscription)
        {
            var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

            var graphClient = GraphClient.GetGraphClient(accessToken);

            try
            {
                await graphClient
                     .Subscriptions[subscription.Id]
                     .Request()
                     .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationToken"></param>
        /// <returns></returns>
        [Route("CreateSubscriptionTranscripts")]
        [HttpPost]
        public async Task<ActionResult<string>> CreateSubscriptionTranscripts([FromQuery] string validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                return Ok(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                var notifications = JsonConvert.DeserializeObject<MeetingTranscriptRecording.Model.Notifications>(content);

                if (notifications != null)
                {
                    if (notifications.Items[0].ResourceData.ODataId != null)
                    {
                        int startIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("communications/onlineMeetings('");

                        int endIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("')/transcripts('");

                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            string onlineMeetingId = notifications.Items[0].ResourceData.ODataId.Substring(startIndex + "communications/onlineMeetings('".Length, endIndex - startIndex - "communications/onlineMeetings('".Length);
                            
                            string TranscriptsId = notifications.Items[0].ResourceData.Id;
                            
                            if (onlineMeetingId != null && TranscriptsId != null)
                            {
                                TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetails);

                                if (EventDetails != null)
                                {
                                    var OldEventDetails = EventDetails;

                                    EventDetails.transcriptsId = TranscriptsId;

                                    TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);
                                }

                            }
                        }
                    }
                }
            }
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationToken"></param>
        /// <returns></returns>
        [Route("CreateSubscriptionRecordings")]
        [HttpPost]
        public async Task<ActionResult<string>> CreateSubscriptionRecordings([FromQuery] string validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                return Ok(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                var notifications = JsonConvert.DeserializeObject<MeetingTranscriptRecording.Model.Notifications>(content);

                if (notifications != null)
                {
                    if (notifications.Items[0].ResourceData.ODataId != null)
                    {
                        int startIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("communications/onlineMeetings('");

                        int endIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("')/recordings('");

                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            string onlineMeetingId = notifications.Items[0].ResourceData.ODataId.Substring(startIndex + "communications/onlineMeetings('".Length, endIndex - startIndex - "communications/onlineMeetings('".Length);
                            
                            string RecordingsId = notifications.Items[0].ResourceData.Id;
                            
                            if (onlineMeetingId != null && RecordingsId != null)
                            {
                                TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetails);

                                if (EventDetails != null)
                                {
                                    var OldEventDetails = EventDetails;

                                    EventDetails.recordingId = RecordingsId;

                                    TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);
                                }

                            }

                        }
                    }
                }
            }
            return Ok();
        }
    }

}