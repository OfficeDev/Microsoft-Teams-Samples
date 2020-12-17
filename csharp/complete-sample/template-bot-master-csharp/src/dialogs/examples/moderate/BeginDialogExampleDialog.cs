using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Begin Dialog Class. Main purpose of this class is to notify users that Child dialog has been called 
    /// and its a Basic example to call Child dialog from Root Dialog.
    /// </summary>

    [Serializable]
    public class BeginDialogExampleDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogBeginDialog);
            await context.PostAsync(Strings.BeginDialog);

            context.Call(new HelloDialog(), ResumeAfterHelloDialog);
        }

        private async Task ResumeAfterHelloDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync(Strings.BeginDialogEnd);
            context.Done<object>(null);
        }
    }
}