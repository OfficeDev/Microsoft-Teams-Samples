using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Begin Dialog Class. Main purpose of this class is to notify users that Child dialog has been called 
    /// and its a Basic example to call Child dialog from Root Dialog.
    /// </summary>
    public class BeginDialogExampleDialog : ComponentDialog
    {
        public BeginDialogExampleDialog() : base(nameof(BeginDialogExampleDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
                SaveResultAsync,
            }));
            AddDialog(new HelpDialog());
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
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogBeginDialog);
            await stepContext.Context.SendActivityAsync(Strings.BeginDialog);

            return await stepContext.BeginDialogAsync(
                        nameof(HelloDialog));
        }

        private async Task<DialogTurnResult> SaveResultAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.BeginDialogEnd);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}