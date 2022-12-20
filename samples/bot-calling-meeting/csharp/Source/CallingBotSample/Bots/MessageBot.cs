// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CallingBotSample.AdaptiveCards;
using CallingBotSample.Cache;
using CallingBotSample.Helpers;
using CallingBotSample.Models;
using CallingBotSample.Options;
using CallingBotSample.Services.MicrosoftGraph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MeetingInfo = Microsoft.Graph.MeetingInfo;

namespace CallingBotSample.Bots
{
    public class MessageBot : TeamsActivityHandler
    {
        private readonly IAdaptiveCardFactory adaptiveCardFactory;
        private readonly AudioRecordingConstants audioRecordingConstants;

        private readonly ICallService callService;
        private readonly IChatService chatService;
        private readonly IOnlineMeetingService onlineMeetingService;
        private readonly IIncidentCache incidentCache;

        private readonly AzureAdOptions azureAdOptions;
        private readonly BotOptions botOptions;
        private readonly ILogger<MessageBot> logger;

        public MessageBot(
            IAdaptiveCardFactory adaptiveCardFactory,
            AudioRecordingConstants audioRecordingConstants,
            ICallService callService,
            IChatService chatService,
            IOnlineMeetingService onlineMeetingService,
            IIncidentCache incidentCache,
            IOptions<AzureAdOptions> azureAdOptions,
            IOptions<BotOptions> botOptions,
            ILogger<MessageBot> logger)
        {
            this.adaptiveCardFactory = adaptiveCardFactory;
            this.audioRecordingConstants = audioRecordingConstants;

            this.callService = callService;
            this.chatService = chatService;
            this.onlineMeetingService = onlineMeetingService;
            this.incidentCache = incidentCache;

            this.azureAdOptions = azureAdOptions.Value;
            this.botOptions = botOptions.Value;
            this.logger = logger;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                dynamic value = turnContext.Activity.Value;
                if (value != null)
                {
                    string type = value["type"];
                    type = string.IsNullOrEmpty(type) ? "." : type.ToLower();
                    string? callId = value["callId"] ?? null;
                    await SendResponse(turnContext, type, callId, cancellationToken);
                }
            }
            else
            {
                turnContext.Activity.RemoveRecipientMention();
                await SendResponse(turnContext, turnContext.Activity.Text.Trim().ToLower(), null, cancellationToken);
            }
        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var fetchData = asJobject.ToObject<TaskModuleFetchData>();

            var taskInfo = new TaskModuleTaskInfo();

