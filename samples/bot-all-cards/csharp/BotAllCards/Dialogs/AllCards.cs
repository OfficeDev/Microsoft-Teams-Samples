// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace BotAllCards.Dialogs
{
    public static class AllCards
    {
        /// <summary>
        /// Adaptive Card
        /// </summary>
        /// <returns></returns>
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
        /// Hero Card
        /// </summary>
        /// <returns></returns>
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
        /// Thumbnail Card
        /// </summary>
        /// <returns></returns>
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
        /// Signin Card
        /// </summary>
        /// <returns></returns>
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
        /// Card collections
        /// </summary>
        /// <returns></returns>
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
        /// Office 365 connector card
        /// </summary>
        /// <returns></returns>
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
        /// OAuth Card
        /// </summary>
        /// <returns></returns>
        public static OAuthCard GetOAuthCard()
        {
            var oauthCard = new OAuthCard
            {
                Text = "BotFramework OAuth Card",
                ConnectionName = "YOUR-CONNECTION-NAME", // Replace with the name of your Azure AD connection.
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Sign In", value: "https://login.microsoftonline.com/") },
            };

            return oauthCard;
        }

        /// <summary>
        /// List card
        /// </summary>
        /// <returns></returns>
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
