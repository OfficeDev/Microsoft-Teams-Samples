// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AppCheckinLocation.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AppCheckinLocation.Controllers
{
    /// <summary>
    /// Handles location check-in functionality with task modules
    /// </summary>
    [TeamsController]
    public class Controller
    {
        private readonly string _applicationBaseUrl;
        private static readonly ConcurrentDictionary<string, UserLocationDetail> _userLocationData = new();

        public Controller(IConfiguration configuration)
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Handles conversation members added event
        /// Sends welcome message with check-in instructions
        /// </summary>
        [Conversation.MembersAdded]
        public async Task OnMembersAdded([Context] ConversationUpdateActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            foreach (var member in activity.MembersAdded)
            {
                if (member.Id != activity.Recipient.Id)
                {
                    await client.Send("Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin').");
                }
            }
        }

        /// <summary>
        /// Handles incoming messages
        /// </summary>
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            if (string.IsNullOrEmpty(activity.Text))
            {
                var card = GetAdaptiveCardForTaskModule();
                if (card != null)
                {
                    await client.Send(card);
                }
                return;
            }

            var text = activity.Text.ToLower().Trim();
            if (text == "viewcheckin")
            {
                var userId = activity.From.AadObjectId ?? activity.From.Id;                
                if (_userLocationData.TryGetValue(userId, out var currentUserDetail) && currentUserDetail.UserDetails != null)
                {
                    var attachments = GetAdaptiveCardForUserLastCheckIn(currentUserDetail.UserDetails);
                    foreach (var attachment in attachments)
                    {
                        if (attachment != null)
                        {
                            await client.Send(attachment);
                        }
                    }
                }
                else
                {
                    await client.Send("No last check in found");
                }
            }
            else
            {
                var card = GetAdaptiveCardForTaskModule();
                if (card != null)
                {
                    await client.Send(card);
                }
            }
        }

        /// <summary>
        /// Handles task/fetch invoke - Opens task module for check-in or location view
        /// </summary>
        [Microsoft.Teams.Apps.Activities.Invokes.Invoke("task/fetch")]
        public object OnTaskFetch([Context] InvokeActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            if (activity.Value == null)
            {
                return new
                {
                    task = new
                    {
                        type = "continue",
                        value = new
                        {
                            url = $"{_applicationBaseUrl}/CheckIn",
                            height = 350,
                            width = 350,
                            title = "Check in details"
                        }
                    }
                };
            }

            var valueJson = JsonSerializer.Serialize(activity.Value);
            var taskData = JsonSerializer.Deserialize<JsonElement>(valueJson);            
            JsonElement dataProperty = taskData;
            if (taskData.TryGetProperty("data", out var dataNode))
            {
                dataProperty = dataNode;
            }
            string? buttonType = null;
            if (dataProperty.TryGetProperty("id", out var idProperty))
            {
                buttonType = idProperty.GetString();
            }

            if (buttonType == "checkin")
            {
                return new
                {
                    task = new
                    {
                        type = "continue",
                        value = new
                        {
                            url = $"{_applicationBaseUrl}/CheckIn",
                            height = 350,
                            width = 350,
                            title = "Check in details"
                        }
                    }
                };
            }
            else if (buttonType == "viewLocation")
            {
                var latitude = dataProperty.GetProperty("latitude").GetDouble();
                var longitude = dataProperty.GetProperty("longitude").GetDouble();
                return new
                {
                    task = new
                    {
                        type = "continue",
                        value = new
                        {
                            url = $"{_applicationBaseUrl}/ViewLocation?latitude={latitude}&longitude={longitude}",
                            height = 350,
                            width = 350,
                            title = "View location"
                        }
                    }
                };
            }
            return new { task = new { type = "continue" } };
        }

        /// <summary>
        /// Handles task/submit invoke - Saves location data and sends confirmation
        /// </summary>
        [Microsoft.Teams.Apps.Activities.Invokes.Invoke("task/submit")]
        public async Task<object> OnTaskSubmit([Context] InvokeActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var activityJson = JsonSerializer.Serialize(activity);
            var activityElement = JsonSerializer.Deserialize<JsonElement>(activityJson);           
            if (!activityElement.TryGetProperty("value", out var valueElement))
            {
                await client.Send("Error: No location data received");
                return new { };
            }            
            var taskData = valueElement;
            var latitude = latElement.GetDouble();
            var longitude = lonElement.GetDouble();
            var user = activity.From?.Name ?? "User";
            var time = activity.LocalTimestamp?.ToString() ?? DateTime.Now.ToString();
            var userId = activity.From?.AadObjectId ?? activity.From?.Id ?? "unknown";
            var userDetails = new UserDetail
            {
                CheckInTime = time,
                UserName = user,
                Longitude = longitude,
                Latitude = latitude,
                UserId = userId
            };
            SaveUserDetails(userId, userDetails);
            await client.Send(GetAdaptiveCardForUserLocation(time, user, latitude, longitude));
            return null;
        }

        /// <summary>
        /// Adaptive card for check-in button
        /// </summary>
        private dynamic? GetAdaptiveCardForTaskModule()
        {
            var cardJson = @"{
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.4"",
                ""body"": [
                    {
                        ""type"": ""TextBlock"",
                        ""text"": ""Please click on check in"",
                        ""weight"": ""Bolder"",
                        ""spacing"": ""Medium""
                    }
                ],
                ""actions"": [
                    {
                        ""type"": ""Action.Submit"",
                        ""title"": ""Check in"",
                        ""data"": {
                            ""msteams"": {
                                ""type"": ""task/fetch""
                            },
                            ""id"": ""checkin""
                        }
                    }
                ]
            }";
            return AdaptiveCard.Deserialize(cardJson);
        }

        /// <summary>
        /// Adaptive card for user's current location info
        /// </summary>
        private dynamic? GetAdaptiveCardForUserLocation(string time, string user, double latitude, double longitude)
        {
            var cardJson = $@"{{
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.4"",
                ""body"": [
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""User name: {user}"",
                        ""weight"": ""Bolder"",
                        ""spacing"": ""Medium"",
                        ""wrap"": true
                    }},
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""Check in time: {time}"",
                        ""weight"": ""Bolder"",
                        ""spacing"": ""Medium"",
                        ""wrap"": true
                    }}
                ],
                ""actions"": [
                    {{
                        ""type"": ""Action.Submit"",
                        ""title"": ""View location"",
                        ""data"": {{
                            ""msteams"": {{
                                ""type"": ""task/fetch""
                            }},
                            ""id"": ""viewLocation"",
                            ""latitude"": {latitude},
                            ""longitude"": {longitude}
                        }}
                    }}
                ]
            }}";
            return AdaptiveCard.Deserialize(cardJson);
        }

        /// <summary>
        /// Adaptive cards for user's last check-ins
        /// </summary>
        private List<dynamic?> GetAdaptiveCardForUserLastCheckIn(List<UserDetail> userDetails)
        {
            var cardList = new List<dynamic?>();

            foreach (var user in userDetails)
            {
                var cardJson = $@"{{
                    ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                    ""type"": ""AdaptiveCard"",
                    ""version"": ""1.4"",
                    ""body"": [
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""User name: {user.UserName}"",
                            ""weight"": ""Bolder"",
                            ""spacing"": ""Medium"",
                            ""wrap"": true
                        }},
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""Check in time: {user.CheckInTime}"",
                            ""weight"": ""Bolder"",
                            ""spacing"": ""Medium"",
                            ""wrap"": true
                        }}
                    ],
                    ""actions"": [
                        {{
                            ""type"": ""Action.Submit"",
                            ""title"": ""View location"",
                            ""data"": {{
                                ""msteams"": {{
                                    ""type"": ""task/fetch""
                                }},
                                ""id"": ""viewLocation"",
                                ""latitude"": {user.Latitude},
                                ""longitude"": {user.Longitude}
                            }}
                        }}
                    ]
                }}";
                cardList.Add(AdaptiveCard.Deserialize(cardJson));
            }
            return cardList;
        }

        /// <summary>
        /// Saves user check-in details (up to 10 check-ins per user)
        /// </summary>
        private void SaveUserDetails(string userId, UserDetail userDetails)
        {
            var userLocationDetail = _userLocationData.GetOrAdd(userId, _ => new UserLocationDetail { UserDetails = new List<UserDetail>() });

            if (userLocationDetail.UserDetails == null)
            {
                userLocationDetail.UserDetails = new List<UserDetail> { userDetails };
            }
            else if (userLocationDetail.UserDetails.Count == 10)
            {
                userLocationDetail.UserDetails.RemoveAt(0);
                userLocationDetail.UserDetails.Add(userDetails);
            }
            else
            {
                userLocationDetail.UserDetails.Add(userDetails);
            }
        }
    }
}