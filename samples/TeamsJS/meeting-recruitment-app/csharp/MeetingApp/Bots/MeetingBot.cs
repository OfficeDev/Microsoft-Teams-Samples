// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetingApp.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MeetingApp.Bots
{
    public class MeetingBot : ActivityHandler
    {
        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        // Dependency injected dictionary for storing Conversation Data that has roster and note details.
        private readonly ConcurrentDictionary<string, ConversationData> _conversationDataReference;

        private IConfiguration _configuration;

        public MeetingBot(
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            ConcurrentDictionary<string, ConversationData> conversationDataReference,
            IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _conversationDataReference = conversationDataReference;
            _configuration = configuration;
        }

        /// <summary>
        /// Method to add conversation reference.
        /// </summary>
        /// <param name="activity">Bot activity</param>
        private void AddConversationReference(Activity activity)
        {
            var meetingId = (activity.GetChannelData<TeamsChannelData>())?.Meeting?.Id;
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(meetingId, conversationReference, (key, newValue) => conversationReference);
        }

        /// <summary>
        /// Method to get the members who are part of the meeting.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        private async Task GetConversationMembers(ITurnContext turnContext)
        {
            var connectorClient = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"]);
            var members = await connectorClient.Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id);
            string output = JsonConvert.SerializeObject(members);
            var rosterInfo = JsonConvert.DeserializeObject<RosterInfo[]>(output);
            var conversationData = new ConversationData()
            {
                Roster = rosterInfo,
                Note = ""
            };
            var meetingId = (turnContext.Activity.GetChannelData<TeamsChannelData>())?.Meeting?.Id;

            // Adding member information to the dictionary.
            _conversationDataReference.AddOrUpdate(meetingId, conversationData, (key, newValue) => conversationData);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            GetConversationMembers(turnContext);
            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetConversationMembers(turnContext);
                    var welcomeText = "Hello and welcome!";
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
