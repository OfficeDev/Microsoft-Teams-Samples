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
    /// This is Message Back Dialog Class. Main purpose of this class is to show example of Message Back event
    /// </summary
    public class MessagebackDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public MessagebackDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(MessagebackDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginMessagebackDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginMessagebackDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogMessageBackDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(Strings.MessageBackTitleMsg);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}