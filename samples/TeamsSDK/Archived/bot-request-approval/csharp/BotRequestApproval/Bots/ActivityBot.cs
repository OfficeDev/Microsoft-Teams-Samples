// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards.Templating;
using BotRequestApproval.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotRequestApproval.Bots;

/// <summary>
/// Bot Activity handler class.
/// </summary>
public class ActivityBot : TeamsActivityHandler
{
    /// <summary>
    /// Handle when a message is addressed to the bot.
    /// </summary>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        string[] path = [".", "Cards", "InitialCard.json"];
        var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
        var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);

        await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
    }

    /// <summary>
    /// Invoked when bot (like a user) are added to the conversation.
    /// </summary>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can send task requests to your manager and your manager can approve/reject the request."), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Invoked when an invoke activity is received from the connector.
    /// </summary>
    protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Name != "adaptiveCard/action")
        {
            return null!;
        }

        var data = JsonConvert.DeserializeObject<InitialSequentialCard>(turnContext.Activity.Value.ToString()!);
        string verb = data!.action!.verb!;

        switch (verb)
        {
            case "initialRefresh":
                string[] initialCard = [".", "Cards", "RequestCard.json"];
                return CreateInvokeResponse(GetNextActionCard(initialCard, data));

            case "requestCard":
                return await HandleRequestCardAsync(turnContext, data, cancellationToken);

            case "refresh":
                string[] cardPath = turnContext.Activity.From.Id == data.action.data!.UserMRI
                    ? [".", "Cards", "AssignedToCard.json"]
                    : [".", "Cards", "OtherMembersCard.json"];
                return CreateInvokeResponse(GetNextActionCard(cardPath, data));

            case "cancelCard":
                return await HandleUpdateCardAsync(turnContext, [".", "Cards", "CancelCard.json"], data);

            case "approved":
                return await HandleUpdateCardAsync(turnContext, [".", "Cards", "ApprovedCard.json"], data);

            case "rejected":
                return await HandleUpdateCardAsync(turnContext, [".", "Cards", "RejectedCard.json"], data);

            default:
                return null!;
        }
    }

    private static async Task<InvokeResponse> HandleRequestCardAsync(ITurnContext<IInvokeActivity> turnContext, InitialSequentialCard data, CancellationToken cancellationToken)
    {
        string[] firstCard = [".", "Cards", "RequestDetailsCardForUser.json"];
        var assigneeInfo = await TeamsInfo.GetMemberAsync(turnContext, data.action!.data!.AssignedTo, cancellationToken);
        data.action.data.UserMRI = assigneeInfo.Id;
        data.action.data.CreatedById = turnContext.Activity.From.Id;
        data.action.data.AssignedToName = assigneeInfo.Name;

        var memberIds = new List<string>();
        string? continuationToken = null;

        do
        {
            var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
            continuationToken = currentPage.ContinuationToken;

            foreach (var member in currentPage.Members)
            {
                if (member.AadObjectId != turnContext.Activity.From.AadObjectId)
                {
                    memberIds.Add(member.Id);
                }
            }
        }
        while (continuationToken != null);

        data.action.data.UserId = memberIds;

        return await HandleUpdateCardAsync(turnContext, firstCard, data);
    }

    private static async Task<InvokeResponse> HandleUpdateCardAsync(ITurnContext<IInvokeActivity> turnContext, string[] cardPath, InitialSequentialCard data)
    {
        var responseAttachment = GetResponseAttachment(cardPath, data, out var cardJson);
        var activity = new Activity
        {
            Type = "message",
            Id = turnContext.Activity.ReplyToId,
            Attachments = [responseAttachment]
        };

        await turnContext.UpdateActivityAsync(activity);

        return CreateInvokeResponse(new AdaptiveCardInvokeResponse
        {
            StatusCode = 200,
            Type = "application/vnd.microsoft.card.adaptive",
            Value = JObject.Parse(cardJson)
        });
    }

    private static Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string? name = null, string? userMRI = null)
    {
        var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
        var template = new AdaptiveCardTemplate(adaptiveCardJson);
        var cardJsonString = template.Expand(new { createdById = userMRI, createdBy = name });

        return new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJsonString)
        };
    }

    private static AdaptiveCardInvokeResponse GetNextActionCard(string[] path, InitialSequentialCard data)
    {
        var cardJson = File.ReadAllText(Path.Combine(path));
        var template = new AdaptiveCardTemplate(cardJson);

        var cardJsonString = template.Expand(new
        {
            data.action!.data!.RequestTitle,
            data.action.data.RequestDescription,
            data.action.data.AssignedTo,
            data.action.data.CreatedBy,
            data.action.data.CreatedById,
            data.action.data.AssignedToName,
            data.action.data.UserMRI
        });

        return new AdaptiveCardInvokeResponse
        {
            StatusCode = 200,
            Type = "application/vnd.microsoft.card.adaptive",
            Value = JObject.Parse(cardJsonString)
        };
    }

    private static Attachment GetResponseAttachment(string[] filepath, InitialSequentialCard data, out string cardJsonString)
    {
        var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
        var template = new AdaptiveCardTemplate(adaptiveCardJson);

        cardJsonString = template.Expand(new
        {
            data.action!.data!.RequestTitle,
            data.action.data.RequestDescription,
            data.action.data.AssignedTo,
            data.action.data.CreatedBy,
            data.action.data.AssignedToName,
            data.action.data.UserMRI,
            data.action.data.UserId,
            data.action.data.CreatedById
        });

        return new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJsonString)
        };
    }
}
