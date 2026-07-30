using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Begin Dialog Class. Main purpose of this class is to notify users that Child dialog has been called 
    /// and its a Basic example to call Child dialog from Root Dialog.
    /// </summary>
    public class BeginDialogExampleDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public BeginDialogExampleDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(BeginDialogExampleDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginBeginDialogExampleDialogAsync,
                ContinueBeginDialogExampleDialogAsync,
            }));
            AddDialog(new HelpDialog(conversationState));
        }

        private async Task<DialogTurnResult> BeginBeginDialogExampleDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogBeginDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(Strings.BeginDialog);

            return await stepContext.BeginDialogAsync(
                        nameof(HelloDialog));
        }

        private async Task<DialogTurnResult> ContinueBeginDialogExampleDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.BeginDialogEnd);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}