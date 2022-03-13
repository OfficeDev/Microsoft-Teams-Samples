// <copyright file="CardFactory.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using System.IO;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Returns CardAttachment.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        /// <inheritdoc/>
        public Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var cardJSON = File.ReadAllText(filePath);

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            return adaptiveCardAttachment;
        }

        /// <inheritdoc/>
        public Attachment CreateAdaptiveCardAttachement(string filePath, object dataObj)
        {
            var cardJSON = File.ReadAllText(filePath);
            if (dataObj != null)
            {
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJSON);
                cardJSON = template.Expand(dataObj);
            }

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            return adaptiveCardAttachment;
        }
    }
}
