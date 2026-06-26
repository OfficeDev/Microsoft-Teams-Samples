// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotAllCards.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Main dialog for handling card selection and display.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        protected string ConnectionName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        public MainDialog(IConfiguration configuration)
            : base(configuration["ConnectionName"])
        {
            ConnectionName = configuration["ConnectionName"];

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
        /// Prompts the user to select a card.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> ShowAllCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("What card would you like to see? You can click or type the card name"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 9."),
                Choices = LoadAllCards(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Sends a Rich Card response to the user based on their choice.
        /// This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> SelectCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Adaptive Card":
                    // Display an Adaptive Card
                    reply.Attachments.Add(AllCards.CreateAdaptiveCardAttachment());
                    break;
                case "Hero Card":
                    // Display a HeroCard.
                    reply.Attachments.Add(AllCards.GetHeroCard());
                    break;
                case "OAuth Card":
                    // Display an OAuthCard
                    reply.Attachments.Add(AllCards.GetOAuthCard(ConnectionName).ToAttachment());
                    break;
                case "Signin Card":
                    // Display a SignInCard.
                    reply.Attachments.Add(AllCards.GetSigninCard().ToAttachment());
                    break;
                case "Thumbnail Card":
                    // Display a ThumbnailCard.
                    reply.Attachments.Add(AllCards.GetThumbnailCard());
                    break;
                case "List Cards":
                    // Display a ListCard
                    reply.Attachments.Add(AllCards.CreateListCardAttachment());
                    break;
                case "Collections Cards":
                    // Display a CollectionsCard
                    reply.Attachments.Add(AllCards.CollectionsCardAttachment());
                    break;
                case "Connector Cards":
                    // Display a Office 365 Connector Cards
                    reply.Attachments.Add(AllCards.Office365ConnectorCard());
                    break;
                default:
                    // Display a carousel of all the rich card types.
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments.Add(AllCards.CreateAdaptiveCardAttachment());
                    reply.Attachments.Add(AllCards.GetHeroCard());
                    reply.Attachments.Add(AllCards.GetOAuthCard(ConnectionName).ToAttachment());
                    reply.Attachments.Add(AllCards.GetSigninCard().ToAttachment());
                    reply.Attachments.Add(AllCards.GetThumbnailCard());
                    reply.Attachments.Add(AllCards.CreateListCardAttachment());
                    reply.Attachments.Add(AllCards.CollectionsCardAttachment());
                    reply.Attachments.Add(AllCards.Office365ConnectorCard());
                    break;
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see all card."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Loads all card choices.
        /// </summary>
        /// <returns>A list of choices representing the available cards.</returns>
        private IList<Choice> LoadAllCards()
        {
            return new List<Choice>
                {
                    new Choice { Value = "Adaptive Card", Synonyms = new List<string> { "adaptive" } },
                    new Choice { Value = "Hero Card", Synonyms = new List<string> { "hero" } },
                    new Choice { Value = "OAuth Card", Synonyms = new List<string> { "oauth" } },
                    new Choice { Value = "Signin Card", Synonyms = new List<string> { "signin" } },
                    new Choice { Value = "Thumbnail Card", Synonyms = new List<string> { "thumbnail", "thumb" } },
                    new Choice { Value = "List Cards", Synonyms = new List<string> { "list" } },
                    new Choice { Value = "Collections Cards", Synonyms = new List<string> { "collections" } },
                    new Choice { Value = "Connector Cards", Synonyms = new List<string> { "office" } },
                };
        }
    }
}
