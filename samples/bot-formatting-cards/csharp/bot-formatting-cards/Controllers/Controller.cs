using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using bot_formatting_cards.Cards;
using System.Text.Json;

namespace bot_formatting_cards.Controllers
{
    [TeamsController]
    public class Controller()
    {
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info($"Received message: {activity.Text}");

            var text = activity.Text?.Trim() ?? "";

            // Check if this is an adaptive card action (button click)
            if (activity.Value != null)
            {
                log.Info("Activity has Value - this is an adaptive card action");
                var valueJson = JsonSerializer.Serialize(activity.Value);
                log.Info($"Activity Value: {valueJson}");

                try
                {
                    // Try to deserialize the Value as a dictionary
                    Dictionary<string, JsonElement>? actionData = null;
                    
                    if (activity.Value is JsonElement jsonElement)
                    {
                        actionData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonElement.GetRawText());
                    }
                    else if (activity.Value is string jsonString)
                    {
                        actionData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
                    }

                    if (actionData != null)
                    {
                        // Check for cardType in the action data
                        if (actionData.TryGetValue("cardType", out var cardTypeElement))
                        {
                            var cardName = cardTypeElement.GetString();
                            log.Info($"Card type selected: {cardName}");
                            
                            if (!string.IsNullOrEmpty(cardName))
                            {
                                var selectedCard = GetCardByName(cardName);
                                if (selectedCard != null)
                                {
                                    await client.Send(selectedCard);
                                    await client.Send($"You have selected **{cardName}**");
                                    await client.Send("Type anything to see all card options.");
                                    await SendCardSelectionMenu(client);
                                    return;
                                }
                            }
                        }

                        // Handle star ratings feedback
                        if (actionData.ContainsKey("rating1") && actionData.ContainsKey("rating2"))
                        {
                            await client.Send($"Ratings Feedback: {valueJson}");
                            await client.Send("Type anything to see all card options.");
                            await SendCardSelectionMenu(client);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error parsing activity value: {ex.Message}");
                }
            }

            // Default: Show card selection menu
            await SendCardSelectionMenu(client);
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "Welcome to Adaptive Card Format Bot! This bot will introduce you to different types of card formats. Type anything to get started.";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        [Microsoft.Teams.Apps.Activities.Invokes.AdaptiveCard.Action]
        public async Task OnAdaptiveCardAction([Context] Microsoft.Teams.Api.Activities.Invokes.AdaptiveCards.ActionActivity actionActivity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Received adaptive card action");

            // Handle card selection from the action submit
            var actionData = actionActivity.Value?.Action?.Data;
            if (actionData != null)
            {
                var actionJson = JsonSerializer.Serialize(actionData);
                log.Info($"Action data: {actionJson}");
                
                // Try to extract cardType from the data dictionary
                if (actionData is IDictionary<string, object> dataDictionary)
                {
                    if (dataDictionary.TryGetValue("cardType", out var cardTypeValue))
                    {
                        var cardName = cardTypeValue?.ToString();
                        if (!string.IsNullOrEmpty(cardName))
                        {
                            var selectedCard = GetCardByName(cardName);
                            if (selectedCard != null)
                            {
                                await client.Send(selectedCard);
                                await client.Send($"You have selected **{cardName}**");
                                await client.Send("Type anything to see all card options.");
                                await SendCardSelectionMenu(client);
                                return;
                            }
                        }
                    }
                    
                    // Handle star ratings feedback
                    if (dataDictionary.ContainsKey("rating1") && dataDictionary.ContainsKey("rating2"))
                    {
                        await client.Send($"Ratings Feedback: {actionJson}");
                        await client.Send("Type anything to see all card options.");
                        await SendCardSelectionMenu(client);
                        return;
                    }
                }
            }

            await client.Send("Action received!");
            await SendCardSelectionMenu(client);
        }

        private static AdaptiveCard? GetCardByName(string cardName)
        {
            return cardName switch
            {
                "MentionSupport" => AllCards.SendMentionSupportCardAsync("User"),
                "InfoMasking" => AllCards.SendInfoMasking(),
                "FullWidthCard" => AllCards.SendFullWidthCardAdaptiveCard(),
                "StageViewImages" => AllCards.SendStageViewImagesCard(),
                "OverflowMenu" => AllCards.SendOverflowMenuCard(),
                "CardWithEmoji" => AllCards.SendCardWithEmoji(),
                "Persona" => AllCards.SendPersonaCardIcons(),
                "PersonaSet" => AllCards.SendPersonaCardSetIcons(),
                "Layout" => AllCards.SendResponsiveLayoutCard(),
                "Border" => AllCards.SendBorderCard(),
                "RoundedCorners" => AllCards.SendRoundedCornerCard(),
                "FluentIcons" => AllCards.SendFluentIconsCard(),
                "MediaElements" => AllCards.SendMediaElementsCard(),
                "StarRatings" => AllCards.SendStarRatingsCard(),
                "ConditionalCard" => AllCards.SendConditionalCard(),
                "ScrollableCard" => AllCards.SendScrollableCard(),
                "CompoundButton" => AllCards.SendCompoundButtonCard(),
                "ContainerLayout" => AllCards.SendContainerLayoutCard(),
                "DonutChart" => AllCards.SendDonutChartCard(),
                "GaugeChart" => AllCards.SendGaugeChartCard(),
                "HorizontalChart" => AllCards.SendHorizontalBarChartCard(),
                "HorizontalStacked" => AllCards.SendHorizontalBarStackedChartCard(),
                "LineChart" => AllCards.SendLineChartCard(),
                "PieChart" => AllCards.SendPieChartCard(),
                "VerticalBarChart" => AllCards.SendVerticalBarChartCard(),
                "VerticalGroupedChart" => AllCards.SendVerticalBarGroupedChartCard(),
                _ => null
            };
        }

        private static async Task SendCardSelectionMenu(IContext.Client client, int pageNumber = 1)
        {
            // Create carousel pages for all cards
            var carouselPages = new List<CarouselPage>();
            
            var allActions = LoadAllCardActions();
            const int cardsPerPage = 5;
            var totalPages = (int)Math.Ceiling(allActions.Count / (double)cardsPerPage);

            // Create a page for each set of cards
            for (int page = 1; page <= totalPages; page++)
            {
                var pageActions = allActions
                    .Skip((page - 1) * cardsPerPage)
                    .Take(cardsPerPage)
                    .ToList();

                var pageElements = new List<CardElement>
                {
                    new TextBlock("What card would you like to see? You can click the card name")
                    {
                        Wrap = true,
                        Size = TextSize.Default
                    },
                    new ActionSet
                    {
                        Actions = pageActions
                    },
                    new TextBlock($"Card {page} out of {totalPages}")
                    {
                        Size = TextSize.Small,
                        Wrap = true,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Spacing = Spacing.Medium
                    }
                };

                carouselPages.Add(new CarouselPage(pageElements));
            }

            // Create the adaptive card with carousel
            var card = new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Body = new List<CardElement>
                {
                    new Carousel
                    {
                        Pages = carouselPages
                    }
                }
            };

            await client.Send(card);
        }

