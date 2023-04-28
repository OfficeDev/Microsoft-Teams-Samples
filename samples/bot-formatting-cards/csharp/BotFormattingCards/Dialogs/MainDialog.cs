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
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        public MainDialog(ILogger<MainDialog> logger)
             : base(nameof(MainDialog))
        {
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
        /// <returns>A task that represents the work queued to execute.</returns>>
        private async Task<DialogTurnResult> ShowAllCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What card would you like to see? You can click the card name"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 7."),
                Choices = loadAllCards(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Send a Rich Card response to the user based on their choice.
        /// This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        /// </summary>
        /// <param name="stepContext"> Initializes a new instance of the <see cref="WaterfallStepContext"/> class.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> SelectCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Get sellected card value
            string selectedValue = ((FoundChoice)stepContext.Result).Value.ToString();

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                
                case "InfoMasking":
                    // Sends Information masking in Adaptive Cards
                    reply.Attachments.Add(AllCards.sendInfoMasking());
                    break;
                case "FullWidthCard":
                    // Sends Full Width Card
                    reply.Attachments.Add(AllCards.sendFullWidthCardAdaptiveCard());
                    break;
                case "StageViewImages":
                    // Sends StageView Images Card
                    reply.Attachments.Add(AllCards.sendStageViewImagesCard());
                    break;
                case "OverflowMenu":
                    // Sends OverFlow Menu Card
                    reply.Attachments.Add(AllCards.sendOverFlowMenuCard());
                    break;
                case "HTMLConnector":
                    // Sends HTML Connector Card
                    reply.Attachments.Add(AllCards.sendHTMLConnectorCard());
                    break;
                case "CardWithEmoji":
                    // Sends Card With Emoji
                    reply.Attachments.Add(AllCards.sendCardWithEmoji());
                    break;
                default:
                    reply.Attachments.Add(AllCards.sendInfoMasking());
                    reply.Attachments.Add(AllCards.sendFullWidthCardAdaptiveCard());
                    reply.Attachments.Add(AllCards.sendStageViewImagesCard());
                    reply.Attachments.Add(AllCards.sendOverFlowMenuCard());
                    reply.Attachments.Add(AllCards.sendHTMLConnectorCard());
                    reply.Attachments.Add(AllCards.sendCardWithEmoji());
                    break;
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("You have Selected <b>" + selectedValue + "</b>"), cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see all card."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Load All Cards
        /// </summary>
        /// <returns>Return the value of all cards on the list.</returns>
        private IList<Choice> loadAllCards()
        {
            try
            {
                var returncardOptions = new List<Choice>()
                {
                    new Choice() { Value = "MentionSupport", Synonyms = new List<string>() { "MentionSupport" } },
                    new Choice() { Value = "InfoMasking", Synonyms = new List<string>() { "InfoMasking" } },
                    new Choice() { Value = "FullWidthCard", Synonyms = new List<string>() { "FullWidthCard" } },
                    new Choice() { Value = "StageViewImages", Synonyms = new List<string>() { "StageViewImages" } },
                    new Choice() { Value = "OverflowMenu", Synonyms = new List<string>() { "OverflowMenu" } },
                    new Choice() { Value = "HTMLConnector", Synonyms = new List<string>() { "HTMLConnector" } },
                    new Choice() { Value = "CardWithEmoji", Synonyms = new List<string>() { "CardWithEmoji" } },
                };

                return returncardOptions;
            }
            catch (Exception Exc)
            {
                Logger.LogInformation(Exc.Message.ToString());
                return null;
            }
        }
    }
}
