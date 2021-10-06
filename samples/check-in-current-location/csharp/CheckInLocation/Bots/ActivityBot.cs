// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using CheckInLocation.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckInLocation.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        private readonly IWebHostEnvironment _env;

        public ActivityBot(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if(turnContext.Activity.Text.ToLower() == "view last check in")
            {
                var fileName = Path.Combine(_env.ContentRootPath, $".\\wwwroot\\UserLocationdetails.json");
                string Json = File.ReadAllText(fileName);
                List<UserDetails> userDetailsList = new List<UserDetails>();

                if (Json == "")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No last check in found"));
                }
                else
                {
                    userDetailsList = JsonConvert.DeserializeObject<List<UserDetails>>(Json);
                    List<UserDetails> userCheckInList = new List<UserDetails>();

                    foreach (var userDetail in userDetailsList)
                    {
                        if (userDetail.UserId == turnContext.Activity.From.AadObjectId)
                        {
                            userCheckInList.Add(userDetail);
                        }
                    }

                    if (userCheckInList.Count == 0)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("No last check in found"));
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUserLastCheckIn(userCheckInList)), cancellationToken);
                    }
                }                       
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
            }            
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample your bot can get your current location and all your last check in's. Please type 'hey' or 'check in' to get check in card or type 'view last check in' to get all check in details"), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handle task module is fetch.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var buttonType = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Id;
            var taskModuleResponse = new TaskModuleResponse();

            if (buttonType == "checkin")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/" + "CheckIn",
                        Height = 350,
                        Width = 350,
                        Title = "Check in details",
                    },
                };
            }
            else if (buttonType == "viewLocation")
            {
                var latitude = (double)asJobject.ToObject<LocationDetails<double>>()?.Latitude;
                var longitude = (double)asJobject.ToObject<LocationDetails<double>>()?.Longitude;
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/" + "ViewLocation?latitude="+ latitude+"&longitude="+longitude,
                        Height = 350,
                        Width = 350,
                        Title = "View location",
                    },
                };
            }

            return Task.FromResult(taskModuleResponse);
        }

        /// <summary>
        /// Handle task module is submit.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var locationInfo = JObject.FromObject(taskModuleRequest.Data);
            var latitude = (double)locationInfo.ToObject<LocationDetails<double>>()?.Latitude;
            var longitude = (double)locationInfo.ToObject<LocationDetails<double>>()?.Longitude;
            string user = turnContext.Activity.From.Name;
            string time = turnContext.Activity.LocalTimestamp.ToString();

            UserDetails userDetails = new UserDetails {
                CheckInTime = time,
                UserName = user,
                Longitude = longitude,
                Latitude = latitude,
                UserId = turnContext.Activity.From.AadObjectId
            };

            SaveUserDetails(userDetails);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUserLocation(time, user, latitude, longitude)), cancellationToken);

            return null;
        }

        /// <summary>
        /// Sample Adaptive card for check in button.
        /// </summary>
        private Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Please click on check in",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Check in",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id="checkin"
                        },
                    }
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for user current location info.
        /// </summary>
        private Attachment GetAdaptiveCardForUserLocation(string time, string user, double latitude, double longitude)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"User name :{user}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"Check in time: {time}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "View location",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id = "viewLocation",
                            Latitude = latitude,
                            Longitude = longitude
                        },
                    }
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for user's last check in's.
        /// </summary>
        private List<Attachment> GetAdaptiveCardForUserLastCheckIn(List<UserDetails> userDetails)
        {
            List<Attachment> attachmentList = new List<Attachment>();

            foreach (var user in userDetails)
            {
                AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = $"User name :{user.UserName}",
                            Weight = AdaptiveTextWeight.Bolder,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true
                        },
                        new AdaptiveTextBlock
                        {
                            Text = $"Check in time: {user.CheckInTime}",
                            Weight = AdaptiveTextWeight.Bolder,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true
                        },
                        new AdaptiveTextBlock
                        {
                            Text = $"Check in latitude: {user.Latitude}",
                            Weight = AdaptiveTextWeight.Bolder,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true
                        },
                        new AdaptiveTextBlock
                        {
                            Text = $"Check in longitude: {user.Longitude}",
                            Weight = AdaptiveTextWeight.Bolder,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true
                        }
                    }
                };

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card,
                };

                attachmentList.Add(attachment);
            }

            return attachmentList;
        }

        // Save user details in json file.
        private void SaveUserDetails(UserDetails userDetails)
        {
            var fileName = Path.Combine(_env.ContentRootPath, $".\\wwwroot\\UserLocationdetails.json");
            string Json = File.ReadAllText(fileName);
            List<UserDetails> userDetailsList = new List<UserDetails>();

            if (Json == "")
            {
                userDetailsList.Add(userDetails);
            }
            else {
                userDetailsList = JsonConvert.DeserializeObject<List<UserDetails>>(Json);
                userDetailsList.Add(userDetails);
            }

            File.WriteAllText(fileName, JsonConvert.SerializeObject(userDetailsList));
        }
    }
}