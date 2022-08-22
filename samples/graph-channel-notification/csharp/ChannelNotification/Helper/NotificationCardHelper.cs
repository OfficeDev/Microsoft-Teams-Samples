// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChannelNotification.Helper
{
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using System.IO;

    public static class NotificationCardHelper
    {
        public static Microsoft.Bot.Schema.Attachment GetChannelUpdateCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelUpdate.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }
        public static Microsoft.Bot.Schema.Attachment GetChannelCreateCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelCreate.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }
        public static Microsoft.Bot.Schema.Attachment GetChannelDeleteCard(string channelName)
        {
            var cardtemplate = GetCardTemplate("ChannelDelete.json");

            var adaptiveCardJson = cardtemplate.Expand(new
            {
                channelName = channelName
            });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card
            };
        }
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
        private static AdaptiveCardTemplate GetCardTemplate(string fileName)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }
    }
}