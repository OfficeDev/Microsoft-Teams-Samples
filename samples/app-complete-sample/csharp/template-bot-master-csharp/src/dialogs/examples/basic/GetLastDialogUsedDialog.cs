using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    public class GetLastDialogUsedDialog : ComponentDialog
    {
        public GetLastDialogUsedDialog() : base(nameof(GetLastDialogUsedDialog))
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
            string dialogName = string.Empty;

            if (stepContext.State.TryGetValue(Strings.LastDialogKey, out dialogName))
            {
                await stepContext.Context.SendActivityAsync(Strings.LastDialogPromptMsg + dialogName);
            }
            else
            {
                //Set the Last Dialog in Conversation Data
                //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchDiaog);

                await stepContext.Context.SendActivityAsync(Strings.LastDialogErrorMsg);
            }

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchDiaog);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}