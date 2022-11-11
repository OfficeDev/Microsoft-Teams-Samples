// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Bots;

using System.Threading;
using CallingMediaBot.Web.AdaptiveCards;
using CallingMediaBot.Web.Interfaces;
using CallingMediaBot.Web.Models;
using CallingMediaBot.Web.Services.MicrosoftGraph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

public class MessageBot : TeamsActivityHandler
{
    private readonly ConversationState conversationState;
    private readonly IAdaptiveCardFactory adaptiveCardFactory;
    private readonly IGraph graph;
    private readonly ICallService callService;
    private readonly ILogger<MessageBot> logger;

    public MessageBot(ConversationState conversationState, IAdaptiveCardFactory adaptiveCardFactory, IGraph graph, ICallService callService, ILogger<MessageBot> logger)
    {
        this.conversationState = conversationState;
        this.adaptiveCardFactory = adaptiveCardFactory;
        this.graph = graph;
        this.callService = callService;
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
        var peoplePicker = asJobject.ToObject<TaskModuleSubmitData>()?.PeoplePicker;
        var action = asJobject.ToObject<TaskModuleSubmitData>()?.Action.ToLowerInvariant();

        var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
        var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

        if (peoplePicker != null)
        {
            try
            {
                switch (action)
                {
                    case "create":
                        var peoplePickerAadIds = peoplePicker.Split(',');
                        var call = await callService.Create(peoplePickerAadIds.Select(p => new Identity { Id = p }).ToArray());

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
        switch (input)
        {
            case "showmeetingactions":
                var conversationStateAccessors = conversationState.CreateProperty<MeetingActionDetails>(nameof(MeetingActionDetails));
                var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new MeetingActionDetails());

                if (conversationData.MeetingId == null)
                {
                    // Without the Meeting ID we are unable to transfer or invite people to the call
                    await turnContext.SendActivityAsync("Meeting ID not found, please use the 'Create call' button to create the call.");
                    return;
                }

                await SendMeetingActionsCard(turnContext, cancellationToken);
                break;
            //case "joinscheduledmeeting":
                //throw new NotImplementedException();
                //var onlineMeeting = await graph.CreateOnlineMeetingAsync();
                //if (onlineMeeting != null)
                //{
                //    var statefullCall = await graph.JoinScheduledMeeting(onlineMeeting.JoinWebUrl);
                //    if (statefullCall != null)
                //    {
                //        await turnContext.SendActivityAsync($"[Click here to Join the meeting]({onlineMeeting.JoinWebUrl})");
                //    }
                //}
                //break;
            default:
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateWelcomeCard()), cancellationToken);
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
