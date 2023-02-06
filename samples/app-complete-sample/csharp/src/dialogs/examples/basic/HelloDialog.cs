using Microsoft.Bot.Builder.Dialogs;
using template_bot_master_csharp;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Begin Dialog Class. Main purpose of this class is to notify users that Child dialog has been called 
    /// and its a Basic example to call Child dialog from Root Dialog.
    /// </summary>
    public class HelloDialog : ComponentDialog
    {
        public HelloDialog() : base(nameof(HelloDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginHelloDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginHelloDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.HelloDialogMsg);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}