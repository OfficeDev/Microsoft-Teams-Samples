// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using BotRequestApproval.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotRequestApproval.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private List<string> memberDetails = new List<string> { };

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string[] path = { ".", "Cards", "InitialCard.json" };
            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);

            await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
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
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can send task request to your manager and your manager can approve/reject the request."), cancellationToken);
                }
            }
        }

        /// <summary>
        ///  Invoked when an invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                var data = JsonConvert.DeserializeObject<InitialSequentialCard>(turnContext.Activity.Value.ToString());
                string verb = data.action.verb;
                AdaptiveCardInvokeResponse adaptiveCardResponse;
                string cardJson;
                JObject response;

                switch (verb)
                {
                    case "initialRefresh":
                        string[] initialCard = { ".", "Cards", "RequestCard.json" };
                        adaptiveCardResponse = GetNextActionCard(initialCard, data);

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "requestCard":
                        
                        string[] firstCard = { ".", "Cards", "RequestDetailsCardForUser.json" };
                        var assigneeInfo = await TeamsInfo.GetMemberAsync(turnContext, data.action.data.AssignedTo, cancellationToken);
                        data.action.data.UserMRI = assigneeInfo.Id;
                        data.action.data.CreatedById = turnContext.Activity.From.Id;
                        data.action.data.AssignedToName = assigneeInfo.Name;
                        var members = new List<TeamsChannelAccount>();
                        string continuationToken = null;

                        do
                        {
                            var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                            continuationToken = currentPage.ContinuationToken;
                            members.AddRange(currentPage.Members);
                        }
                        while (continuationToken != null);

                        foreach (var member in members)
                        {
                            if (member.AadObjectId != turnContext.Activity.From.AadObjectId)
                            {
                                var newMemberInfo = member.Id;
                                memberDetails.Add(newMemberInfo);
                            }
                        }

                        data.action.data.UserId = memberDetails;
                        var responseAttachment = GetResponseAttachment(firstCard, data, out cardJson);
                        Activity pendingActivity = new Activity();
                        pendingActivity.Type = "message";
                        pendingActivity.Id = turnContext.Activity.ReplyToId;
                        pendingActivity.Attachments = new List<Attachment> { responseAttachment };

                        await turnContext.UpdateActivityAsync(pendingActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "refresh":

                        if (turnContext.Activity.From.Id == data.action.data.UserMRI)
                        {
                            string[] assignedToCard = { ".", "Cards", "AssignedToCard.json" };
                            adaptiveCardResponse = GetNextActionCard(assignedToCard, data);

                            return CreateInvokeResponse(adaptiveCardResponse);
                        }
                        else
                        {
                            string[] othersCard = { ".", "Cards", "OtherMembersCard.json" };
                            adaptiveCardResponse = GetNextActionCard(othersCard, data);

                            return CreateInvokeResponse(adaptiveCardResponse);
                        }

                    case "cancelCard":
                        string[] cancelCard = { ".", "Cards", "CancelCard.json" };
                        var cancelCardResponse = GetResponseAttachment(cancelCard, data, out cardJson);
                        Activity canceledActivity = new Activity();
                        canceledActivity.Type = "message";
                        canceledActivity.Id = turnContext.Activity.ReplyToId;
                        canceledActivity.Attachments = new List<Attachment> { cancelCardResponse };
                        await turnContext.UpdateActivityAsync(canceledActivity);
                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "approved":
                        string[] approvedCard = { ".", "Cards", "ApprovedCard.json" };
                        var approvedAttachment = GetResponseAttachment(approvedCard, data, out cardJson);
                        Activity approvedActivity = new Activity();
                        approvedActivity.Type = "message";
                        approvedActivity.Id = turnContext.Activity.ReplyToId;
                        approvedActivity.Attachments = new List<Attachment> { approvedAttachment };

                        await turnContext.UpdateActivityAsync(approvedActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "rejected":
                        string[] rejectedCard = { ".", "Cards", "RejectedCard.json" };
                        var rejectedAttachment = GetResponseAttachment(rejectedCard, data, out cardJson);
                        Activity rejectedActivity = new Activity();
                        rejectedActivity.Type = "message";
                        rejectedActivity.Id = turnContext.Activity.ReplyToId;
                        rejectedActivity.Attachments = new List<Attachment> { rejectedAttachment };

                        await turnContext.UpdateActivityAsync(rejectedActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);
                }
            }

            return null;
        }

        // Get intial card.
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };

            return adaptiveCardAttachment;
        }

        // Get next card.
        private AdaptiveCardInvokeResponse GetNextActionCard(string[] path, InitialSequentialCard data)
        {
            var cardJson = File.ReadAllText(Path.Combine(path));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJson);

            var payloadData = new
            {
                requestTitle = data.action.data.RequestTitle,
                requestDescription = data.action.data.RequestDescription,
                assignedTo = data.action.data.AssignedTo,
                createdBy = data.action.data.CreatedBy,
                createdById = data.action.data.CreatedById,
                assignedToName = data.action.data.AssignedToName,
                userMRI = data.action.data.UserMRI
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            var cardJsonstring = template.Expand(payloadData);
            var card = JObject.Parse(cardJsonstring);

            var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = "application/vnd.microsoft.card.adaptive",
                Value = card
            };

            return adaptiveCardResponse;
        }

        // Get response attachment
        private Attachment GetResponseAttachment(string[] filepath, InitialSequentialCard data, out string cardJsonString)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);

            var payloadData = new
            {
                requestTitle = data.action.data.RequestTitle,
                requestDescription = data.action.data.RequestDescription,
                assignedTo = data.action.data.AssignedTo,
                createdBy = data.action.data.CreatedBy,
                assignedToName = data.action.data.AssignedToName,
                userMRI = data.action.data.UserMRI,
                userId = data.action.data.UserId,
                createdById = data.action.data.CreatedById,
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString),
            };

            return adaptiveCardAttachment;
        }
    }
}