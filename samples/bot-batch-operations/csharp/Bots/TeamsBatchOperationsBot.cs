// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsBatchOperationsBot : TeamsActivityHandler
    {
        public TeamsBatchOperationsBot(IConfiguration config)
        {
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            if (turnContext.Activity.Text == null)
            {
                await SendDataToBatchOperations(turnContext, cancellationToken);
            }
            else
            {
                var text = turnContext.Activity.Text.Trim().ToLower();
                if (text.Contains("listusers"))
                    await MessageListOfUsersInputAsync(turnContext, cancellationToken);               
                else if (text.Contains("tenant"))
                    await MessageAllUsersInTenantAsync(turnContext, cancellationToken);
                else if (text.Contains("team"))
                    await MessageAllUsersInTeamInputAsync(turnContext, cancellationToken);
                else if (text.Contains("listchannels"))
                    await MessageListOfChannelsInputAsync(turnContext, cancellationToken);
                else if (text.Contains("state"))
                    await GetOperationStateInputAsync(turnContext, cancellationToken);
                else if (text.Contains("failed"))
                    await GetFailedEntriesInputAsync(turnContext, cancellationToken);
                else if (text.Contains("cancel"))
                    await CancelOperationInputAsync(turnContext, cancellationToken);
                else
                    await CardActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
                }
            }
        }

        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Microsoft Teams batch operations demo bot. This bot is configured in {turnContext.Activity.Conversation.Name}");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Microsoft Teams batch operations demo bot.");
            }
        }

        private async Task CardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            var card = new HeroCard
            {
                Buttons = new List<CardAction>
                        {                          
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message list of users",
                                Text = "listUsers"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message all users in tenant",
                                Text = "tenant"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message all users in team",
                                Text = "team"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message list of channels",
                                Text = "listChannels"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Get Operation state",
                                Text = "state"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Get failed entries",
                                Text = "failed"
                            }
                            ,new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Cancel operation",
                                Text = "cancel"
                            }
                        }
            };


            await SendWelcomeCard(turnContext, card, cancellationToken);

        }

        private async Task MessageListOfUsersInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "MultipleInputCard.json", "user-id" )), cancellationToken);
        }

        private async Task MessageListOfUsersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var membersList = new List<object>();
            var tenantId = turnContext.Activity.Conversation.TenantId;

            for (int i = 1; i <= 5; i++)
            {
                var userId = GetId(turnContext.Activity.Value, $"user-id{i}");
                membersList.Add(new { Id = userId });
            }

            var message = MessageFactory.Text("Hello user! You are part of the batch.");
            var operationId = await TeamsInfo.SendMessageToListOfUsersAsync(turnContext, message, membersList, tenantId, cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"All messages have been sent. OperationId: {operationId}"), cancellationToken);
        }

        private async Task MessageListOfChannelsInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "MultipleInputCard.json", "channel-id")), cancellationToken);
        }

        private async Task MessageListOfChannelsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var membersList = new List<object>();
            var tenantId = turnContext.Activity.Conversation.TenantId;

            for (int i = 1; i <= 5; i++)
            {
                var channelId = GetId(turnContext.Activity.Value, $"channel-id{i}");
                membersList.Add(new { Id = channelId });
            }

            var message = MessageFactory.Text($"Hello channel user! You are part of the batch.");
            var operationId = await TeamsInfo.SendMessageToListOfChannelsAsync(turnContext, message, membersList, tenantId, cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"All messages have been sent. OperationId: {operationId}"), cancellationToken);
        }

        private async Task MessageAllUsersInTenantAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            var tenantId = turnContext.Activity.Conversation.TenantId;

            var message = MessageFactory.Text("Hello user! You received this tenant message from batch.");

            var operationId = await TeamsInfo.SendMessageToAllUsersInTenantAsync(turnContext, message, tenantId, cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"All messages have been sent. OperationId: {operationId}"), cancellationToken);
        }

        private async Task MessageAllUsersInTeamInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "InputCard.json", "team-id")), cancellationToken);
        }
        private async Task MessageAllUsersInTeamAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamId = GetId(turnContext.Activity.Value, "team-id");
            var tenantId = turnContext.Activity.Conversation.TenantId;

            var message = MessageFactory.Text("Hello user! You received this team message from batch.");
            var operationId = await TeamsInfo.SendMessageToAllUsersInTeamAsync(turnContext, message, teamId, tenantId, cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"All messages have been sent. OperationId: {operationId}"), cancellationToken);
        }

        private async Task GetOperationStateInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "InputCard.json", "state-operationId")), cancellationToken);
        }

        private async Task GetOperationStateAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var operationId = GetId(turnContext.Activity.Value, "state-operationId");
            var operationState = await TeamsInfo.GetOperationStateAsync(turnContext, operationId, cancellationToken);
            var statusResponses = "";

            foreach (var statusResponse in operationState.StatusMap)
            {
                statusResponses += statusResponse.Key + ": " + statusResponse.Value + ", ";
            }

            var response = $"The operation was {operationState.State} with the status responses: {statusResponses} and total entries count: {operationState.TotalEntriesCount}";

            await turnContext.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
        }

        private async Task GetFailedEntriesInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "InputCard.json", "entries-operationId")), cancellationToken);
        }

        private async Task GetFailedEntriesAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var operationId = GetId(turnContext.Activity.Value, "entries-operationId");
            string continuationToken = null;
            List<BatchFailedEntry> failedEntries = new List<BatchFailedEntry>();
            var message = $"This is the list of failed entries for the operation {operationId}";

            do
            {
                var currentPage = await TeamsInfo.GetPagedFailedEntriesAsync(turnContext, operationId, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                failedEntries = failedEntries.Concat(currentPage.FailedEntries).ToList();
            }
            while (continuationToken != null);

            foreach (var entry in failedEntries)
            {
                message += $"\n id: {entry.Id}, error: {entry.Error} \n\n";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private async Task CancelOperationInputAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetInputCard(turnContext, "InputCard.json", "cancel-operationId")), cancellationToken);
        }

        private async Task CancelOperationAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var operationId = GetId(turnContext.Activity.Value, "cancel-operationId");
            var message = $"The operation with Id: {operationId} ";

            try
            {
                await TeamsInfo.CancelOperationAsync(turnContext, operationId, cancellationToken);
                message += "has been canceled";
            }
            catch (Exception ex)
            {
                message += $"couldn't be canceled. ex: {ex.Message}";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }

        private Attachment GetInputCard(ITurnContext<IMessageActivity> turnContext, string resourceName, string operationName)
        {
            string[] filepath = { ".", "Resources", resourceName };

            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            if (operationName == "team-id")
                adaptiveCardJson = adaptiveCardJson.Replace("the operation", "the team");
            adaptiveCardJson = adaptiveCardJson.Replace("\"inputid", $"\"{operationName}");
            
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdBy = turnContext.Activity.From.Name
            };

            //"Expand" the template - this generates the final Adaptive Card payload
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };

            return adaptiveCardAttachment;
        }

        private async Task SendDataToBatchOperations(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var operation = turnContext.Activity.Value.ToString();

            if (operation.Contains("user"))
                await MessageListOfUsersAsync(turnContext, cancellationToken);
            else if (operation.Contains("channel"))
                await MessageListOfChannelsAsync(turnContext, cancellationToken);
            else if (operation.Contains("team"))
                await MessageAllUsersInTeamAsync(turnContext, cancellationToken);
            else if (operation.Contains("state"))
                await GetOperationStateAsync(turnContext, cancellationToken);
            else if (operation.Contains("entries"))
                await GetFailedEntriesAsync(turnContext, cancellationToken);
            else if (operation.Contains("cancel"))
                await CancelOperationAsync(turnContext, cancellationToken);
        }
       
        private string GetId(object source, string propName)
        {
            var jobject = source as JObject;
            var jtoken = jobject?[propName];

            return jtoken?.Value<string>().Trim();
        }

        private static async Task SendWelcomeCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            card.Title = "Welcome!";

            var activity = MessageFactory.Attachment(card.ToAttachment());

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
    }
}
