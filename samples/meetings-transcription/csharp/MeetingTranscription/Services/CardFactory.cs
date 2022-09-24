// <copyright file="CardFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscription.Services
{
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Returns CardAttachment.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        /// <inheritdoc/>
        public Attachment CreateAdaptiveCardAttachement(object dataObj)
        {
            var cardJSON = File.ReadAllText(Path.Combine(".", "Resources", "TranscriptCard.json"));
            if (dataObj != null)
            {
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJSON);
                cardJSON = template.Expand(dataObj);
            }

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return adaptiveCardAttachment;
        }

        /// <inheritdoc/>
        public Attachment CreateNotFoundCardAttachement()
        {
            var cardJSON = File.ReadAllText(Path.Combine(".", "Resources", "NotFoundCard.json"));
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return adaptiveCardAttachment;
        }
    }
}
