// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SequentialUserSpecificFlow.Helpers;
using SequentialUserSpecificFlow.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SequentialUserSpecificFlow.Bots
{
    public class UserSpecificBot : ActivityHandler
    {
        private List<Info> memberDetails = new List<Info> { };

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string[] path = { ".", "Resources", "initialCard.json" };
            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
        }

        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };
            return adaptiveCardAttachment;
        }

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
                        string[] initialCard = { ".", "Resources", "firstCard.json" };
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
                                var newMemberInfo = new Info { value = member.AadObjectId, title = member.Name };
                                memberDetails.Add(newMemberInfo);
                            }
                        }

                        adaptiveCardResponse = GetNextActionCard(initialCard, data);
                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "firstCard":
                        string[] firstCard = { ".", "Resources", "secondCard.json" };
                        var assigneeInfo = await TeamsInfo.GetMemberAsync(turnContext, data.action.data.AssignedTo, cancellationToken);
                        data.action.data.UserMRI = assigneeInfo.Id;
                        adaptiveCardResponse = GetNextActionCard(firstCard, data);

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "secondCard":
                        string[] secondCard = { ".", "Resources", "thirdCard.json" };
                        if (data.action.data.Category == "Software")
                        {
                            adaptiveCardResponse = GetNextActionCard(secondCard, data, Constants.Software);
                        }
                        else
                        {
                            adaptiveCardResponse = GetNextActionCard(secondCard, data, Constants.Hardware);
                        }

                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "thirdCard":

                        var initiator = await TeamsInfo.GetMemberAsync(turnContext, data.action.data.AssignedTo, cancellationToken);
                        data.action.data.AssignedToName = initiator.Name;

                        string[] thirdCard = { ".", "Resources", "reviewCard.json" };
                        var responseAttachment = GetResponseAttachment(thirdCard, data, out cardJson);
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

                        string[] assignedToCard = { ".", "Resources", "assignedToCard.json" };
                        adaptiveCardResponse = GetNextActionCard(assignedToCard, data);
                        return CreateInvokeResponse(adaptiveCardResponse);

                    case "approved":

                        string[] approvedCard = { ".", "Resources", "approvedCard.json" };
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

                        string[] rejectedCard = { ".", "Resources", "rejectedCard.json" };
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

        public AdaptiveCardInvokeResponse GetNextActionCard(string[] path, InitialSequentialCard data, List<string> subCategory = null)
        {
            var cardJson = File.ReadAllText(Path.Combine(path));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJson);
            string subCat1 = "";
            string subCat2 = "";

            if (subCategory != null)
            {
                subCat1 = subCategory[0];
                subCat2 = subCategory[1];
            }

            var payloadData = new
            {
                assignees = memberDetails,
                incidentTitle = data.action.data.IncidentTitle,
                assignedTo = data.action.data.AssignedTo,
                category = data.action.data.Category,
                subCategory1 = subCat1,
                subCategory2 = subCat2,
                subCategory = data.action.data.SubCategory,
                createdBy = data.action.data.CreatedBy,
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

        private Attachment GetResponseAttachment(string[] filepath, InitialSequentialCard data, out string cardJsonString)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                incidentTitle = data.action.data.IncidentTitle,
                assignedTo = data.action.data.AssignedTo,
                category = data.action.data.Category,
                subCategory = data.action.data.SubCategory,
                createdBy = data.action.data.CreatedBy,
                assignedToName = data.action.data.AssignedToName,
                userMRI = data.action.data.UserMRI
            };
            cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString),
            };
            return adaptiveCardAttachment;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}