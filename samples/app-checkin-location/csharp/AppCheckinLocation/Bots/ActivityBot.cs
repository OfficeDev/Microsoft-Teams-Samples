// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AppCheckinLocation.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppCheckinLocation.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        private readonly IWebHostEnvironment _env;
        protected readonly BotState _conversationState;
        protected readonly IStatePropertyAccessor<UserLocationDetail> _UserDetail;

        public ActivityBot(IConfiguration configuration, IWebHostEnvironment env, ConversationState conversationState)
        {
            _conversationState = conversationState;
            _env = env;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            _UserDetail = conversationState.CreateProperty<UserLocationDetail>(nameof(UserLocationDetail));
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if(turnContext.Activity.Text.ToLower().Trim() == "viewcheckin")
            {
                var currentUserDetail = await this._UserDetail.GetAsync(turnContext, () => new UserLocationDetail());
                List<UserDetail> userDetailsList = new List<UserDetail>();

                if (currentUserDetail.UserDetails == null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No last check in found"));
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForUserLastCheckIn(currentUserDetail.UserDetails)), cancellationToken);
                }                       
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
            }            
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
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
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin')."), cancellationToken);
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

            UserDetail userDetails = new UserDetail {
                CheckInTime = time,
                UserName = user,
                Longitude = longitude,
                Latitude = latitude,
                UserId = turnContext.Activity.From.AadObjectId
            };

            await SaveUserDetailsAsync(turnContext,userDetails);
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
        private List<Attachment> GetAdaptiveCardForUserLastCheckIn(List<UserDetail> userDetails)
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
                                Latitude = user.Latitude,
                                Longitude = user.Longitude
                            },
                        }
                    },
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
        private async Task SaveUserDetailsAsync(ITurnContext<IInvokeActivity> turnContext, UserDetail userDetails)
        {
            var currentUserDetail = await this._UserDetail.GetAsync(turnContext, () => new UserLocationDetail());
            List<UserDetail> userDetailsList = new List<UserDetail>();
            if (currentUserDetail.UserDetails == null)
            {
                userDetailsList.Add(userDetails);
                currentUserDetail.UserDetails = userDetailsList;
                await this._UserDetail.SetAsync(turnContext, currentUserDetail);
            }
            else if (currentUserDetail.UserDetails.Count == 10)
            {
                currentUserDetail.UserDetails.RemoveAt(0); 
                userDetailsList = currentUserDetail.UserDetails;
                userDetailsList.Add(userDetails);
                currentUserDetail.UserDetails = userDetailsList;
                await this._UserDetail.SetAsync(turnContext, currentUserDetail);
            }
            else
            {
                userDetailsList = currentUserDetail.UserDetails;
                userDetailsList.Add(userDetails);
                currentUserDetail.UserDetails = userDetailsList;
                await this._UserDetail.SetAsync(turnContext, currentUserDetail);
            }
        }
    }
}