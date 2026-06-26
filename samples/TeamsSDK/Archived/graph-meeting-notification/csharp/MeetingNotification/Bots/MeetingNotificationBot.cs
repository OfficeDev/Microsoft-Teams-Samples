// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Bots
{
    using MeetingNotification.Helper;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class MeetingNotificationBot : TeamsActivityHandler
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
        private readonly SubscriptionHelper subscriptionHelper;

        public MeetingNotificationBot(ILogger<MeetingNotificationBot> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, SubscriptionHelper subscriptionHelper)
        {
            this.logger = logger;
            this.conversationReferences = conversationReferences;
            this.subscriptionHelper = subscriptionHelper;
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
                var meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext, null, cancellationToken);
                var decodedMeetingJoinUrl = HttpUtility.UrlDecode(meetingInfo.Details.JoinUrl.ToString());
                AddConversationReference(turnContext.Activity as Activity, decodedMeetingJoinUrl);
                await this.subscriptionHelper.InitializeAllSubscription(meetingInfo.Details.JoinUrl.ToString());

                await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetSubscriptionInitializedCard(true)), cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken).ConfigureAwait(false);
                    await this.subscriptionHelper.CheckSubscriptions().ConfigureAwait(false); ;
                }
            }
            catch (InvalidOperationException ex)
            {
                this.logger.LogError(ex.Message);
                await turnContext.SendActivityAsync(MessageFactory.Text("Please install this bot in meeting chat to get meeting notifications"), cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex.Message);
                await turnContext.SendActivityAsync(MessageFactory.Text("Something went wrong! Please try installing bot again"), cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext, null, cancellationToken);
                var decodedMeetingJoinUrl = HttpUtility.UrlDecode(meetingInfo.Details.JoinUrl.ToString());

                AddConversationReference(turnContext.Activity as Activity, decodedMeetingJoinUrl);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(NotificationCardHelper.GetSubscriptionInitializedCard(false)), cancellationToken);
                await this.subscriptionHelper.InitializeAllSubscription(meetingInfo.Details.JoinUrl.ToString());
            }
            catch (Exception ex)
            {
                if (ex.Message == "The meetingId can only be null if turnContext is within the scope of a MS Teams Meeting.")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please install this bot in meeting chat to get meeting notifications"), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Something went wrong! Please try installing bot again."), cancellationToken);
                }
            }
        }
    }
}