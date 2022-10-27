// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CallingMediaBot.Domain.Factories;
using CallingMediaBot.Web.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace CallingMediaBot.Web.Bots;

public class MessageBot : ActivityHandler
{
    private readonly IAdaptiveCardFactory adaptiveCardFactory;
    private readonly IGraph graph;

    public MessageBot(IAdaptiveCardFactory adaptiveCardFactory, IGraph graph)
    {
        this.adaptiveCardFactory = adaptiveCardFactory;
        this.graph = graph;
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

    private async Task SendResponse(ITurnContext<IMessageActivity> turnContext, string input, CancellationToken cancellationToken)
    {
        switch (input)
        {
            case "createcall":
                var call = await graph.CreateCallAsync();
                if (call != null)
                {
                    await turnContext.SendActivityAsync("Placed a call Successfully.");
                }
                break;
            case "transfercall":
                var sourceCallResponse = await graph.CreateCallAsync();
                if (sourceCallResponse != null)
                {
                    await turnContext.SendActivityAsync("Transferring the call!");
                    await graph.TransferCallAsync(sourceCallResponse.Id);
                }
                break;
            case "joinscheduledmeeting":
                var onlineMeeting = await graph.CreateOnlineMeetingAsync();
                if (onlineMeeting != null)
                {
                    var statefullCall = await graph.JoinScheduledMeeting(onlineMeeting.JoinWebUrl);
                    if (statefullCall != null)
                    {
                        await turnContext.SendActivityAsync($"[Click here to Join the meeting]({onlineMeeting.JoinWebUrl})");
                    }
                }
                break;
            case "inviteparticipant":
                var meeting = await graph.CreateOnlineMeetingAsync();
                if (meeting != null)
                {
                    var statefullCall = await graph.JoinScheduledMeeting(meeting.JoinWebUrl);
                    if (statefullCall != null)
                    {

                        await graph.InviteParticipant(statefullCall.Id);
                        await turnContext.SendActivityAsync("Invited participant successfuly");
                    }
                }
                break;
            default:
                await turnContext.SendActivityAsync("Welcome to bot");
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateWelcomeCard()));
                break;
        }
    }
}