            switch (fetchData?.Action)
            {
                // Opens a module with a people picker where users can be selected. Later those user will be used to create a call
                case "createcall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to create a call with:", "Create", callId: null, isMultiSelect: true);
                    taskInfo.Title = "Create call";
                    break;
                // Opens a module with a people picker where a user can be selected to transfer the current call to
                case "transfercall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to transfer the call to:", "Transfer", fetchData?.CallId);
                    taskInfo.Title = "Transfer call";
                    break;
                // Opens a module with a people picker where a user can be selected to invite a participant to the current call
                case "inviteparticipant":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to invite to the call:", "Invite", fetchData?.CallId);
                    taskInfo.Title = "Select the user to invite";
                    break;
                // Opens a modules with a form to create an incident. This includes a incident title, and those who should be on the call.
                case "openincidenttask":
                    taskInfo.Card = adaptiveCardFactory.CreateIncidentCard();
                    taskInfo.Title = "Create incident";
                    break;
                default:
                    break;
            }

            return Task.FromResult(new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = taskInfo,
                },
            });
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var moduleSubmitData = asJobject.ToObject<TaskModuleSubmitData>();
            var peoplePicker = moduleSubmitData?.PeoplePicker;

            if (peoplePicker != null)
            {
                // Adaptive Card people picker returns a comma separated list of aad IDs
                var peoplePickerAadIds = peoplePicker.Split(',');
                var action = moduleSubmitData?.Action?.ToLowerInvariant();
                var callId = moduleSubmitData?.CallId;

                try
                {
                    switch (action)
                    {
                        case "create":
                            var call = await callService.Create(users: peoplePickerAadIds.Select(p => new Identity { Id = p }).ToArray());

                            if (call != null)
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateMeetingActionsCard(call.Id)));

                                return await CreateTaskModuleMessageResponse("Working on that, you can close this dialog now.");
                            }
                            break;
                        case "transfer":
                            return await MakeGraphCallThatMightNotBeFound(
                                () => callService.Transfer(
                                        callId!,
                                        new Identity { Id = peoplePicker }),
                                callId,
                                CreateTaskModuleMessageResponse);
                        case "invite":
                            return await MakeGraphCallThatMightNotBeFound(
                                () => callService.InviteParticipant(
                                        callId!,
                                        new IdentitySet { User = new Identity { Id = peoplePicker } }),
                                callId,
                                CreateTaskModuleMessageResponse);
                        case "createincident":
                            if (moduleSubmitData?.IncidentName != null)
                            {
                                return await CreateIncidentCall(
                                    turnContext,
                                    moduleSubmitData.IncidentName,
                                    peoplePickerAadIds,
                                    cancellationToken);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (ServiceException ex)
                {
                    logger.LogError(ex, "Failure while making Graph Call");
                    return await CreateTaskModuleMessageResponse($"Something went wrong 😖. {ex.Message}");
                }
            }

            return await CreateTaskModuleMessageResponse("Something went wrong 😖");
        }

        private async Task SendResponse(ITurnContext<IMessageActivity> turnContext, string input, string? callId, CancellationToken cancellationToken)
        {
            switch (input)
            {
                case "playrecordprompt":
                    await MakeGraphCallThatMightNotBeFound(
                        () => callService.Record(callId!, audioRecordingConstants.PleaseRecordYourMessage),
                        callId,
                        (message) => UpdateActivityAsync(message, turnContext, cancellationToken));
                    break;
                case "hangup":
                    await MakeGraphCallThatMightNotBeFound(
                        () => callService.HangUp(callId!),
                        callId,
                        (message) => UpdateActivityAsync(message, turnContext, cancellationToken));
                    break;
                case "joinscheduledmeeting":
                    if (turnContext.Activity.ChannelData["meeting"] != null)
                    {
                        var call = await JoinScheduledMeeting(turnContext, cancellationToken);

                        if (call != null)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateMeetingActionsCard(call.Id)));
                        }
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Meeting not found. Are you calling this from a meeting chat?", cancellationToken: cancellationToken);
                    }
                    break;
                default:
                    await turnContext.SendActivityAsync(
                        MessageFactory.Attachment(
                            adaptiveCardFactory.CreateWelcomeCard(turnContext.Activity.ChannelData["meeting"] != null)), cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// Wrapper around Graph calls. Handles cases where a call is not found.
        /// This might happen when an API call is made using a CallId that has already ended
        /// </summary>
        /// <typeparam name="TResult">Return value <paramref name="errorHandler"/></typeparam>
        /// <param name="function">Function to call</param>
        /// <param name="callId">Call's id</param>
        /// <param name="errorHandler">Handler for when there is an error</param>
        /// <returns>A Task, except when there is an error when <typeparamref name="TResult"/> will be returned</returns>
        private async Task<TResult> MakeGraphCallThatMightNotBeFound<TResult>(Func<Task> function, string? callId, Func<string, Task<TResult>> errorHandler)
        {
            if (callId == null)
            {
                // Without the Meeting ID we are unable to play the prompt
                return await errorHandler("Meeting ID not found, please use the 'Create call' button to create the call.");
            }

            try
            {
                await function();
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    // If it's not a NotFound error please ignore
                    throw ex;
                }

                logger.LogError("Call not found. Return error");
                return await errorHandler("That action failed. Unable to find call");
            }

            return default;
        }

        private async Task<Call> JoinScheduledMeeting(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var users = await TeamsInfo.GetPagedMembersAsync(turnContext, cancellationToken: cancellationToken);

            var organiser = new Identity
            {
                // This needs to be the organiser of the meeting, so you can't use the activity invoker
                Id = users.Members[0].AadObjectId,
            };

            var channelDataTenant = JObject.Parse(JsonConvert.SerializeObject(turnContext.Activity.ChannelData)).SelectToken("tenant");
            organiser.SetTenantId(channelDataTenant["id"].ToString());

            return await callService.Create(
                new ChatInfo {
                    ThreadId = turnContext.Activity.Conversation.Id,
                    // NOTE: If you don't provide a Message Id, users will not be able to join the call the bot creates.
                    MessageId = "0"
                },
                new OrganizerMeetingInfo
                {
                    Organizer = new IdentitySet
                    {
                        User = organiser
                    },
                });
        }

        private async Task<TaskModuleResponse> CreateIncidentCall(ITurnContext turnContext, string incidentSubject, string[] peoplePickerAadIds, CancellationToken cancellationToken)
        {
            var onlineMeeting = await onlineMeetingService.Create(incidentSubject, peoplePickerAadIds);

            if (onlineMeeting != null)
            {
                (ChatInfo chatInfo, MeetingInfo meetingInfo) = JoinInfo.ParseJoinURL(onlineMeeting.JoinWebUrl);

                var meetingCall = await callService.Create(chatInfo, meetingInfo);

                if (meetingCall != null)
                {
                    await chatService.InstallApp(meetingCall.ChatInfo.ThreadId, botOptions.CatalogAppId);

                    var incidentDetails = new IncidentDetails
                    {
                        CallId = meetingCall.Id,
                        IncidentSubject = incidentSubject,
                        MeetingInfo = meetingInfo,
                        ChatInfo = chatInfo,
                        StartTime = DateTime.Now,
                        Participants = peoplePickerAadIds.Select(p => new Identity
                        {
                            Id = p,
                        })
                    };
                    incidentCache.Set(meetingCall.Id, incidentDetails);

                    await SendActivityToConversation(
                        turnContext,
                        chatInfo.ThreadId,
                        MessageFactory.Attachment(adaptiveCardFactory.CreateIncidentMeetingCard(
                            incidentDetails.IncidentSubject,
                            incidentDetails.CallId,
                            incidentDetails.StartTime,
                            null
                        )),
                        cancellationToken);

                    await turnContext.SendActivityAsync("Created incident call successfully.", cancellationToken: cancellationToken);
                }

                return await CreateTaskModuleMessageResponse("Working on that, you can close this dialog now.");
            }

            return await CreateTaskModuleMessageResponse("Something went wrong 😖");
        }

        private async Task SendActivityToConversation(ITurnContext turnContext, string conversationId, IActivity activity, CancellationToken cancellationToken)
        {
            var newReference = new ConversationReference
            {
                Conversation = new ConversationAccount
                {
                    Id = conversationId
                },
                ServiceUrl = turnContext.Activity.ServiceUrl
            };

            await (turnContext.Adapter).ContinueConversationAsync(
                azureAdOptions.ClientId,
                newReference,
                async (ITurnContext turnContext, CancellationToken cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                },
                cancellationToken);
        }

        private async Task<TaskModuleResponse> CreateTaskModuleMessageResponse(string value)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = value
                },
            };
        }

        private async Task<bool> UpdateActivityAsync(string responseText, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var updatedActivity = MessageFactory.Text(responseText);
            updatedActivity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(updatedActivity, cancellationToken);
            return true;
        }
    }
}
