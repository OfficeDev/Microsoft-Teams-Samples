// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace AppCompleteAuth.Dialogs
{
    /// <summary>
    /// LogoutDialog class to handle user logout functionality.
    /// </summary>
    public class LogoutDialog : ComponentDialog
    {
        public LogoutDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        protected string ConnectionName { get; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the parent's dialog stack.
        /// </summary>
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        /// <summary>
        /// Called when the dialog is continued, where it is the active dialog and the user replies with a new activity.
        /// </summary>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        /// <summary>
        /// Interrupts the dialog to handle logout commands.
        /// </summary>
        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                // Allow logout anywhere in the command
                if (text.Contains("logout"))
                {
                    // The bot adapter encapsulates the authentication processes.
                    var userTokenClient = innerDc.Context.TurnState.Get<UserTokenClient>();
                    await userTokenClient.SignOutUserAsync(innerDc.Context.Activity.From.Id, ConnectionName, innerDc.Context.Activity.ChannelId, cancellationToken).ConfigureAwait(false);
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                    return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}