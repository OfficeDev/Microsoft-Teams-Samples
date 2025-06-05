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
        private readonly List<string> _memberDetails = new List<string>();

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
                                _memberDetails.Add(member.Id);
                            }
                        }

                        data.action.data.UserId = _memberDetails;
                        var responseAttachment = GetResponseAttachment(firstCard, data, out cardJson);
                        var pendingActivity = new Activity
                        {
                            Type = "message",
                            Id = turnContext.Activity.ReplyToId,
                            Attachments = new List<Attachment> { responseAttachment }
                        };

                        await turnContext.UpdateActivityAsync(pendingActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "refresh":
                        string[] cardPath = turnContext.Activity.From.Id == data.action.data.UserMRI
                            ? new[] { ".", "Cards", "AssignedToCard.json" }
                            : new[] { ".", "Cards", "OtherMembersCard.json" };

                        adaptiveCardResponse = GetNextActionCard(cardPath, data);
                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "cancelCard":
                        string[] cancelCard = { ".", "Cards", "CancelCard.json" };
                        var cancelCardResponse = GetResponseAttachment(cancelCard, data, out cardJson);
                        var canceledActivity = new Activity
                        {
                            Type = "message",
                            Id = turnContext.Activity.ReplyToId,
                            Attachments = new List<Attachment> { cancelCardResponse }
                        };

                        await turnContext.UpdateActivityAsync(canceledActivity);
                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "approved":
                        string[] approvedCard = { ".", "Cards", "ApprovedCard.json" };
                        var approvedAttachment = GetResponseAttachment(approvedCard, data, out cardJson);
                        var approvedActivity = new Activity
                        {
                            Type = "message",
                            Id = turnContext.Activity.ReplyToId,
                            Attachments = new List<Attachment> { approvedAttachment }
                        };

                        await turnContext.UpdateActivityAsync(approvedActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "rejected":
                        string[] rejectedCard = { ".", "Cards", "RejectedCard.json" };
                        var rejectedAttachment = GetResponseAttachment(rejectedCard, data, out cardJson);
                        var rejectedActivity = new Activity
                        {
                            Type = "message",
                            Id = turnContext.Activity.ReplyToId,
                            Attachments = new List<Attachment> { rejectedAttachment }
                        };

                        await turnContext.UpdateActivityAsync(rejectedActivity);

                        response = JObject.Parse(cardJson);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse
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

        /// <summary>
        /// Get initial card.
        /// </summary>
        /// <param name="filepath">The file path.</param>
        /// <param name="name">The name.</param>
        /// <param name="userMRI">The user MRI.</param>
        /// <returns>The attachment.</returns>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            var cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString)
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get next card.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="data">The data.</param>
        /// <returns>The adaptive card invoke response.</returns>
        private AdaptiveCardInvokeResponse GetNextActionCard(string[] path, InitialSequentialCard data)
        {
            var cardJson = File.ReadAllText(Path.Combine(path));
            var template = new AdaptiveCardTemplate(cardJson);

            var payloadData = new
            {
                data.action.data.RequestTitle,
                data.action.data.RequestDescription,
                data.action.data.AssignedTo,
                data.action.data.CreatedBy,
                data.action.data.CreatedById,
                data.action.data.AssignedToName,
                data.action.data.UserMRI
            };

            var cardJsonString = template.Expand(payloadData);
            var card = JObject.Parse(cardJsonString);

            var adaptiveCardResponse = new AdaptiveCardInvokeResponse
            {
                StatusCode = 200,
                Type = "application/vnd.microsoft.card.adaptive",
                Value = card
            };

            return adaptiveCardResponse;
        }

        /// <summary>
        /// Get response attachment.
        /// </summary>
        /// <param name="filepath">The file path.</param>
        /// <param name="data">The data.</param>
        /// <param name="cardJsonString">The card JSON string.</param>
        /// <returns>The attachment.</returns>
        private Attachment GetResponseAttachment(string[] filepath, InitialSequentialCard data, out string cardJsonString)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            var template = new AdaptiveCardTemplate(adaptiveCardJson);

            var payloadData = new
            {
                data.action.data.RequestTitle,
                data.action.data.RequestDescription,
                data.action.data.AssignedTo,
                data.action.data.CreatedBy,
                data.action.data.AssignedToName,
                data.action.data.UserMRI,
                data.action.data.UserId,
                data.action.data.CreatedById
            };

            cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString)
            };

            return adaptiveCardAttachment;
        }
    }
}
