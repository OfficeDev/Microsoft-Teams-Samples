// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using AdaptiveCards;
using AdaptiveCards.Templating;
using CallingBotSample.Interfaces;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace CallingBotSample.Helpers
{
    /// <summary>
    /// Helper for cards.
    /// </summary>
    public class CardHelper : ICard
    {
        private readonly ILogger<CardHelper> logger;

        public CardHelper(ILogger<CardHelper> logger)
        {
            this.logger = logger;
        }

        public Attachment GetWelcomeCardAttachment()
        {
            var template = GetCardTemplate("WelcomeCard.json");

            var serializedJson = template.Expand(new { });
            return CreateAttachment(serializedJson);
        }

        private AdaptiveCardTemplate GetCardTemplate(string fileName)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }

        private Attachment CreateAttachment(string adaptiveCardJson)
        {
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card,
            };
        }
    }
}
