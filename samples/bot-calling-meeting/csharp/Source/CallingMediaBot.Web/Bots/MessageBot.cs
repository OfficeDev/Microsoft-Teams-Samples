// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Bots;
using System.Collections;
using CallingMediaBot.Web.AdaptiveCards;
using CallingMediaBot.Web.Interfaces;
using CallingMediaBot.Web.Options;
using CallingMediaBot.Web.Services.MicrosoftGraph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

public class MessageBot : ActivityHandler
{
    private readonly IAdaptiveCardFactory adaptiveCardFactory;
    private readonly IGraph graph;
    private readonly ICallService callService;
    private readonly IEnumerable<UserOptions> users;

    public MessageBot(IAdaptiveCardFactory adaptiveCardFactory, IGraph graph, ICallService callService, IOptions<List<UserOptions>> users)
    {
        this.adaptiveCardFactory = adaptiveCardFactory;
        this.graph = graph;
        this.callService = callService;
        this.users = users.Value;
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
                var call = await callService.Create(new Identity
                {
                    DisplayName = users.FirstOrDefault()?.DisplayName,
                    Id = users.FirstOrDefault()?.Id
                });

                if (call != null)
                {
                    await turnContext.SendActivityAsync("Placed a call Successfully.", cancellationToken: cancellationToken);
                }
                break;
            case "transfercall":
                throw new NotImplementedException();
                //var sourceCallResponse = await callService.Create(new Identity
                //{
                //    DisplayName = users.FirstOrDefault()?.DisplayName,
                //    Id = users.FirstOrDefault()?.Id
                //});

                //if (sourceCallResponse != null)
                //{
                //    await turnContext.SendActivityAsync("Transferring the call!");
                //    await callService.Transfer(sourceCallResponse.Id, new Identity
                //    {
                //        DisplayName = users.ElementAt(1).DisplayName,
                //        Id = users.ElementAt(1).Id
                //    });
                //}
                break;
            case "joinscheduledmeeting":
                throw new NotImplementedException();
                //var onlineMeeting = await graph.CreateOnlineMeetingAsync();
                //if (onlineMeeting != null)
                //{
                //    var statefullCall = await graph.JoinScheduledMeeting(onlineMeeting.JoinWebUrl);
                //    if (statefullCall != null)
                //    {
                //        await turnContext.SendActivityAsync($"[Click here to Join the meeting]({onlineMeeting.JoinWebUrl})");
                //    }
                //}
                break;
            case "inviteparticipant":
                throw new NotImplementedException();
                //var meeting = await graph.CreateOnlineMeetingAsync();
                //if (meeting != null)
                //{
                //    var statefullCall = await graph.JoinScheduledMeeting(meeting.JoinWebUrl);
                //    if (statefullCall != null)
                //    {

                //        await graph.InviteParticipant(statefullCall.Id);
                //        await turnContext.SendActivityAsync("Invited participant successfuly");
                //    }
                //}
                break;
            default:
                await turnContext.SendActivityAsync("Welcome to bot", cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateWelcomeCard()), cancellationToken);
                break;
        }
    }
}
