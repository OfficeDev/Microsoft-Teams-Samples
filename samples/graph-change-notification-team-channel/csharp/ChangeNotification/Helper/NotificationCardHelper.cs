// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Helper
{
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using System.IO;
    using Microsoft.Bot.Schema;

    public static class NotificationCardHelper
    {
        /// <summary>
        /// When Channel Updated GetChannelUpdateCard will display.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static Attachment GetChannelUpdateCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelUpdate.json");
            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        /// <summary>
        /// when new channel created GetChannelCreateCard will display.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static Attachment GetChannelCreateCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelCreate.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        /// <summary>
        /// when new channel created GetChannelDeleteCard will display.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static Attachment GetChannelDeleteCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelDelete.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }

        /// <summary>
        /// when subscription created for specific team GetSubscriptionInitializedCard will display.
        /// </summary>
        /// <param name="isWelcomeCard"></param>
        /// <returns></returns>
        public static Attachment GetSubscriptionInitializedCard(bool isWelcomeCard)
        {
            var cardtemplate = GetCardTemplate("WelcomeCard.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                isWelcomeCard= isWelcomeCard
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Attachment()
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