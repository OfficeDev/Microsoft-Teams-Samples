// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace BotAllCards.Cards
{
    public static class AllCards
    {
        /// <summary>
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment CreateAdaptiveCardAttachment()
        {
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// This card typically contains a single large image, one or more buttons, and a small amount of text.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment GetHeroCard()
        {
            var paths = new[] { ".", "Resources", "heroCard.json" };
            var heroCardJson = File.ReadAllText(Path.Combine(paths));

            var heroCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.hero",
                Content = JsonConvert.DeserializeObject(heroCardJson),
            };

            return heroCardAttachment;
        }

        /// <summary>
        /// This card typically contains a single thumbnail image, some short text, and one or more buttons.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment GetThumbnailCard()
        {
            var paths = new[] { ".", "Resources", "heroCard.json" };
            var thumbnailCardJson = File.ReadAllText(Path.Combine(paths));

            var thumbnailCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.thumbnail",
                Content = JsonConvert.DeserializeObject(thumbnailCardJson),
            };

            return thumbnailCardAttachment;
        }

        /// <summary>
        /// This card enables a bot to request that a user signs in.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.SigninCard results.</returns>
        public static SigninCard GetSigninCard()
        {
            var signinCard = new SigninCard
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl, "Sign-in", value: "https://login.microsoftonline.com/") },
            };

            return signinCard;
        }

        /// <summary>
        /// This card collection is used to return multiple items in a single response.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results</returns>
        public static Attachment CollectionsCardAttachment()
        {
            var paths = new[] { ".", "Resources", "collectionsCard.json" };
            var collectionsCardJson = File.ReadAllText(Path.Combine(paths));

            var collectionsCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(collectionsCardJson),
            };

            return collectionsCardAttachment;
        }

        /// <summary>
        /// This card has a flexible layout with multiple sections, fields, images, and actions.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results</returns>
        public static Attachment Office365ConnectorCard()
        {
            var paths = new[] { ".", "Resources", "o365ConnectorCard.json" };
            var OfficeCardJson = File.ReadAllText(Path.Combine(paths));

            var OfficeCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.teams.card.o365connector",
                Content = JsonConvert.DeserializeObject(OfficeCardJson),
            };

            return OfficeCardAttachment;
        }

        /// <summary>
        /// This card enables a bot to request that a user OAuth Card
        /// </summary>
        /// <returns>>Return Microsoft.Bot.Schema.Attachment results</returns>
        public static OAuthCard GetOAuthCard(string ConnectionName)
        {
            var oauthCard = new OAuthCard
            {
                Text = "BotFramework OAuth Card",
                ConnectionName = ConnectionName,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Sign In", value: "https://login.microsoftonline.com/") },
            };

            return oauthCard;
        }

        /// <summary>
        /// This card contains a scrolling list of items.
        /// </summary>
        /// <returns>>Return Microsoft.Bot.Schema.Attachment results</returns>
        public static Attachment CreateListCardAttachment()
        {
            var paths = new[] { ".", "Resources", "listCard.json" };
            var listCardJson = File.ReadAllText(Path.Combine(paths));

            var listCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.teams.card.list",
                Content = JsonConvert.DeserializeObject(listCardJson),
            };

            return listCardAttachment;
        }

    }
}
