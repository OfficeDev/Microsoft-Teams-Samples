// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CallingBotSample.AdaptiveCards;
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
using Newtonsoft.Json.Linq;

namespace CallingBotSample.Bots
{
    public class MessageBot : TeamsActivityHandler
    {
        private readonly ConversationState conversationState;
        private readonly IAdaptiveCardFactory adaptiveCardFactory;
        private readonly AudioRecordingConstants audioRecordingConstants;
        private readonly ICallService callService;
        private readonly AzureAdOptions azureAdOptions;
        private readonly ILogger<MessageBot> logger;

        public MessageBot(
            ConversationState conversationState,
            IAdaptiveCardFactory adaptiveCardFactory,
            AudioRecordingConstants audioRecordingConstants,
            ICallService callService,
            IOptions<AzureAdOptions> azureAdOptions,
            ILogger<MessageBot> logger)
        {
            this.conversationState = conversationState;
            this.adaptiveCardFactory = adaptiveCardFactory;
            this.audioRecordingConstants = audioRecordingConstants;
            this.callService = callService;
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
                    await SendResponse(turnContext, type, cancellationToken);
                }
            }
            else
            {
                turnContext.Activity.RemoveRecipientMention();
                await SendResponse(turnContext, turnContext.Activity.Text.Trim().ToLower(), cancellationToken);
            }
        }

        protected override async Task OnTeamsMeetingStartAsync(MeetingStartEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendMeetingActionsCard(turnContext, cancellationToken);
        }

        protected override async Task OnTeamsMeetingEndAsync(MeetingEndEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

            var activity = MessageFactory.Text("This meeting has completed, you can no longer perform actions.");
            activity.Id = conversationData.MeetingActionsCardActivityId;
            if (activity.Id != null)
            {
                await turnContext.UpdateActivityAsync(activity, cancellationToken);
            }
        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var action = asJobject.ToObject<TaskModuleFetchData>()?.Action;

            var taskInfo = new TaskModuleTaskInfo();

            switch (action)
            {
                case "createcall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to create a call with:", "Create", isMultiSelect: true);
                    taskInfo.Title = "Create call";
                    break;
                case "transfercall":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to transfer the call to:", "Transfer");
                    taskInfo.Title = "Transfer call";
                    break;
                case "inviteparticipant":
                    taskInfo.Card = adaptiveCardFactory.CreatePeoplePickerCard("Choose who to invite to the call:", "Invite");
                    taskInfo.Title = "Select the user to invite";
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

            var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

            if (peoplePicker != null)
            {
                try
                {
                    var action = moduleSubmitData?.Action?.ToLowerInvariant();

                    switch (action)
                    {
                        case "create":
                            var peoplePickerAadIds = peoplePicker.Split(',');
                            var call = await callService.Create(users: peoplePickerAadIds.Select(p => new Identity { Id = p }).ToArray());

                            if (call != null)
                            {
                                // Save the meeting ID so it can be used for transferring/inviting participants to the call later.
                                conversationData.MeetingId = call.Id;
                                await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                                await turnContext.SendActivityAsync("Placed a call Successfully.", cancellationToken: cancellationToken);
                                return CreateTaskModuleResponse("Working on that, you can close this dialog now.");
                            }
                            break;
                        case "transfer":
                        case "invite":
                            if (conversationData.MeetingId != null)
                            {
                                if (action == "transfer")
                                {
                                    await callService.Transfer(
                                        conversationData.MeetingId,
                                        new Identity { Id = peoplePicker });
                                }
                                else
                                {
                                    await callService.InviteParticipant(
                                        conversationData.MeetingId,
                                        new IdentitySet { User = new Identity { Id = peoplePicker } });
                                }

                                return CreateTaskModuleResponse("Working on that, you can close this dialog now.");
                            }
                            else
                            {
                                return CreateTaskModuleResponse("Something went wrong 😖. We were unable to get the meeting id of this meeting.");
                            }
                        default:
                            break;
                    }
                }
                catch (ServiceException ex)
                {
                    logger.LogError(ex, "Failure while making Graph Call");
                    return CreateTaskModuleResponse($"Something went wrong 😖. {ex.Message}");
                }
            }

            return CreateTaskModuleResponse("Something went wrong 😖");
        }

        private async Task SendResponse(ITurnContext<IMessageActivity> turnContext, string input, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

            switch (input)
            {
                case "showmeetingactions":
                    if (conversationData.MeetingId == null)
                    {
                        // Without the Meeting ID we are unable to transfer or invite people to the call
                        await turnContext.SendActivityAsync("Meeting ID not found, please use the 'Create call' button to create the call.");
                        return;
                    }

                    await SendMeetingActionsCard(turnContext, cancellationToken);
                    break;
                case "playrecordprompt":
                    if (conversationData.MeetingId == null)
                    {
                        // Without the Meeting ID we are unable to play the prompt
                        await turnContext.SendActivityAsync("Meeting ID not found, please use the 'Create call' button to create the call.");
                        return;
                    }

                    await callService.Record(conversationData.MeetingId, audioRecordingConstants.PleaseRecordYourMessage);
<<<<<<< HEAD
                    break;
                case "hangup":
                    if (conversationData.MeetingId == null)
                    {
                        // Without the Meeting ID we are unable to play the prompt
                        await turnContext.SendActivityAsync("Meeting ID not found, unable to end meeting.");
                        return;
                    }

                    await callService.HangUp(conversationData.MeetingId);
=======
>>>>>>> 2a170132 (Calling Bot: Prompt for Recording and echo response)
                    break;
                case "joinscheduledmeeting":
                    if (turnContext.Activity.ChannelData["meeting"] != null)
                    {
                        var users = await TeamsInfo.GetPagedMembersAsync(turnContext, cancellationToken: cancellationToken);

                        var call = await callService.Create(
                            turnContext.Activity.Conversation.Id,
                            new Identity
                            {
                                Id = users.Members[0].AadObjectId,
                                AdditionalData = new Dictionary<string, object> { { "tenantId", azureAdOptions.TenantId } }
                            });

                        if (call != null)
                        {
                            // Save the meeting ID so it can be used for transferring/inviting participants to the call later.
                            conversationData.MeetingId = call.Id;
                            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                            await turnContext.SendActivityAsync("Joined meeting successfully.", cancellationToken: cancellationToken);
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

        private async Task SendMeetingActionsCard(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

            var resourceResponse = await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateMeetingActionsCard()));
            conversationData.MeetingActionsCardActivityId = resourceResponse.Id;
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private TaskModuleResponse CreateTaskModuleResponse(string value)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = value
                },
            };
        }
    }
}
