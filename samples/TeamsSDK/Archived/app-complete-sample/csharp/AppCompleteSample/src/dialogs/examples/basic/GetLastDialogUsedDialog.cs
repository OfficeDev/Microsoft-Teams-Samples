using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteSample.Dialogs
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
                BeginGetLastDialogUsedDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginGetLastDialogUsedDialogAsync(
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
                currentState.LastDialogKey = Strings.LastDialogFetchDiaog;
                await this._conversationState.SetAsync(stepContext.Context, currentState);

                await stepContext.Context.SendActivityAsync(Strings.LastDialogErrorMsg);
            }

            //Set the Last Dialog in Conversation Data
            currentState.LastDialogKey = Strings.LastDialogFetchDiaog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}