using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
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
    [Serializable]
    public class QuizFullDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogQuizDialog);

            await context.PostAsync(Strings.QuizDialogBeginTitle);
            context.Call(new Quiz1Dialog(), ResumeAfterRunQuiz1Dialog);
        }

        private async Task ResumeAfterRunQuiz1Dialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync(Strings.Quiz1ThanksTitleMsg);
            context.Call(new Quiz2Dialog(), this.ResumeAfterBeginQuiz2);
        }

        private async Task ResumeAfterBeginQuiz2(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync(Strings.QuizThanksTitleMsg);
            context.Done<object>(null);
        }
    }
}