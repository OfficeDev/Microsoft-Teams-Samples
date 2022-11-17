// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using global::AdaptiveCards;
using global::AdaptiveCards.Templating;
using Microsoft.Bot.Schema;

namespace CallingBotSample.AdaptiveCards
{
    public class AdaptiveCardFactory : IAdaptiveCardFactory
    {
        public Attachment CreateWelcomeCard()
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
