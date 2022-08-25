// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChannelNotification.Bots
{
    using ChannelNotification.Helper;
    using ChannelNotification.Provider;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    //using TeamInfo = Microsoft.Graph.TeamInfo;

    public class ChannelNotificationBot : TeamsActivityHandler
    {
        /// <summary>
        /// Logs the information of bot hanadler.
        /// </summary>
        protected readonly ILogger logger;

        /// <summary>
        /// Store the coversation reference of meeting.
        /// </summary>
        private readonly ConcurrentDictionary<string, ConversationReference> conversationReferences;

        /// <summary>
        /// Manages the subscriptions created.
        /// </summary>
        private readonly SubscriptionManager subscriptionManager;
        
        public ChannelNotificationBot(ILogger<ChannelNotificationBot> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, SubscriptionManager subscriptionManager)
        {
            this.logger = logger;
            this.conversationReferences = conversationReferences;
            this.subscriptionManager = subscriptionManager;
        }

        /// <summary>
        /// Adds the coversation reference in dictionary.
        /// </summary>
        /// <param name="activity">Bot activity.</param>
        /// <param name="meetingUrl">Url of meeting passed as key.</param>
        private void AddConversationReference(Activity activity, string meetingUrl)
        {
            var conversationReference = activity.GetConversationReference();
            this.conversationReferences.AddOrUpdate(meetingUrl, conversationReference, (key, newValue) => conversationReference);
        }
        protected override async Task OnInstallationUpdateAddAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var teamsinfo = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);
                var teamID = teamsinfo.AadGroupId.ToString();
                AddConversationReference(turnContext.Activity as Activity, teamID);
                await this.subscriptionManager.InitializeAllSubscription(teamID,"");
                await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetSubscriptionInitializedCard(true)), cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex.Message);

                if (ex.Message == "The teamId can only be null if turnContext is within the scope of a Teams .")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please install this bot in Channel  to get notifications"), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Something went wrong! Please try installing bot again"), cancellationToken);
                }

            }
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var teamsinfo = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);
                var teamID = teamsinfo.AadGroupId.ToString();
                AddConversationReference(turnContext.Activity as Activity, teamID);
                await this.subscriptionManager.InitializeAllSubscription(teamID, "");
            }
            catch (Exception ex)
            {
                if (ex.Message == "The teamId can only be null if turnContext is within the scope of a Teams .")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please install this bot in channel chat to get  notifications"), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Something went wrong! Please try installing bot again."), cancellationToken);
                }
            }
        }
    }
}