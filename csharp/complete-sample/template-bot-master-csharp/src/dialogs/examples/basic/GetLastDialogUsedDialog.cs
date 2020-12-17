using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is get Last Dialog Class. Main purpose of this class is to set the Last Active dialog information
    /// </summary>

    [Serializable]
    public class GetLastDialogUsedDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string dialogName = string.Empty;

            if (context.UserData.TryGetValue(Strings.LastDialogKey, out dialogName))
            {
                await context.PostAsync(Strings.LastDialogPromptMsg + dialogName);
            }
            else
            {
                //Set the Last Dialog in Conversation Data
                context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchDiaog);

                await context.PostAsync(Strings.LastDialogErrorMsg);
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchDiaog);

            context.Done<object>(null);
        }
    }
}