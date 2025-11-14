/// <summary>
/// Copyright(c) Microsoft. All Rights Reserved.
/// Licensed under the MIT License.
/// </summary>
using System.Text.Json;
using AdaptiveCards.Templating;
using Microsoft.Teams.Cards;
namespace bot_formatting_cards.Cards
{
    public static class AllCards
    {

        
        /// <summary>
        /// Sends Mention Support Card.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <param name="name">The name of the user to mention.</param>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendMentionSupportCardAsync(string name)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "mentionSupport.json"));

            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var memberData = new { userName = name };

            string cardJson = template.Expand(memberData);
            var card = JsonSerializer.Deserialize<AdaptiveCard>(cardJson);

            return card;
        }

        /// <summary>
        /// Sends Information Masking in Adaptive Cards.
        /// Use the information masking property to mask specific information, such as password or sensitive information from users within the Adaptive Card Input.Text input element.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendInfoMasking()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "informationMasking.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Sends Sample Adaptive Card With Full Width.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendFullWidthCardAdaptiveCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "sampleAdaptiveWithFullWidth.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Stage view for images in Adaptive Cards.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendStageViewImagesCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "stageViewForImages.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Sends Overflow Menu Card.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendOverflowMenuCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "overflowMenu.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Format sample for text formatting in Adaptive Cards.
        /// The following code shows an example of formatting options similar to HTML connector cards, using Markdown.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendHTMLConnectorCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "formatHTMLConnectorCard.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Sends Card With Emoji.
        /// An Adaptive Card is a customizable card that can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendCardWithEmoji()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardWithEmoji.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Persona card Icon in an Adaptive Card.
        /// If you want to show a single user in an Adaptive Card, the Adaptive Card displays the people icon and the name of the user.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendPersonaCardIcons()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptivePeoplePersonaCardIcon.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Persona Card Set Icon in an Adaptive Card.
        /// If you want to show multiple users in an Adaptive Card, the Adaptive Card displays only the people icon of the users.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendPersonaCardSetIcons()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptivePeoplePersonaCardSetIcon.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Adaptive Card updated to be responsive using targetWidth.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendResponsiveLayoutCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardResponsiveLayout.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Adaptive Card showcasing the border feature.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendBorderCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardBorders.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Adaptive Card showcasing the rounded corner for different elements of adaptive card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendRoundedCornerCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardRoundedCorners.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Generates an Adaptive Card that includes Fluent icons.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendFluentIconsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardFluentIcon.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Creates and returns an Adaptive Card with media elements.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendMediaElementsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardMediaElements.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Sends a star ratings card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendStarRatingsCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardStarRatings.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Conditional enablement of action button.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendConditionalCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardConditional.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Scrollable container for adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendScrollableCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardScrollable.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Compound Button adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendCompoundButtonCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardCompoundButton.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Container Layout adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendContainerLayoutCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardContainerLayouts.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Donut Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendDonutChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardDonutChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Gauge Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendGaugeChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardGaugeChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Horizontal Bar Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendHorizontalBarChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardHorizontalBarChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Horizontal Bar Stacked Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendHorizontalBarStackedChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardHorizontalBarStacked.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Line Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendLineChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardLineChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Pie Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendPieChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardPieChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Vertical Bar Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendVerticalBarChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardVerticalBarChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Vertical Bar Grouped Chart adaptive Card.
        /// </summary>
        /// <returns>Returns AdaptiveCard.</returns>
        public static AdaptiveCard? SendVerticalBarGroupedChartCard()
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(".", "Resources", "adaptiveCardVerticalBarGroupedChart.json"));
            var card = JsonSerializer.Deserialize<AdaptiveCard>(adaptiveCardJson);
            return card;
        }

        /// <summary>
        /// Gets a list of all available card options
        /// </summary>
        /// <returns>Returns list of card names.</returns>
        public static List<string> GetAllCardNames()
        {
            return new List<string>
            {
                "MentionSupport",
                "InfoMasking",
                "FullWidthCard",
                "StageViewImages",
                "OverflowMenu",
                "HTMLConnector",
                "CardWithEmoji",
                "Persona",
                "PersonaSet",
                "Layout",
                "Border",
                "RoundedCorners",
                "FluentIcons",
                "MediaElements",
                "StarRatings",
                "ConditionalCard",
                "ScrollableCard",
                "CompoundButton",
                "ContainerLayout",
                "DonutChart",
                "GaugeChart",
                "HorizontalChart",
                "HorizontalStacked",
                "LineChart",
                "PieChart",
                "VerticalBarChart",
                "VerticalGroupedChart"
            };
        }
    }
}
