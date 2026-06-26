
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Quiz1 Dialog and Child Dialog of Quiz Dialog.
    /// Calling Sequence of this Dialog is :-
    /// Root Dialog --> Quiz Dialog 
    /// Quiz Dialog --> Quiz1 Dialog (Child dialog of Quiz Dialog)
    /// Once this dialog Ends, it will resume in Quiz Dialog
    /// </summary>
    public class Quiz1Dialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        private static List<Choice> Quiz1Options = new List<Choice>()
            {
                new Choice(Strings.OptionYes) { Synonyms = new List<string> { Strings.OptionYes } },
                new Choice(Strings.OptionNo) { Synonyms = new List<string> { Strings.OptionNo } },
            };
        public Quiz1Dialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(Quiz1Dialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginQuizAsync,
                EndQuiz1DialogAsync
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }

        private async Task<DialogTurnResult> BeginQuizAsync(
           WaterfallStepContext stepContext,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogQuiz1Dialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.PromptAsync(
                    nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Strings.Quiz1Question),
                        Choices = Quiz1Options,
                    },
                    cancellationToken);
        }

        private async Task<DialogTurnResult> EndQuiz1DialogAsync(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            var optionSelected = (stepContext.Result as FoundChoice).Value;
            if (optionSelected.ToLower() == Strings.OptionYes)
            {
                await stepContext.Context.SendActivityAsync(Strings.QuizAnswerYes);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (optionSelected.ToLower() == Strings.OptionNo)
            {
                await stepContext.Context.SendActivityAsync(Strings.QuizAnswerNo);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}