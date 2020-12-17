using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Quiz1 Dialog and Child Dialog of Quiz Dialog.
    /// Calling Sequence of this Dialog is :-
    /// Root Dialog --> Quiz Dialog 
    /// Quiz Dialog --> Quiz1 Dialog (Child dialog of Quiz Dialog)
    /// Once this dialog Ends, it will resume in Quiz Dialog
    /// </summary>
    [Serializable]
    public class Quiz1Dialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var message = CreateQuiz(context);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogQuiz1Dialog);

            await context.PostAsync(message);
            context.Wait(this.MessageReceivedAsync);
        }

        #region Create Quiz Message Card
        private IMessageActivity CreateQuiz(IDialogContext context)
        {
            var message = context.MakeMessage();
            var attachment = CreateQuizCard();
            message.Attachments.Add(attachment);
            return message;
        }

        private Attachment CreateQuizCard()
        {
            return new HeroCard
            {
                Title = Strings.Quiz1Question,
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.ImBack, Strings.OptionYes, value: Strings.OptionYes),
                   new CardAction(ActionTypes.ImBack, Strings.OptionNo, value: Strings.OptionNo)
                }
            }.ToAttachment();
        }
        #endregion

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if (result == null)
            {
                throw new InvalidOperationException((nameof(result)) + Strings.NullException);
            }

            var activity = await result;

            if (activity.Text.ToLower() == Strings.OptionYes)
            {
                await context.PostAsync(Strings.QuizAnswerYes);
                context.Done<object>(null);
            }
            else if (activity.Text.ToLower() == Strings.OptionNo)
            {
                await context.PostAsync(Strings.QuizAnswerNo);
                context.Done<object>(null);
            }
        }
    }
}