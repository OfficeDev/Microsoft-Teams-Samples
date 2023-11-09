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
using System.Text.RegularExpressions;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.SignalR.Client;

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
                // Initialize a list to store event data
                var CardResults = new List<CardData>();

                // Get an access token for making API requests
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                // Create subscription for user events
                await CreateSubscriptionMeEvents(accessToken);

                // Retrieve the base URL from configuration
                var BaseURL = _configuration["AzureAd:BaseURL"];

                // Construct the Graph API endpoint to get user events sorted by start date
                string graphApiEndpointEvents = BaseURL + $"me/events";

                // Make an authenticated API request to get event data
                var responseBody = await AuthHelper.GetApiData(graphApiEndpointEvents, accessToken);

                // Deserialize the response into a list of events
                var responseData = JsonConvert.DeserializeObject<ResponseEventData>(responseBody);

                // Check if there are events and process them
                if (responseData != null && responseData.Value.Count > 0)
                {
                    var allEvents = responseData.Value;
                    foreach (EventData element in allEvents)
                    {
                        // Check if it's an online meeting event with a subject and within the last month
                        if (element.isOnlineMeeting == true && !string.IsNullOrEmpty(element.subject) && DateTime.Now.AddMonths(-1) <= element.start.dateTime)
                        {
                            // Create a CardData object to store event details
                            var Obj = new CardData();
                            Obj.subject = element.subject;
                            Obj.start = element.start.dateTime.ToString("MMM dd h:mm tt");
                            Obj.end = element.end.dateTime.ToString("MMM dd h:mm tt");
                            Obj.organizer = element.organizer.emailAddress.name;
                            Obj.condition = false;
                            Obj.transcriptsId = null;
                            Obj.recordingId = null;

                            // Get the join URL for the online meeting
                            string joinUrl = element.onlineMeeting.joinUrl;

                            // Construct the Graph API endpoint to retrieve online meeting details
                            string graphApiEndpointJoinUrl = $"https://graph.microsoft.com/v1.0/me/onlineMeetings?$filter=JoinWebUrl%20eq%20'" + joinUrl + "'";

                            // Make an authenticated API request to get online meeting details
                            var responseBodyJoinUrl = await AuthHelper.GetApiData(graphApiEndpointJoinUrl, accessToken);

                            // Deserialize the response into JoinUrlData
                            var responseJoinUrlData = JsonConvert.DeserializeObject<JoinUrlData>(responseBodyJoinUrl);

                            if (responseJoinUrlData != null && responseJoinUrlData.Value.Count > 0 && responseJoinUrlData.Value[0].id != null)
                            {
                                // Update the CardData object with the online meeting ID
                                Obj.onlineMeetingId = responseJoinUrlData.Value[0].id;
                            }
                            else
                            {
                                return null;
                            }

                            // Get the join URL for the online meeting
                            TranscriptRecordingEventDetails.TryAdd(Obj.onlineMeetingId, Obj);


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
        /// 
        /// </summary>
        /// <param name="JoinUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getMeetingTranscriptsIdRecordingId")]
        public async Task<JsonResult> getTranscriptsIdRecordingId([FromBody] MeetingIdRequestBody MeetingId)
        {
            try
            {
                // Get an access token for making API requests
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                // Retrieve the base URL from configuration
                var BaseURL = _configuration["AzureAd:BaseURL"];

                if (MeetingId.meetingId != null)
                {
                    // Construct the Graph API endpoint to retrieve online transcripts
                    string graphApiEndpointOnlineTranscripts = BaseURL + $"me/onlineMeetings/" + MeetingId.meetingId + "/transcripts";

                    // Make an authenticated API request to get online transcripts
                    var responseBodyTranscripts = await AuthHelper.GetApiData(graphApiEndpointOnlineTranscripts, accessToken);

                    // Deserialize the response into transcriptsData
                    var responseTranscriptsData = JsonConvert.DeserializeObject<transcriptsData>(responseBodyTranscripts);

                    if (responseTranscriptsData != null && responseTranscriptsData.Value.Count > 0 && responseTranscriptsData.Value[0].id != null)
                    {
                        // Update the CardData object with the transcripts ID and set a condition
                        string TranscriptsId = responseTranscriptsData.Value[0].id;

                        TranscriptRecordingEventDetails.TryGetValue(MeetingId.meetingId, out CardData EventDetailsTranscriptsIds);

                        if (EventDetailsTranscriptsIds != null)
                        {
                            var OldEventDetailsTranscriptsIds = EventDetailsTranscriptsIds;
                            EventDetailsTranscriptsIds.transcriptsId = TranscriptsId;
                            EventDetailsTranscriptsIds.condition = true;
                            TranscriptRecordingEventDetails.TryUpdate(MeetingId.meetingId, EventDetailsTranscriptsIds, OldEventDetailsTranscriptsIds);

                        }
                    }
                    else
                    {
                        // If there are no transcripts, create a subscription for them
                        await CreateTranscriptsSubscription(MeetingId.meetingId, accessToken);
                    }

                    // Construct the Graph API endpoint to retrieve online recordings
                    string graphApiEndpointOnlineRecordings = BaseURL + $"me/onlineMeetings/" + MeetingId.meetingId + "/recordings";

                    var responseBodyRecordings = await AuthHelper.GetApiData(graphApiEndpointOnlineRecordings, accessToken);

                    var responseRecordingsData = JsonConvert.DeserializeObject<RecordingData>(responseBodyRecordings);

                    if (responseRecordingsData != null && responseRecordingsData.Value.Count > 0 && responseRecordingsData.Value[0].id != null)
                    {
                        string RecordingId = responseRecordingsData.Value[0].id;

                        TranscriptRecordingEventDetails.TryGetValue(MeetingId.meetingId, out CardData EventDetailsRecordingIds);

                        if (EventDetailsRecordingIds != null)
                        {
                            var OldEventDetailsRecordingIds = EventDetailsRecordingIds;
                            EventDetailsRecordingIds.recordingId = RecordingId;
                            EventDetailsRecordingIds.condition = true;
                            TranscriptRecordingEventDetails.TryUpdate(MeetingId.meetingId, EventDetailsRecordingIds, OldEventDetailsRecordingIds);
                        }
                    }
                    else
                    {
                        // If there are no transcripts, create a subscription for them
                        await CreateRecordingsSubscription(MeetingId.meetingId, accessToken);
                    }
                }
                else
                {
                    return null;
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
        /// Creates a subscription for Microsoft Graph API events.
        /// </summary>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        public async Task<IActionResult> CreateSubscriptionMeEvents(string accessToken)
        {
            try
            {
                IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;

                var graphClient = GraphClient.GetGraphClient(accessToken);
                try
                {
                    // Retrieve existing subscriptions using the Graph API.
                    existingSubscriptions = await graphClient.Subscriptions.Request().GetAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }

                // Define the notification URL for the subscription, typically an endpoint for handling notifications.
                var notificationUrl = _configuration["AzureAd:BaseUrlNgrok"] + "/CreateSubscriptionMeEventsPost";

                var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == "me/events");

                // Check if an existing subscription with the same resource already exists and has a different notification URL.
                if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
                {
                    // If an existing subscription with a different notification URL is found, delete it.
                    await DeleteSubscription(existingSubscription);
                    existingSubscription = null;
                }

                // Get the current time and calculate an expiration time (typically 1 hour from now).
                DateTime currentTime = DateTime.UtcNow; // Get the current time
                DateTime newTime = currentTime.AddHours(2); // Add 2 hours

                if (existingSubscription == null)
                {
                    // Create a new subscription and add it using the Graph API.
                    var sub = new Subscription
                    {
                        Resource = "me/events",
                        ChangeType = "created,updated",
                        NotificationUrl = notificationUrl,
                        ClientState = "ClientState",
                        ExpirationDateTime = newTime
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
        /// Creates or updates a subscription for online meeting transcripts in Microsoft Graph API.
        /// </summary>
        /// <param name="onlineMeetingId">The ID of the online meeting for which the subscription is created.</param>
        /// <param name="accessToken">The access token for making Graph API requests.</param>
        /// <returns>An IActionResult representing the HTTP response.</returns>
        public async Task<IActionResult> CreateTranscriptsSubscription(string onlineMeetingId, string accessToken)
        {
            try
            {
                IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;

                var graphClient = GraphClient.GetGraphClient(accessToken);
                try
                {
                    // Retrieve existing subscriptions using the Graph API.
                    existingSubscriptions = await graphClient.Subscriptions.Request().GetAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }

                // Define the notification URL for the subscription, typically an endpoint for handling notifications.
                var notificationUrl = _configuration["AzureAd:BaseUrlNgrok"] + "/CreateSubscriptionTranscripts";

                var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == "communications/onlineMeetings/" + onlineMeetingId + "/transcripts");

                // Check if an existing subscription with the same resource already exists and has a different notification URL.
                if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
                {
                    // If an existing subscription with a different notification URL is found, delete it.
                    await DeleteSubscription(existingSubscription);
                    existingSubscription = null;
                }

                // Get the current time and calculate an expiration time (typically 1 hour from now).
                DateTime currentDateTime = DateTime.UtcNow;
                DateTime expirationDateTime = currentDateTime + TimeSpan.FromHours(1);

                // Format the "expirationDateTime" as an ISO 8601 string with "Z" indicating UTC
                string expirationDateTimeString = expirationDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

                if (existingSubscription == null)
                {
                    // Create a new subscription and add it using the Graph API.
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
        /// Creates or updates a subscription for online meeting recordings in Microsoft Graph API.
        /// </summary>
        /// <param name="onlineMeetingId">The ID of the online meeting for which the subscription is created.</param>
        /// <param name="accessToken">The access token for making Graph API requests.</param>
        /// <returns>An IActionResult representing the HTTP response.</returns>
        public async Task<IActionResult> CreateRecordingsSubscription(string onlineMeetingId, string accessToken)
        {
            try
            {
                // Initialize a variable to hold existing subscriptions.
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
                    // If an existing subscription with a different notification URL is found, delete it.
                    await DeleteSubscription(existingSubscription);
                    existingSubscription = null;
                }

                // Get the current time and calculate an expiration time (typically 1 hour from now).
                DateTime currentDateTime = DateTime.UtcNow;
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
        /// Deletes a subscription in Microsoft Graph API on behalf of a user.
        /// </summary>
        /// <param name="subscription">The subscription to be deleted.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task DeleteSubscription(Subscription subscription)
        {
            // Obtain an access token on behalf of a user for making Graph API requests.
            var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

            // Create a Graph API client using the obtained access token.
            var graphClient = GraphClient.GetGraphClient(accessToken);

            try
            {
                // Attempt to delete the specified subscription using the Graph API.
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
        /// This action handles incoming HTTP POST requests for the "CreateSubscriptionTranscripts" route.
        /// It is responsible for processing validation tokens and notifications related to online meeting transcripts.
        /// </summary>
        /// <param name="validationToken">An optional validation token provided as a query parameter.</param>
        /// <returns>An ActionResult representing the HTTP response.</returns>
        [Route("CreateSubscriptionTranscripts")]
        [HttpPost]
        public async Task<ActionResult<string>> CreateSubscriptionTranscripts([FromQuery] string validationToken = null)
        {
            // Handle validation: Check if a validation token is provided in the query parameter.
            if (!string.IsNullOrEmpty(validationToken))
            {
                // Return the validation token as an OK response.
                return Ok(validationToken);
            }

            // Handle notifications: Read and process incoming notifications.
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                // Deserialize the JSON content into a C# object of type "Notifications."
                var notifications = JsonConvert.DeserializeObject<MeetingTranscriptRecording.Model.Notifications>(content);

                if (notifications != null)
                {
                    // Check if notifications are not null and if the first item has "ODataId."

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
                                // Retrieve the event details for "onlineMeetingId" from the data store.
                                TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetails);

                                // If event details are found, update the "transcriptsId" and store them back.
                                if (EventDetails != null)
                                {
                                    var OldEventDetails = EventDetails;

                                    if (OldEventDetails.transcriptsId != TranscriptsId)
                                    {
                                        // Update the "transcriptsId" in the event details.
                                        EventDetails.transcriptsId = TranscriptsId;
                                        EventDetails.signalRCondition = true;
                                        EventDetails.condition = true;

                                        // Try to update the event details in the data store.
                                        TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);

                                        var hubConnection = new HubConnectionBuilder().WithUrl(_configuration["AzureAd:BaseUrlNgrok"] + "/chatHub").Build();
                                        await hubConnection.StartAsync();
                                        await hubConnection.InvokeAsync("SendMessage", "TranscriptRecording", TranscriptRecordingEventDetails);
                                        await hubConnection.StopAsync();
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return Ok();
        }

        /// <summary>
        /// This action handles incoming HTTP POST requests for the "CreateSubscriptionRecordings" route.
        /// It processes validation tokens and notifications related to online meetings and recordings.
        /// </summary>
        /// <param name="validationToken">A validation token, if provided as a query parameter.</param>
        /// <returns>An ActionResult representing the HTTP response.</returns>
        [Route("CreateSubscriptionRecordings")]
        [HttpPost]
        public async Task<ActionResult<string>> CreateSubscriptionRecordings([FromQuery] string validationToken = null)
        {
            // Handle validation: Check if a validation token is provided in the query parameter.
            if (!string.IsNullOrEmpty(validationToken))
            {
                // Return the validation token as an OK response.
                return Ok(validationToken);
            }

            // Handle notifications: Read and process incoming notifications.
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                // Deserialize the JSON content into a C# object of type "Notifications."
                var notifications = JsonConvert.DeserializeObject<MeetingTranscriptRecording.Model.Notifications>(content);

                if (notifications != null)
                {
                    // Check if notifications are not null and if the first item has "ODataId."
                    if (notifications.Items[0].ResourceData.ODataId != null)
                    {
                        // Extract the start and end indexes for "onlineMeetingId" and "RecordingsId" in "ODataId."
                        int startIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("communications/onlineMeetings('");

                        int endIndex = notifications.Items[0].ResourceData.ODataId.IndexOf("')/recordings('");

                        // Ensure valid start and end indexes are found.
                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            // Extract "onlineMeetingId" from "ODataId."
                            string onlineMeetingId = notifications.Items[0].ResourceData.ODataId.Substring(startIndex + "communications/onlineMeetings('".Length, endIndex - startIndex - "communications/onlineMeetings('".Length);

                            // Extract "RecordingsId" from the notifications data.
                            string RecordingsId = notifications.Items[0].ResourceData.Id;

                            // Check if both "onlineMeetingId" and "RecordingsId" are not null.
                            if (onlineMeetingId != null && RecordingsId != null)
                            {
                                // Retrieve the event details for "onlineMeetingId" from the data store.
                                TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetails);

                                // If event details are found, update the "recordingId" and store them back.
                                if (EventDetails != null)
                                {
                                    var OldEventDetails = EventDetails;

                                    if (OldEventDetails.recordingId != RecordingsId)
                                    {
                                        EventDetails.recordingId = RecordingsId;
                                        EventDetails.signalRCondition = true;
                                        EventDetails.condition = true;

                                        // Try to update the event details in the data store.
                                        TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);

                                        var hubConnection = new HubConnectionBuilder().WithUrl(_configuration["AzureAd:BaseUrlNgrok"] + "/chatHub").Build();
                                        await hubConnection.StartAsync();
                                        await hubConnection.InvokeAsync("SendMessage", "TranscriptRecording", TranscriptRecordingEventDetails);
                                        await hubConnection.StopAsync();
                                    }
                                }

                            }

                        }
                    }
                }
            }
            return Ok(); // Return an OK response to indicate that the processing was successful.
        }


        /// <summary>
        /// Get Token for given tenant.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<string> GetToken()
        {
            try
            {
                // Create a ConfidentialClientApplication to acquire an access token
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_configuration["AzureAd:MicrosoftAppId"])
                                                  .WithClientSecret(_configuration["AzureAd:MicrosoftAppPassword"])
                                                  .WithAuthority($"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

                // Define the required scopes for accessing Microsoft Graph API
                string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

                // Acquire an access token for the specified scopes
                var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

                // Return the access token obtained from the authentication process
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during token acquisition
                return null; // Return null to indicate failure in obtaining the token
            }
        }

        /// <summary>
        /// Handles incoming notifications for subscription events.
        /// </summary>
        /// <param name="validationToken">The validation token for subscription validation.</param>
        /// <returns>An ActionResult representing the result of the operation.</returns>
        [Route("CreateSubscriptionMeEventsPost")]
        [HttpPost]
        public async Task<ActionResult<string>> CreateSubscriptionMeEventsPost([FromQuery] string validationToken = null)
        {
            // Handle validation token
            if (!string.IsNullOrEmpty(validationToken))
            {
                return Ok(validationToken);
            }

            // Handle notifications received in the request body
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                // Deserialize the received JSON notifications
                var notifications = JsonConvert.DeserializeObject<MeetingTranscriptRecording.Model.Notifications>(content);

                // Get an access token for making API requests
                var accessToken = await GetToken();

                // Retrieve the base URL from configuration
                var BaseURL = _configuration["AzureAd:BaseURL"];

                // Check if there are valid notifications and resources in them
                if (notifications != null && notifications.Items[0].Resource != null)
                {
                    // Construct the Graph API endpoint for events
                    string graphApiEndpointEvents = BaseURL + notifications.Items[0].Resource;

                    // Make an authenticated API request to get event data
                    var responseBody = await AuthHelper.GetApiData(graphApiEndpointEvents, accessToken);

                    // Deserialize the event data
                    var responseData = JsonConvert.DeserializeObject<EventData>(responseBody);

                    if (responseData != null)
                    {
                        if (responseData.isOnlineMeeting == true)
                        {
                            string joinUrl = responseData.onlineMeeting.joinUrl;

                            // Define a regex pattern to extract a user ID
                            string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

                            // Use Regex.Match to find the user ID in the resource URL
                            Match match = Regex.Match(notifications.Items[0].Resource, pattern);

                            // Extract the user ID
                            string userId = match.Value;

                            // Construct the Graph API endpoint to retrieve online meeting details
                            string graphApiEndpointJoinUrl = $"https://graph.microsoft.com/v1.0/users/" + userId + "/onlineMeetings?$filter=JoinWebUrl%20eq%20'" + joinUrl + "'";

                            var responseBodyJoinUrl = await AuthHelper.GetApiData(graphApiEndpointJoinUrl, accessToken);

                            // Make an authenticated API request to get online meeting details
                            var responseJoinUrlData = JsonConvert.DeserializeObject<JoinUrlData>(responseBodyJoinUrl);

                            if (responseJoinUrlData != null && responseJoinUrlData.Value.Count > 0 && responseJoinUrlData.Value[0].id != null)
                            {
                                // Get the online meeting ID
                                string onlineMeetingId = responseJoinUrlData.Value[0].id;

                                // Retrieve event details from a dictionary
                                TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetails);

                                if (EventDetails != null)
                                {
                                    // Store the old event details for comparison
                                    var OldEventDetails = EventDetails;

                                    // Update event details if they have changed
                                    if (OldEventDetails.subject != responseData.subject)
                                    {
                                        EventDetails.subject = responseData.subject;

                                        TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);
                                    }

                                    if (OldEventDetails.start.ToString() != responseData.start.dateTime.ToString("MMM dd h:mm tt"))
                                    {
                                        EventDetails.start = responseData.start.dateTime.ToString("MMM dd h:mm tt");

                                        TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);
                                    }

                                    if (OldEventDetails.end.ToString() != responseData.end.dateTime.ToString("MMM dd h:mm tt"))
                                    {
                                        EventDetails.end = responseData.end.dateTime.ToString("MMM dd h:mm tt");

                                        TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetails, OldEventDetails);
                                    }
                                }

                                // Construct the Graph API endpoint to retrieve online transcripts
                                string graphApiEndpointOnlineTranscripts = BaseURL + $"users/" + userId + $"/onlineMeetings/" + onlineMeetingId + "/transcripts";

                                // Make an authenticated API request to get online transcripts
                                var responseBodyTranscripts = await AuthHelper.GetApiData(graphApiEndpointOnlineTranscripts, accessToken);

                                // Deserialize the response data for online transcripts
                                var responseTranscriptsData = JsonConvert.DeserializeObject<transcriptsData>(responseBodyTranscripts);

                                if (responseTranscriptsData != null && responseTranscriptsData.Value.Count > 0 && responseTranscriptsData.Value[0].id != null)
                                {
                                    // Get the transcripts ID
                                    string getTranscriptsId = responseTranscriptsData.Value[0].id;

                                    // Retrieve event details for transcripts from a dictionary
                                    TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetailsTranscript);

                                    if (EventDetailsTranscript != null)
                                    {
                                        // Store the old event details for transcripts for comparison
                                        var OldEventDetailsTranscript = EventDetailsTranscript;

                                        // Update transcripts ID if it has changed
                                        if (OldEventDetailsTranscript.transcriptsId != getTranscriptsId)
                                        {
                                            EventDetailsTranscript.transcriptsId = getTranscriptsId;

                                            TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetailsTranscript, OldEventDetailsTranscript);
                                        }
                                    }
                                }

                                // Construct the Graph API endpoint to retrieve online recordings
                                string graphApiEndpointOnlineRecordings = BaseURL + $"users/" + userId + $"/onlineMeetings/" + onlineMeetingId + "/recordings";

                                // Make an authenticated API request to get online recordings
                                var responseBodyRecordings = await AuthHelper.GetApiData(graphApiEndpointOnlineRecordings, accessToken);

                                // Deserialize the response data for online recordings
                                var responseRecordingsData = JsonConvert.DeserializeObject<RecordingData>(responseBodyRecordings);

                                if (responseRecordingsData != null && responseRecordingsData.Value.Count > 0 && responseRecordingsData.Value[0].id != null)
                                {
                                    // Get the recording ID
                                    string getRecordingId = responseRecordingsData.Value[0].id;

                                    // Retrieve event details for recordings from a dictionary
                                    TranscriptRecordingEventDetails.TryGetValue(onlineMeetingId, out CardData EventDetailsRecord);

                                    if (EventDetailsRecord != null)
                                    {
                                        // Store the old event details for recordings for comparison
                                        var OldEventDetailsRecord = EventDetailsRecord;

                                        // Update recording ID if it has changed
                                        if (OldEventDetailsRecord.recordingId != getRecordingId)
                                        {
                                            EventDetailsRecord.recordingId = getRecordingId;

                                            TranscriptRecordingEventDetails.TryUpdate(onlineMeetingId, EventDetailsRecord, OldEventDetailsRecord);
                                        }
                                    }
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