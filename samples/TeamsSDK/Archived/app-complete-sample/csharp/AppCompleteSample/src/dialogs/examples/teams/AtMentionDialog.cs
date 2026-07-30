using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AppCompleteSample;
using AppCompleteSample.src.dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is At Mention Dialog Class. Main purpose of this dialog class is to Atmention (@mention)
    /// the user who has started the conversation.
    /// we can pass ChannelAccount object as first parameter of AddMentionToText method to @mention any user.
    /// e.g. ChannelAccount userInformation = new ChannelAccount("29:1TPHVQrnqOI3_FZbeY32VvlBwo1trPhN96SiKYP3av-QCKYGlLBApj-w9fgoI9SCUz4bEmzo9tVlSQdHzgoSzeQ", "Ashish");
    /// </summary>
    public class AtMentionDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public AtMentionDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(AtMentionDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginAtMentionDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginAtMentionDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            var message = stepContext.Context.Activity;
            Activity replyActivity = message as Activity;
            replyActivity.GetMentions();
            await stepContext.Context.SendActivityAsync(replyActivity);

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogAtMentionDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}