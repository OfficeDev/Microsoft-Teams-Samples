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
        /// ContentType: mimetype/Contenttype for the file.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.card.adaptive";

        /// <summary>
        /// Sends Mention Support Card.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <param name="name">The name of the user to mention.</param>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendMentionSupportCardAsync(string name)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "mentionSupport.json"));

            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var memberData = new { userName = name };

            string cardJson = template.Expand(memberData);
            var mentionSupportAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            return mentionSupportAdaptiveCardAttachment;
        }

        /// <summary>
        /// Sends Information Masking in Adaptive Cards.
        /// Use the information masking property to mask specific information, such as password or sensitive information from users within the Adaptive Card Input.Text input element.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendInfoMasking()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "informationMasking.json"));

            var infoMaskingAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return infoMaskingAdaptiveCardAttachment;
        }

        /// <summary>
        /// Sends Sample Adaptive Card With Full Width.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendFullWidthCardAdaptiveCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "sampleAdaptiveWithFullWidth.json"));

            var fullWidthAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return fullWidthAdaptiveCardAttachment;
        }

        /// <summary>
        /// Stage view for images in Adaptive Cards.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendStageViewImagesCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "stageViewForImages.json"));

            var stageViewImagesAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return stageViewImagesAdaptiveCardAttachment;
        }

        /// <summary>
        /// Sends Overflow Menu Card.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendOverflowMenuCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "overflowMenu.json"));

            var overflowMenuAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return overflowMenuAdaptiveCardAttachment;
        }

        /// <summary>
        /// Format sample for HTML connector cards.
        /// The following code shows an example of formatting for HTML connector cards.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendHTMLConnectorCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "formatHTMLConnectorCard.json"));

            var htmlConnectorAdaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.teams.card.o365connector",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return htmlConnectorAdaptiveCardAttachment;
        }

        /// <summary>
        /// Sends Card With Emoji.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendCardWithEmoji()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardWithEmoji.json"));

            var emojiAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return emojiAdaptiveCardAttachment;
        }

        /// <summary>
        /// Persona card Icon in an Adaptive Card.
        /// If you want to show a single user in an Adaptive Card, the Adaptive Card displays the people icon and the name of the user.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendPersonaCardIcons()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptivePeoplePersonaCardIcon.json"));

            var personaCardIconAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return personaCardIconAdaptiveCardAttachment;
        }

        /// <summary>
        /// Persona Card Set Icon in an Adaptive Card.
        /// If you want to show multiple users in an Adaptive Card, the Adaptive Card displays only the people icon of the users.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendPersonaCardSetIcons()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptivePeoplePersonaCardSetIcon.json"));

            var personaCardSetIconAdaptiveCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return personaCardSetIconAdaptiveCardAttachment;
        }

        /// <summary>
        /// Adaptive Card updated to be responsive using targetWidth.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendResponsiveLayoutCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardResponsiveLayout.json"));

            var responsiveLayoutCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return responsiveLayoutCardAttachment;
        }

        /// <summary>
        /// Adaptive Card showcasing the border feature.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendBorderCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardBorders.json"));

            var borderCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return borderCardAttachment;
        }

        /// <summary>
        /// Adaptive Card showcasing the rounded corner for different elements of adaptive card.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendRoundedCornerCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardRoundedCorners.json"));

            var roundedCornersCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return roundedCornersCardAttachment;
        }

        /// <summary>
        /// Generates an Adaptive Card attachment that includes Fluent icons.
        /// This method reads the adaptive card JSON from a resource file and
        /// deserializes it to be included as an attachment with Fluent icons in the card.
        /// Fluent icons provide a modern and visually appealing way to enhance the UI within Adaptive Cards.
        /// </summary>
        /// <returns>Returns an Attachment object that contains the Adaptive Card with Fluent icons.</returns>
        public static Attachment SendFluentIconsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardFluentIcon.json"));

            var fluentIconsCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return fluentIconsCardAttachment;
        }

        /// <summary>
        /// Creates and returns an Attachment containing an adaptive card with media elements.
        /// </summary>
        /// <returns>Returns an Attachment object with a media elements adaptive card.</returns>
        public static Attachment SendMediaElementsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardMediaElements.json"));

            var mediaElementsCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return mediaElementsCardAttachment;
        }

        /// <summary>
        /// Sends a star ratings card as an attachment for displaying or collecting user feedback.
        /// </summary>
        /// <returns>Returns an Attachment object containing the star ratings card, which can display read-only ratings or collect ratings from users.</returns>
        public static Attachment SendStarRatingsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardStarRatings.json"));

            var starRatingsCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return starRatingsCardAttachment;
        }

        /// <summary>
        /// Conditional enablement of action button.
        /// To enable your Action.Submit button only when the user fills out at least one input field and disables the button again if the user clears the input.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendConditionalCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardConditional.json"));

            var conditionalCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return conditionalCardAttachment;
        }

        /// <summary>
        /// Scrollable container for adaptive Card.
        /// If you have a long list of items in a container within an adaptive card, it can make the card quite tall. To address this, you can use a scrollable container to limit the card's height.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendScrollableCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardScrollable.json"));

            var scrollableCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return scrollableCardAttachment;
        }

        /// <summary>
        /// Compound Button adaptive Card.
        /// Sends adaptive card showing compound button.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendCompoundButtonCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardCompoundButton.json"));

            var compoundButtonCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return compoundButtonCardAttachment;
        }

        /// <summary>
        /// Container Layout adaptive Card.
        /// Sends adaptive card showing Container Layout.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendContainerLayoutCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardContainerLayouts.json"));

            var containerLayoutCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return containerLayoutCardAttachment;
        }

        /// <summary>
        /// Donut Chart adaptive Card.
        /// Sends adaptive card showing Donut Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendDonutChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardDonutChart.json"));

            var donutChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return donutChartCardAttachment;
        }

        /// <summary>
        /// Gauge Chart adaptive Card.
        /// Sends adaptive card showing Gauge Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendGaugeChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardGaugeChart.json"));

            var gaugeChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return gaugeChartCardAttachment;
        }

        /// <summary>
        /// Horizontal Bar Chart adaptive Card.
        /// Sends adaptive card showing Horizontal Bar Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendHorizontalBarChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardHorizontalBarChart.json"));

            var horizontalBarChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return horizontalBarChartCardAttachment;
        }

        /// <summary>
        /// Horizontal Bar Stacked Chart adaptive Card.
        /// Sends adaptive card showing Horizontal Bar Stacked Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendHorizontalBarStackedChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardHorizontalBarStacked.json"));

            var horizontalBarStackedChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return horizontalBarStackedChartCardAttachment;
        }

        /// <summary>
        /// Line Chart adaptive Card.
        /// Sends adaptive card showing Line Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendLineChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardLineChart.json"));

            var lineChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return lineChartCardAttachment;
        }

        /// <summary>
        /// Pie Chart adaptive Card.
        /// Sends adaptive card showing Pie Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendPieChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardPieChart.json"));

            var pieChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return pieChartCardAttachment;
        }

        /// <summary>
        /// Vertical Bar Chart adaptive Card.
        /// Sends adaptive card showing Vertical Bar Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendVerticalBarChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardVerticalBarChart.json"));

            var verticalBarChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return verticalBarChartCardAttachment;
        }

        /// <summary>
        /// Vertical Bar Grouped Chart adaptive Card.
        /// Sends adaptive card showing Vertical Bar Grouped Chart.
        /// </summary>
        /// <returns>Returns Microsoft.Bot.Schema.Attachment results.</returns>
        public static Attachment SendVerticalBarGroupedChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardVerticalBarGroupedChart.json"));

            var verticalBarGroupedChartCardAttachment = new Attachment
            {
                ContentType = ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return verticalBarGroupedChartCardAttachment;
        }
    }
}
