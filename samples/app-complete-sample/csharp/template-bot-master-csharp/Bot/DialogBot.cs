using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Bots
{
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;

        public DialogBot(ConversationState conversationState, T dialog)
        {
            _conversationState = conversationState;
            _dialog = dialog;
        }

        protected override async Task OnMessageActivityAsync(
    ITurnContext<IMessageActivity> turnContext,
    CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await _dialog.Run(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}