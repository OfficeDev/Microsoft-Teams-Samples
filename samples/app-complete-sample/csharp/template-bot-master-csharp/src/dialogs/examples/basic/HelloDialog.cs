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

    [Serializable]
    public class HelloDialog : ComponentDialog
    {
        public HelloDialog() : base(nameof(HelloDialog))
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
            await stepContext.Context.SendActivityAsync(Strings.HelloDialogMsg);

            // Begin the Formflow dialog.
            await stepContext.BeginDialogAsync(
                nameof(HelloDialog),
                cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}