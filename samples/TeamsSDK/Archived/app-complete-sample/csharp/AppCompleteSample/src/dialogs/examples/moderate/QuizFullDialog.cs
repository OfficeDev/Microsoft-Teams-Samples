using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Quiz Dialog class and its a example for below scenarios
    ///     1. Call Main Dialog from Root Dialog ( Root Dialog -> Quiz Dialog)
    ///     2. Call Dialog Within Dialog ( Quiz Dialog -> Quiz1 Dialog)
    ///     3. Call Another Dialog after end of Child Dialog ( Quiz1 Dialog --> Quiz2 Dialog, Quiz2 only be called when Quiz1 Completed, its not child dialog of Quiz1 Dialog)
    ///     4. Once Quiz 2 is Done -> it will Resume Quiz Dialog
    ///     5. Once Quiz Dialog Ends -> it will ResumeRoot Dialog
    /// </summary>
    public class QuizFullDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public QuizFullDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(QuizFullDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginQuizFullDialogAsync,
                ResumeAfterRunQuiz1Dialog,
                ResumeAfterBeginQuiz2
            }));
            AddDialog(new Quiz1Dialog(conversationState));
            AddDialog(new Quiz2Dialog(conversationState));
        }

        private async Task<DialogTurnResult> BeginQuizFullDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogQuizDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(Strings.QuizDialogBeginTitle);

            return await stepContext.BeginDialogAsync(
                        nameof(Quiz1Dialog));
        }

        private async Task<DialogTurnResult> ResumeAfterRunQuiz1Dialog(WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.Quiz1ThanksTitleMsg);
            return await stepContext.BeginDialogAsync(
                        nameof(Quiz2Dialog));
        }

        private async Task<DialogTurnResult> ResumeAfterBeginQuiz2(WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.QuizThanksTitleMsg);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}