        /// <summary>
        /// Load all cards as Action.Submit buttons similar to Bot Builder's Choice options.
        /// </summary>
        /// <returns>Returns the list of all card actions.</returns>
        private static List<Microsoft.Teams.Cards.Action> LoadAllCardActions()
        {
            return new List<Microsoft.Teams.Cards.Action>
            {
                new SubmitAction { Title = "MentionSupport", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "MentionSupport" } } }) },
                new SubmitAction { Title = "InfoMasking", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "InfoMasking" } } }) },
                new SubmitAction { Title = "FullWidthCard", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "FullWidthCard" } } }) },
                new SubmitAction { Title = "StageViewImages", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "StageViewImages" } } }) },
                new SubmitAction { Title = "OverflowMenu", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "OverflowMenu" } } }) },
                new SubmitAction { Title = "HTMLConnector", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "HTMLConnector" } } }) },
                new SubmitAction { Title = "CardWithEmoji", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "CardWithEmoji" } } }) },
                new SubmitAction { Title = "Persona", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "Persona" } } }) },
                new SubmitAction { Title = "PersonaSet", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "PersonaSet" } } }) },
                new SubmitAction { Title = "Layout", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "Layout" } } }) },
                new SubmitAction { Title = "Border", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "Border" } } }) },
                new SubmitAction { Title = "RoundedCorners", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "RoundedCorners" } } }) },
                new SubmitAction { Title = "FluentIcons", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "FluentIcons" } } }) },
                new SubmitAction { Title = "MediaElements", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "MediaElements" } } }) },
                new SubmitAction { Title = "StarRatings", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "StarRatings" } } }) },
                new SubmitAction { Title = "ConditionalCard", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "ConditionalCard" } } }) },
                new SubmitAction { Title = "ScrollableCard", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "ScrollableCard" } } }) },
                new SubmitAction { Title = "CompoundButton", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "CompoundButton" } } }) },
                new SubmitAction { Title = "ContainerLayout", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "ContainerLayout" } } }) },
                new SubmitAction { Title = "DonutChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "DonutChart" } } }) },
                new SubmitAction { Title = "GaugeChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "GaugeChart" } } }) },
                new SubmitAction { Title = "HorizontalChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "HorizontalChart" } } }) },
                new SubmitAction { Title = "HorizontalStacked", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "HorizontalStacked" } } }) },
                new SubmitAction { Title = "LineChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "LineChart" } } }) },
                new SubmitAction { Title = "PieChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "PieChart" } } }) },
                new SubmitAction { Title = "VerticalBarChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "VerticalBarChart" } } }) },
                new SubmitAction { Title = "VerticalGroupedChart", Data = new Microsoft.Teams.Common.Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = new Dictionary<string, object?> { { "cardType", "VerticalGroupedChart" } } }) }
            };
        }
    }
}