// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CallingBotSample.AdaptiveCards;
using CallingBotSample.Helpers;
using CallingBotSample.Models;
using CallingBotSample.Options;
using CallingBotSample.Services.MicrosoftGraph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
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
        private readonly IMemoryCache callBotCache;

        private readonly AzureAdOptions azureAdOptions;
        private readonly ILogger<MessageBot> logger;

        public MessageBot(
            IAdaptiveCardFactory adaptiveCardFactory,
            AudioRecordingConstants audioRecordingConstants,
            ICallService callService,
            IChatService chatService,
            IOnlineMeetingService onlineMeetingService,
            IMemoryCache callBotCache,
            IOptions<AzureAdOptions> azureAdOptions,
            ILogger<MessageBot> logger)
        {
            this.adaptiveCardFactory = adaptiveCardFactory;
            this.audioRecordingConstants = audioRecordingConstants;

            this.callService = callService;
            this.chatService = chatService;
            this.onlineMeetingService = onlineMeetingService;
            this.callBotCache = callBotCache;

            this.azureAdOptions = azureAdOptions.Value;
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
                case "createcall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to create a call with:", "Create", null, isMultiSelect: true);
                    taskInfo.Title = "Create call";
                    break;
                case "transfercall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to transfer the call to:", "Transfer", fetchData?.CallId);
                    taskInfo.Title = "Transfer call";
                    break;
                case "inviteparticipant":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to invite to the call:", "Invite", fetchData?.CallId);
                    taskInfo.Title = "Select the user to invite";
                    break;
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

                                return await CreateTaskModuleResponse("Working on that, you can close this dialog now.");
                            }
                            break;
                        case "transfer":
                            return await MakeGraphCallThatMightNotBeFound(
                                () => callService.Transfer(
                                        callId!,
                                        new Identity { Id = peoplePicker }),
                                callId,
                                CreateTaskModuleResponse);
                        case "invite":
                            return await MakeGraphCallThatMightNotBeFound(
                                () => callService.InviteParticipant(
                                        callId!,
                                        new IdentitySet { User = new Identity { Id = peoplePicker } }),
                                callId,
                                CreateTaskModuleResponse);
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
                    return await CreateTaskModuleResponse($"Something went wrong 😖. {ex.Message}");
                }
            }

            return await CreateTaskModuleResponse("Something went wrong 😖");
        }

        private async Task SendResponse(ITurnContext<IMessageActivity> turnContext, string input, string? callId, CancellationToken cancellationToken)
        {
            switch (input)
            {
                case "playrecordprompt":
                    await MakeGraphCallThatMightNotBeFound(
                        () => callService.Record(callId!, audioRecordingConstants.PleaseRecordYourMessage),
                        callId,
                        (message) => SendActivityResponse(message, turnContext, cancellationToken));
                    break;
                case "hangup":
                    await MakeGraphCallThatMightNotBeFound(
                        () => callService.HangUp(callId!),
                        callId,
                        (message) => SendActivityResponse(message, turnContext, cancellationToken));
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

        private async Task<TResult> MakeGraphCallThatMightNotBeFound<TResult>(Func<Task> function, string? callId, Func<string, Task<TResult>> errorHandler)
        {
            if (callId == null)
            {
                // Without the Meeting ID we are unable to play the prompt
                //await turnContext.SendActivityAsync("Meeting ID not found, please use the 'Create call' button to create the call.");
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

            return await callService.Create(
                new ChatInfo { ThreadId = turnContext.Activity.Conversation.Id },
                new OrganizerMeetingInfo
                {
                    Organizer = new IdentitySet
                    {
                        User = new Identity
                        {
                            Id = users.Members[0].AadObjectId,
                            AdditionalData = new Dictionary<string, object> { { "tenantId", azureAdOptions.TenantId } }
                        },
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
                    await chatService.InstallApp(meetingCall.ChatInfo.ThreadId, "60413772-76b9-48c3-b7e5-2ecc82f60f41");

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
                    callBotCache.Set($"{meetingCall.Id}:incident", incidentDetails);

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

                return await CreateTaskModuleResponse("Working on that, you can close this dialog now.");
            }

            return await CreateTaskModuleResponse("Something went wrong 😖");
        }

        private async Task SendActivityToConversation(ITurnContext turnContext, string conversationId, IActivity activity, CancellationToken cancellationToken)
        {
            var newReference = new ConversationReference
            {
                Bot = new ChannelAccount
                {
                    Id = azureAdOptions.ClientId
                },
                Conversation = new ConversationAccount
                {
                    Id = conversationId
                },
                ServiceUrl = turnContext.Activity.ServiceUrl,
                ChannelId = turnContext.Activity.ChannelId,
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

        private async Task<TaskModuleResponse> CreateTaskModuleResponse(string value)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = value
                },
            };
        }

        private async Task<bool> SendActivityResponse(string responseText, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var updatedActivity = MessageFactory.Text(responseText);
            updatedActivity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(updatedActivity, cancellationToken);
            return true;
        }
    }
}
