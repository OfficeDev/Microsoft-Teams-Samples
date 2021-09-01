using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
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
        public QuizFullDialog() : base(nameof(QuizFullDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
                ResumeAfterRunQuiz1Dialog,
                ResumeAfterBeginQuiz2
            }));
            AddDialog(new Quiz1Dialog());
            AddDialog(new Quiz2Dialog());
        }

        private async Task<DialogTurnResult> BeginFormflowAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogQuizDialog);
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