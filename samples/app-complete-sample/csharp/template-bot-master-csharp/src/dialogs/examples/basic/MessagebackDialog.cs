using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Message Back Dialog Class. Main purpose of this class is to show example of Message Back event
    /// </summary
    public class MessagebackDialog : ComponentDialog
    {
        public MessagebackDialog() : base(nameof(MessagebackDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
            }));
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
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogMessageBackDialog);
            await stepContext.Context.SendActivityAsync(Strings.MessageBackTitleMsg);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}