/// <summary>
/// Copyright(c) Microsoft. All Rights Reserved.
/// Licensed under the MIT License.
/// </summary>
using System.IO;
using Newtonsoft.Json;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
namespace BotAllCards.Cards
{
    public static class AllCards
    {
        /// <summary>
        /// ContentType:mimetype/Contenttype for the file.
        /// </summary>
        public const string contentType = "application/vnd.microsoft.card.adaptive";

        /// <summary>
        /// Sends Mention Support Card 
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendMentionSupportCardAsync(string name)
        {
            var paths = new[] { ".", "Resources", "mentionSupport.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var templateJSON = adaptiveCardJson;
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJSON);
            var memberData = new
            {
                userName = name
            };

            string cardJSON = template.Expand(memberData);
            var mentionSupportAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return mentionSupportAdaptiveCardAttachment;
        }

        /// <summary>
        /// Sends Information masking in Adaptive Cards
        /// Use the information masking property to mask specific information, such as password or sensitive information from users within the Adaptive Card Input.Text input element.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendInfoMasking()
        {
            var paths = new[] { ".", "Resources", "informationMasking.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var sendsInfoMaskingAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
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
                ContentType = contentType,
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
                ContentType = contentType,
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
                ContentType = contentType,
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
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return emojiAdaptiveCardAttachment;
        }

        /// <summary>
        /// Persona card Icon in an Adaptive Card
        /// If you want to show a single user in an Adaptive Card, the Adaptive Card displays the people icon and the name of the user.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendPersonaCardIcons()
        {
            var paths = new[] { ".", "Resources", "adaptivePeoplePersonaCardIcon.json" };
            var adaptiveCardPersonaCardIconJson = File.ReadAllText(Path.Combine(paths));

            var PersonaCardIconAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardPersonaCardIconJson),
            };

            return PersonaCardIconAdaptiveCardAttachment;
        }

        /// <summary>
        /// Persona Card Set Icon in an Adaptive Card
        /// If you want to show multiple users in an Adaptive Card, the Adaptive Card displays only the people icon of the users.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendPersonaCardSetIcons()
        {
            var paths = new[] { ".", "Resources", "adaptivePeoplePersonaCardSetIcon.json" };
            var adaptiveCardPersonaCardSetIconJson = File.ReadAllText(Path.Combine(paths));

            var PersonaCardSetIconAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardPersonaCardSetIconJson),
            };

            return PersonaCardSetIconAdaptiveCardAttachment;
        }
        /// <summary>
        /// Adaptive Card updated to be responsive using targetWidth.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment sendResponsiveLayoutCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardResponsiveLayout.json" };
            var adaptiveCardlayoutJson = File.ReadAllText(Path.Combine(paths));

            var LayoutCardAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardlayoutJson),
            };

            return LayoutCardAdaptiveCardAttachment;
        }

        /// <summary>
        /// Adaptive Card showcasing the border feature.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendBorderCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardBorders.json" };
            var adaptiveCardBorderJson = File.ReadAllText(Path.Combine(paths));

            var BorderAdaptiveCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardBorderJson),
            };

            return BorderAdaptiveCardAttachment;
        }

        /// <summary>
        /// Adaptive Card showcasing the rounded corner for different elements of adaptive card.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendRoundedCornerCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardRoundedCorners.json" };
            var adaptiveCardRoundedCornersJson = File.ReadAllText(Path.Combine(paths));

            var RoundedCornersCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardRoundedCornersJson),
            };

            return RoundedCornersCardAttachment;
        }

        /// <summary>
        /// Generates an Adaptive Card attachment that includes Fluent icons. 
        /// This method reads the adaptive card JSON from a resource file and 
        /// deserializes it to be included as an attachment with Fluent icons in the card.
        /// Fluent icons provide a modern and visually appealing way to enhance the UI within Adaptive Cards.
        /// </summary>
        /// <returns>
        /// Returns an Attachment object that contains the Adaptive Card with Fluent icons.
        /// </returns>
        public static Attachment SendFluentIconsCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardFluentIcon.json" };
            var adaptiveCardFluentIconsJson = File.ReadAllText(Path.Combine(paths));

            var FluentIconsCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardFluentIconsJson),
            };

            return FluentIconsCardAttachment;
        }

        /// <summary>
        /// Creates and returns an Attachment containing an adaptive card with media elements.
        /// </summary>
        /// <returns>An Attachment object with a media elements adaptive card.</returns>
        public static Attachment SendMediaElementsCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardMediaElements.json" };
            var adaptiveCardMediaElementsJson = File.ReadAllText(Path.Combine(paths));

            var MediaElementsCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardMediaElementsJson),
            };

            return MediaElementsCardAttachment;
        }

        /// <summary>
        /// Sends a star ratings card as an attachment for displaying or collecting user feedback.
        /// </summary>
        /// <returns>An Attachment object containing the star ratings card, which can display read-only ratings or collect ratings from users.</returns>
        public static Attachment SendStarRatingsCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardStarRatings.json" };
            var adaptiveCardStarRatingsJson = File.ReadAllText(Path.Combine(paths));

            var StarRatingsCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardStarRatingsJson),
            };

            return StarRatingsCardAttachment;
        }

        /// <summary>
        /// Conditinal enablement of action button
        /// To enable your Action.Submit button only when the user fills out at least one input field and disables the button again if the user clears the input.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendConditionalCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardConditional.json" };
            var adaptiveCardConditionalJson = File.ReadAllText(Path.Combine(paths));

            var ConditionalCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardConditionalJson),
            };

            return ConditionalCardAttachment;
        }

        /// <summary>
        /// Scorrable container for adaptive Card
        /// If you have a long list of items in a container within an adaptive card, it can make the card quite tall. To address this, you can use a scrollable container to limit the card's height.
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendScrollableCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardScrollable.json" };
            var adaptiveCardScrollableJson = File.ReadAllText(Path.Combine(paths));

            var ScrollableCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardScrollableJson),
            };

            return ScrollableCardAttachment;
        }

        /// <summary>
        /// Compound Button adaptive Card
        /// Sends adaptive card showing compound button
        /// </summary>
        /// <returns>Return Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendCompoundButtonCard()
        {
            var paths = new[] { ".", "Resources", "adaptiveCardCompoundButton.json" };
            var adaptiveCardCompoundButtonJson = File.ReadAllText(Path.Combine(paths));

            var CompoundButtonCardAttachment = new Attachment()
            {
                ContentType = contentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardCompoundButtonJson),
            };

            return CompoundButtonCardAttachment;
        }
    }
}
