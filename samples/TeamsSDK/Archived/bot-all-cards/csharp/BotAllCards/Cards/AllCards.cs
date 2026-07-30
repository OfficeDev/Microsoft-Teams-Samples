// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace BotAllCards.Cards
{
    /// <summary>
    /// Provides methods to create various types of cards.
    /// </summary>
    public static class AllCards
    {
        /// <summary>
        /// Creates an Adaptive Card attachment.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the Adaptive Card.</returns>
        public static Attachment CreateAdaptiveCardAttachment()
        {
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
        }

        /// <summary>
        /// Creates a Hero Card attachment.
        /// This card typically contains a single large image, one or more buttons, and a small amount of text.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the Hero Card.</returns>
        public static Attachment GetHeroCard()
        {
            var paths = new[] { ".", "Resources", "heroCard.json" };
            var heroCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.hero",
                Content = JsonConvert.DeserializeObject(heroCardJson),
            };
        }

        /// <summary>
        /// Creates a Thumbnail Card attachment.
        /// This card typically contains a single thumbnail image, some short text, and one or more buttons.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the Thumbnail Card.</returns>
        public static Attachment GetThumbnailCard()
        {
            var paths = new[] { ".", "Resources", "thumbnailCard.json" };
            var thumbnailCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.thumbnail",
                Content = JsonConvert.DeserializeObject(thumbnailCardJson),
            };
        }

        /// <summary>
        /// Creates a Sign-in Card.
        /// This card enables a bot to request that a user signs in.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.SigninCard representing the Sign-in Card.</returns>
        public static SigninCard GetSigninCard()
        {
            return new SigninCard
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, "Sign-in", value: "https://login.microsoftonline.com/")
                    },
            };
        }

        /// <summary>
        /// Creates a Collections Card attachment.
        /// This card collection is used to return multiple items in a single response.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the Collections Card.</returns>
        public static Attachment CollectionsCardAttachment()
        {
            var paths = new[] { ".", "Resources", "collectionsCard.json" };
            var collectionsCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(collectionsCardJson),
            };
        }

        /// <summary>
        /// Creates an Office 365 Connector Card attachment.
        /// This card has a flexible layout with multiple sections, fields, images, and actions.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the Office 365 Connector Card.</returns>
        public static Attachment Office365ConnectorCard()
        {
            var paths = new[] { ".", "Resources", "o365ConnectorCard.json" };
            var officeCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.teams.card.o365connector",
                Content = JsonConvert.DeserializeObject(officeCardJson),
            };
        }

        /// <summary>
        /// Creates an OAuth Card.
        /// This card enables a bot to request that a user signs in using OAuth.
        /// </summary>
        /// <param name="connectionName">The connection name for the OAuth provider.</param>
        /// <returns>A Microsoft.Bot.Schema.OAuthCard representing the OAuth Card.</returns>
        public static OAuthCard GetOAuthCard(string connectionName)
        {
            return new OAuthCard
            {
                Text = "BotFramework OAuth Card",
                ConnectionName = connectionName,
                Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, "Sign In", value: "https://login.microsoftonline.com/")
                    },
            };
        }

        /// <summary>
        /// Creates a List Card attachment.
        /// This card contains a scrolling list of items.
        /// </summary>
        /// <returns>A Microsoft.Bot.Schema.Attachment representing the List Card.</returns>
        public static Attachment CreateListCardAttachment()
        {
            var paths = new[] { ".", "Resources", "listCard.json" };
            var listCardJson = File.ReadAllText(Path.Combine(paths));

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.teams.card.list",
                Content = JsonConvert.DeserializeObject(listCardJson),
            };
        }
    }
}
