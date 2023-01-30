// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BotAllCards.Cards
{
    public static class AllCards
    {
        /// <summary>
        /// Sends Mention Support Card 
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendMentionSupportCard()
        {
            var paths = new[] { ".", "Resources", "mentionSupport.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var mentionSupportAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return mentionSupportAdaptiveCardAttachment;
        }
        /// <summary>
        /// Sends InfoMasking Card
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendInfoMasking()
        {
            var paths = new[] { ".", "Resources", "informationMasking.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var sendsInfoMaskingAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return sendsInfoMaskingAdaptiveCardAttachment;
        }
        /// <summary>
        /// Sends Sample Adaptive Card With Full Width
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendFullWidthCardAdaptiveCard()
        {
            var paths = new[] { ".", "Resources", "sampleAdaptiveWithFullWidth.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var fullWidthAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return fullWidthAdaptiveCardAttachment;
        }
        /// <summary>
        /// Stage view for images in Adaptive Cards
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendStageViewImagesCard()
        {
            var paths = new[] { ".", "Resources", "stageViewForImages.json" };
            var stageViewImagesAdaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(stageViewImagesAdaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }
        /// <summary>
        /// Sends OverFlow Menu Card
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendOverFlowMenuCard()
        {
            var paths = new[] { ".", "Resources", "overflowMenu.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var sendsOverFlowMenuAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return sendsOverFlowMenuAdaptiveCardAttachment;
        }
        /// <summary>
        /// Format sample for HTML connector cards
        /// The following code shows an example of formatting for HTML connector cards.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendHTMLConnectorCard()
        {
            var paths = new[] { ".", "Resources", "formatHTMLConnectorCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var htmlConnectorAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.teams.card.o365connector",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return htmlConnectorAdaptiveCardAttachment;
        }
        /// <summary>
        /// Sends Card With Emoji
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendCardWithEmoji()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardWithEmoji.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var emojiAdaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return emojiAdaptiveCardAttachment;
        }
    }
}
