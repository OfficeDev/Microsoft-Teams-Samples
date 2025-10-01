/// <summary>
/// Copyright(c) Microsoft. All Rights Reserved.
/// Licensed under the MIT License.
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotAllCards.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// MainDialog class that handles the main dialog flow.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        public MainDialog(ILogger<MainDialog> logger)
             : base(nameof(MainDialog))
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Define the main dialog and its related components.
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                    ShowAllCardAsync,
                    SelectCardAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Prompts the user if the user is not in the middle of a dialog.
        /// Re-prompts the user when an invalid input is received.
        /// </summary>
        /// <param name="stepContext">Creates a new WaterfallStepContext instance.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> ShowAllCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("What card would you like to see? You can click the card name."),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 7."),
                Choices = LoadAllCards(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Send a Rich Card response to the user based on their choice.
        /// This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        /// </summary>
        /// <param name="stepContext">Initializes a new instance of the <see cref="WaterfallStepContext"/> class.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> SelectCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Get selected card value
            string selectedValue = ((FoundChoice)stepContext.Result).Value;

            // Decide which type of card(s) we are going to show the user
            switch (selectedValue)
            {
                case "InfoMasking":
                    reply.Attachments.Add(AllCards.SendInfoMasking());
                    break;
                case "FullWidthCard":
                    reply.Attachments.Add(AllCards.SendFullWidthCardAdaptiveCard());
                    break;
                case "StageViewImages":
                    reply.Attachments.Add(AllCards.SendStageViewImagesCard());
                    break;
                case "OverflowMenu":
                    reply.Attachments.Add(AllCards.SendOverflowMenuCard());
                    break;
                case "HTMLConnector":
                    reply.Attachments.Add(AllCards.SendHTMLConnectorCard());
                    break;
                case "CardWithEmoji":
                    reply.Attachments.Add(AllCards.SendCardWithEmoji());
                    break;
                case "Persona":
                    reply.Attachments.Add(AllCards.SendPersonaCardIcons());
                    break;
                case "PersonaSet":
                    reply.Attachments.Add(AllCards.SendPersonaCardSetIcons());
                    break;
                case "Layout":
                    reply.Attachments.Add(AllCards.SendResponsiveLayoutCard());
                    break;
                case "Border":
                    reply.Attachments.Add(AllCards.SendBorderCard());
                    break;
                case "RoundedCorners":
                    reply.Attachments.Add(AllCards.SendRoundedCornerCard());
                    break;
                case "FluentIcons":
                    reply.Attachments.Add(AllCards.SendFluentIconsCard());
                    break;
                case "MediaElements":
                    reply.Attachments.Add(AllCards.SendMediaElementsCard());
                    break;
                case "StarRatings":
                    reply.Attachments.Add(AllCards.SendStarRatingsCard());
                    break;
                case "ConditionalCard":
                    reply.Attachments.Add(AllCards.SendConditionalCard());
                    break;
                case "ScrollableCard":
                    reply.Attachments.Add(AllCards.SendScrollableCard());
                    break;
                case "CompoundButton":
                    reply.Attachments.Add(AllCards.SendCompoundButtonCard());
                    break;
                case "ContainerLayout":
                    reply.Attachments.Add(AllCards.SendContainerLayoutCard());
                    break;
                case "DonutChart":
                    reply.Attachments.Add(AllCards.SendDonutChartCard());
                    break;
                case "GaugeChart":
                    reply.Attachments.Add(AllCards.SendGaugeChartCard());
                    break;
                case "HorizontalChart":
                    reply.Attachments.Add(AllCards.SendHorizontalBarChartCard());
                    break;
                case "HorizontalStacked":
                    reply.Attachments.Add(AllCards.SendHorizontalBarStackedChartCard());
                    break;
                case "LineChart":
                    reply.Attachments.Add(AllCards.SendLineChartCard());
                    break;
                case "PieChart":
                    reply.Attachments.Add(AllCards.SendPieChartCard());
                    break;
                case "VerticalBarChart":
                    reply.Attachments.Add(AllCards.SendVerticalBarChartCard());
                    break;
                case "VerticalGroupedChart":
                    reply.Attachments.Add(AllCards.SendVerticalBarGroupedChartCard());
                    break;
                default:
                    reply.Attachments.Add(AllCards.SendInfoMasking());
                    reply.Attachments.Add(AllCards.SendFullWidthCardAdaptiveCard());
                    reply.Attachments.Add(AllCards.SendStageViewImagesCard());
                    reply.Attachments.Add(AllCards.SendOverflowMenuCard());
                    reply.Attachments.Add(AllCards.SendHTMLConnectorCard());
                    reply.Attachments.Add(AllCards.SendCardWithEmoji());
                    reply.Attachments.Add(AllCards.SendPersonaCardIcons());
                    reply.Attachments.Add(AllCards.SendPersonaCardSetIcons());
                    reply.Attachments.Add(AllCards.SendResponsiveLayoutCard());
                    reply.Attachments.Add(AllCards.SendBorderCard());
                    reply.Attachments.Add(AllCards.SendRoundedCornerCard());
                    reply.Attachments.Add(AllCards.SendFluentIconsCard());
                    reply.Attachments.Add(AllCards.SendMediaElementsCard());
                    reply.Attachments.Add(AllCards.SendStarRatingsCard());
                    reply.Attachments.Add(AllCards.SendConditionalCard());
                    reply.Attachments.Add(AllCards.SendScrollableCard());
                    reply.Attachments.Add(AllCards.SendCompoundButtonCard());
                    reply.Attachments.Add(AllCards.SendContainerLayoutCard());
                    reply.Attachments.Add(AllCards.SendDonutChartCard());
                    reply.Attachments.Add(AllCards.SendGaugeChartCard());
                    reply.Attachments.Add(AllCards.SendHorizontalBarChartCard());
                    reply.Attachments.Add(AllCards.SendHorizontalBarStackedChartCard());
                    reply.Attachments.Add(AllCards.SendLineChartCard());
                    reply.Attachments.Add(AllCards.SendPieChartCard());
                    reply.Attachments.Add(AllCards.SendVerticalBarChartCard());
                    reply.Attachments.Add(AllCards.SendVerticalBarGroupedChartCard());
                    break;
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You have selected <b>{selectedValue}</b>"), cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see all cards."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Load all cards.
        /// </summary>
        /// <returns>Returns the list of all cards.</returns>
        private IList<Choice> LoadAllCards()
        {
            try
            {
                var cardOptions = new List<Choice>
                    {
                        new Choice { Value = "MentionSupport", Synonyms = new List<string> { "MentionSupport" } },
                        new Choice { Value = "InfoMasking", Synonyms = new List<string> { "InfoMasking" } },
                        new Choice { Value = "FullWidthCard", Synonyms = new List<string> { "FullWidthCard" } },
                        new Choice { Value = "StageViewImages", Synonyms = new List<string> { "StageViewImages" } },
                        new Choice { Value = "OverflowMenu", Synonyms = new List<string> { "OverflowMenu" } },
                        new Choice { Value = "HTMLConnector", Synonyms = new List<string> { "HTMLConnector" } },
                        new Choice { Value = "CardWithEmoji", Synonyms = new List<string> { "CardWithEmoji" } },
                        new Choice { Value = "Persona", Synonyms = new List<string> { "Persona" } },
                        new Choice { Value = "PersonaSet", Synonyms = new List<string> { "PersonaSet" } },
                        new Choice { Value = "Layout", Synonyms = new List<string> { "Layout" } },
                        new Choice { Value = "Border", Synonyms = new List<string> { "Border" } },
                        new Choice { Value = "RoundedCorners", Synonyms = new List<string> { "RoundedCorners" } },
                        new Choice { Value = "FluentIcons", Synonyms = new List<string> { "FluentIcons" } },
                        new Choice { Value = "MediaElements", Synonyms = new List<string> { "MediaElements" } },
                        new Choice { Value = "StarRatings", Synonyms = new List<string> { "StarRatings" } },
                        new Choice { Value = "ConditionalCard", Synonyms = new List<string> { "ConditionalCard" } },
                        new Choice { Value = "ScrollableCard", Synonyms = new List<string> { "ScrollableCard" } },
                        new Choice { Value = "CompoundButton", Synonyms = new List<string> { "CompoundButton" } },
                        new Choice { Value = "ContainerLayout", Synonyms = new List<string> { "ContainerLayout" } },
                        new Choice { Value = "DonutChart", Synonyms = new List<string> { "DonutChart" } },
                        new Choice { Value = "GaugeChart", Synonyms = new List<string> { "GaugeChart" } },
                        new Choice { Value = "HorizontalChart", Synonyms = new List<string> { "HorizontalChart" } },
                        new Choice { Value = "HorizontalStacked", Synonyms = new List<string> { "HorizontalStacked" } },
                        new Choice { Value = "LineChart", Synonyms = new List<string> { "LineChart" } },
                        new Choice { Value = "PieChart", Synonyms = new List<string> { "PieChart" } },
                        new Choice { Value = "VerticalBarChart", Synonyms = new List<string> { "VerticalBarChart" } },
                        new Choice { Value = "VerticalGroupedChart", Synonyms = new List<string> { "VerticalGroupedChart" } }
                    };

                return cardOptions;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading card options.");
                return null;
            }
        }
    }
}