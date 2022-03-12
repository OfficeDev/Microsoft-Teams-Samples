// <copyright file="CardFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.UserSpecificViews.Cards
{
    using System;
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.UserSpecificViews.Bot;
    using Newtonsoft.Json;

    /// <summary>
    /// Card factory implementation.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        private const string SelectCardTypeCardTemplatePath = "{0}\\assets\\templates\\select-card-type.json";
        private const string RefreshSpecificUserViewCardTemplatePath = "{0}\\assets\\templates\\refresh-specific-user.json";
        private const string RefreshAllUsersViewCardTemplatePath = "{0}\\assets\\templates\\refresh-all-users.json";
        private const string UpdatedBaseCardTemplatePath = "{0}\\assets\\templates\\updated-base-card.json";

        private const string BaseCardStatus = "Base";
        private const string UpdatedCardStatus = "Updated";
        private const string FinalCardStatus = "Final";

        private const string PersonalView = "Personal";
        private const string SharedView = "Shared";

        /// <summary>
        /// Initializes a new instance of the <see cref="CardFactory"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        public CardFactory()
        {
        }

        /// <inheritdoc/>
        public Attachment GetSelectCardTypeCard()
        {
            var data = new { };
            var template = GetCardTemplate(SelectCardTypeCardTemplatePath);
            var serializedJson = template.Expand(data);
            return CreateAttachment(serializedJson);
        }

        public Attachment GetAutoRefreshForAllUsersBaseCard(string cardType)
        {
            var count = 0; // Initial count is 0 for base card.
            var data = new
            {
                count,
                cardType,
                cardStatus = BaseCardStatus,
                trigger = "NA",
                view = SharedView,
                message = "Original Message"
            };

            var template = GetCardTemplate(RefreshAllUsersViewCardTemplatePath);
            var serializedJson = template.Expand(data);
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment GetAutoRefreshForSpecificUserBaseCard(string userMri, string cardType)
        {
            var count = 0;
            var data = new
            {
                count,
                cardType,
                cardStatus = BaseCardStatus,
                trigger = "NA",
                view = SharedView,
                userMri,
                message = "Original Message"
            };

            var template = GetCardTemplate(RefreshSpecificUserViewCardTemplatePath);
            var serializedJson = template.Expand(data);
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment GetUpdatedCardForUser(string userMri, RefreshActionData actionData)
        {
            var data = new
            {
                count = actionData.action.data.RefreshCount,
                userMri = userMri,
                cardType = actionData.action.data.CardType,
                cardStatus = UpdatedCardStatus,
                trigger = actionData.trigger,
                view = PersonalView,
                message = "Updated Message!"
            };

            var template = GetCardTemplate(RefreshSpecificUserViewCardTemplatePath);
            var serializedJson = template.Expand(data);
            return CreateAttachment(serializedJson);
        }

        

        /// <inheritdoc/>
        public Attachment GetFinalBaseCard(RefreshActionData actionData)
        {
            var data = new
            {
                count = actionData.action.data.RefreshCount,
                cardType = actionData.action.data.CardType,
                cardStatus = FinalCardStatus,
                trigger = actionData.trigger,
                view = SharedView,
                message = "Final Message!"
            };

            var template = GetCardTemplate(UpdatedBaseCardTemplatePath);
            var serializedJson = template.Expand(data);
            return CreateAttachment(serializedJson);
        }

        private static AdaptiveCardTemplate GetCardTemplate(string templatePath)
        {
            templatePath = string.Format(templatePath, AppDomain.CurrentDomain.BaseDirectory);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }

        private static Attachment CreateAttachment(string adaptiveCardJson)
        {
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
        }
    }
}
