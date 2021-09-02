using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Card Dialog Class. Main purpose of this dialog class is to post different types of Cards example like Hero Card, Thumbnail Card etc.
    /// </summary>
    public class DisplayCardsDialog : ComponentDialog
    {
        private static List<Choice> options = new List<Choice>()
            {
                new Choice(Strings.DisplayCardHeroCard) { Synonyms = new List<string> { Strings.DisplayCardHeroCard } },
                new Choice(Strings.DisplayCardThumbnailCard) { Synonyms = new List<string> { Strings.DisplayCardThumbnailCard } },
                new Choice(Strings.DisplayCardO365ConnectorCardDefault) { Synonyms = new List<string> { Strings.DisplayCardO365ConnectorCardDefault } },
                new Choice(Strings.DisplayCardO365ConnectorCard2) { Synonyms = new List<string> { Strings.DisplayCardO365ConnectorCard2 } },
                new Choice(Strings.DisplayCardO365ConnectorCard3) { Synonyms = new List<string> { Strings.DisplayCardO365ConnectorCard3 } },
                new Choice(Strings.DisplayCardO365ConnectorActionableCardDefault) { Synonyms = new List<string> { Strings.DisplayCardO365ConnectorActionableCardDefault } },
                new Choice(Strings.DisplayCardO365ConnectorActionableCard2) { Synonyms = new List<string> { Strings.DisplayCardO365ConnectorActionableCard2 } },
                 new Choice(Strings.DisplayCardAdaptiveCard) { Synonyms = new List<string> { Strings.DisplayCardAdaptiveCard } }
            };

        public DisplayCardsDialog() : base(nameof(DisplayCardsDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginDisplayCardsAsync,
                GetCardNameAsync,
                ResumeAfterOptionDialogAsync
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new HeroCardDialog());
            AddDialog(new ThumbnailcardDialog());
            AddDialog(new AdaptiveCardDialog());
            AddDialog(new O365ConnectorCardActionsDialog());
            AddDialog(new O365ConnectorCardDialog());
        }
        private async Task<DialogTurnResult> BeginDisplayCardsAsync(
           WaterfallStepContext stepContext,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogDisplayCardsDialog);

            await stepContext.Context.SendActivityAsync(Strings.DisplayCardMsgTitle);
            return await stepContext.PromptAsync(
                   nameof(ChoicePrompt),
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text(Strings.DisplayCardPromptText),
                       Choices = options,
                       RetryPrompt = MessageFactory.Text(Strings.DisplayCardPromptText)
                   },
                   cancellationToken);
        }

        private async Task<DialogTurnResult> GetCardNameAsync(WaterfallStepContext stepContext,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            var selectedCard = (stepContext.Result as FoundChoice).Value;
            if (selectedCard.Equals(Strings.DisplayCardHeroCard))
            {
                return await stepContext.BeginDialogAsync(
                        nameof(HeroCardDialog));
            }
            else if(selectedCard.Equals(Strings.DisplayCardThumbnailCard))
            {
                return await stepContext.BeginDialogAsync(
                         nameof(ThumbnailcardDialog));
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCardDefault)){
                return await stepContext.BeginDialogAsync(
                         nameof(O365ConnectorCardDialog));
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCard2))
            {
                return await stepContext.BeginDialogAsync(
                         nameof(O365ConnectorCardDialog));
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCard3))
            {
                return await stepContext.BeginDialogAsync(
                         nameof(O365ConnectorCardDialog));
            }
            else if(selectedCard.Equals(Strings.DisplayCardO365ConnectorActionableCardDefault))
            {
                return await stepContext.BeginDialogAsync(
                         nameof(O365ConnectorCardActionsDialog));
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorActionableCard2)){
                return await stepContext.BeginDialogAsync(
                         nameof(O365ConnectorCardActionsDialog));
            }
            else if (selectedCard.Equals(Strings.DisplayCardAdaptiveCard))
            {
                return await stepContext.BeginDialogAsync(
                         nameof(AdaptiveCardDialog));
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ResumeAfterOptionDialogAsync(WaterfallStepContext stepContext,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(Strings.DisplayCardErrorMsg + ex.Message);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}