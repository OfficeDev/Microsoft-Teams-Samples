using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Message Back Dialog Class. Main purpose of this class is to show example of Message Back event
    /// </summary>

    [Serializable]
    public class MessagebackDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogMessageBackDialog);

            await context.PostAsync(Strings.MessageBackTitleMsg);

            context.Done<object>(null);
        }
    }
}