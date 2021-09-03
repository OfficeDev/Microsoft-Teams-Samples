using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    public class GetLastDialogUsedDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public GetLastDialogUsedDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(GetLastDialogUsedDialog))
        {
            this._conversationState = conversationState;
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
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            if (currentState.LastDialogKey != null)
            {
                await stepContext.Context.SendActivityAsync(Strings.LastDialogPromptMsg + currentState.LastDialogKey);
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