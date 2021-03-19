// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using SidePanel.Controllers;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class SidePanelBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string replyText;
            var meetingId = turnContext.Activity.TeamsGetMeetingInfo().Id;
            var userId = turnContext.Activity.From.Id;
            var tenantID = turnContext.Activity.Conversation.TenantId;
            string userName = turnContext.Activity.From.Name;

            TeamsMeetingParticipant participant = await GetMeetingParticipantAsync(turnContext, meetingId, userId, tenantID);
            TeamsChannelAccount member = participant.User;
            MeetingParticipantInfo meetingInfo = participant.Meeting;
            ConversationAccount conversation = participant.Conversation;
            var meetingRole = meetingInfo.Role;

            if(meetingRole == "Organizer")
            {
                HomeController.meetingOrganizerId = turnContext.Activity.From.AadObjectId;
                HomeController.userName = userName;
                HomeController.conversationId = turnContext.Activity.Conversation.Id;
                replyText = "Hi **" + userName + "**. You are an **"+ meetingRole + "** for the current meeting. You can add Agenda and Publish it to the meeting using the Meeting Side Panel.";
            }
            else
            {
                replyText = "Hi **" + userName + "**. You are **" + meetingRole + "** for the current meeting. This is a SidePanel Application used to add the Agenda points in Meeting by the Organizer.";
            }
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        public static async Task<TeamsMeetingParticipant> GetMeetingParticipantAsync(ITurnContext turnContext, string meetingId = null, string participantId = null, string tenantId = null, CancellationToken cancellationToken = default)
        {
            meetingId ??= turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
            participantId ??= turnContext.Activity.From.AadObjectId ?? throw new InvalidOperationException($"{nameof(participantId)} is required.");
            tenantId ??= turnContext.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.FetchParticipantAsync(meetingId, participantId, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        private static ITeamsConnectorClient GetTeamsConnectorClient(ITurnContext turnContext)
        {
            var connectorClient = GetConnectorClient(turnContext);
            if (connectorClient is ConnectorClient connectorClientImpl)
            {
                return new TeamsConnectorClient(connectorClientImpl.BaseUri, connectorClientImpl.Credentials, connectorClientImpl.HttpClient, connectorClientImpl.HttpClient == null);
            }
            else
            {
                return new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials);
            }
        }

        private static IConnectorClient GetConnectorClient(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IConnectorClient>() ?? throw new InvalidOperationException("This method requires a connector client.");
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome " + turnContext.Activity.From.Name + " to the Meeting Extensibility SidePanel app.";
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