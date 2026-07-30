using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteSample.Bots
{
    /// <summary>
    /// Extension methods for running dialogs.
    /// </summary>
    public static class DialogExtensions
    {
        /// <summary>
        /// Runs the dialog with the provided turn context and state accessor.
        /// </summary>
        /// <param name="dialog">The dialog to run.</param>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="accessor">The state property accessor for dialog state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task Run(
            this Dialog dialog,
            ITurnContext turnContext,
            IStatePropertyAccessor<DialogState> accessor,
            CancellationToken cancellationToken)
        {
            var dialogSet = new DialogSet(accessor);
            dialogSet.Add(dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            // Handle 'cancel' interruption
            if (turnContext.Activity.Text.Equals("cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                var reply = MessageFactory.Text("Ok, restarting conversation.");
                await turnContext.SendActivityAsync(reply, cancellationToken);
                await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            if (results.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            }
        }
    }
}