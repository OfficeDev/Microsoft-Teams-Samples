// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Helper
{
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using MeetingNotification.Model;
    using System.Collections.Generic;
    using System.IO;

    public static class NotificationCardHelper
    {
        /// <summary>
        /// Gets the subscription created card.
        /// </summary>
        /// <param name="isWelcomeCard">If card is send as welcome card.</param>
        /// <returns>Subscription adaptive card</returns>
        public static Microsoft.Bot.Schema.Attachment GetSubscriptionInitializedCard(bool isWelcomeCard)
        {
            var cardtemplate = GetCardTemplate("WelcomeCard.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                isWelcomeCard= isWelcomeCard
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        /// <summary>
        /// Gets the meeting start/end notification card.
        /// </summary>
        /// <param name="meetingResource">Meeting resource data.</param>
        /// <returns>Meeting start/end adaptive card.</returns>
        public static Microsoft.Bot.Schema.Attachment GetMeetingStartedEndedCard(MeetingResource meetingResource)
        {

            var cardtemplate = GetCardTemplate("MeetingNotificationStartEndCard.json");

            var cardData = meetingResource.EventType == MeetingNotificationType.CallStarted ? new
            {
                notification = "Meeting has been started."
            } : new
            {
                notification = "Meeting has been ended."
            };

            var adaptiveCardJson = cardtemplate.Expand(cardData);
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        /// <summary>
        /// Gets the meeting update notification card.
        /// </summary>
        /// <param name="meetingResource">Meeting resource data.</param>
        /// <returns>Meeting update adaptive card.</returns>
        public static Microsoft.Bot.Schema.Attachment GetMeetingUpdatedCard(MeetingResourceUpdate meetingResource)
        {
            var cardtemplate = GetCardTemplate("MeetingNotificationUpdateCard.json");

            var cardData =  new
            {
                notification = "Meeting has been ended.",
                activeParticipantJoinedCount = meetingResource.ActiveParticipantJoined.Count,
                activeParticipantLeftCount = meetingResource.ActiveParticipantLeft.Count,
                activeParticipantsJoined = new List<AdaptiveFact>(),
                activeParticipantsLeft = new List<AdaptiveFact>(),
            };

            if (meetingResource.ActiveParticipantJoined.Count > 0)
            {
                for (var i = 0;  i < meetingResource.ActiveParticipantJoined.Count; i++)
                {
                    cardData.activeParticipantsJoined.Add(new AdaptiveFact() { Title =  (i + 1).ToString(), Value = meetingResource.ActiveParticipantJoined[i].Identity.User.DisplayName });
                }
            }

            if (meetingResource.ActiveParticipantLeft.Count > 0)
            {
                for (var i = 0; i < meetingResource.ActiveParticipantLeft.Count; i++)
                {
                    cardData.activeParticipantsLeft.Add(new AdaptiveFact() { Title = (i + 1).ToString(), Value = meetingResource.ActiveParticipantLeft[i].Identity.User.DisplayName });
                }
            }

            var adaptiveCardJson = cardtemplate.Expand(cardData);
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        private static AdaptiveCardTemplate GetCardTemplate(string fileName)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }
    }
}