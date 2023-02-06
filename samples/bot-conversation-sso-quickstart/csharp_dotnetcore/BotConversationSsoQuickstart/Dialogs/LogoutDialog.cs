// <copyright file="LogoutDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class LogoutDialog : ComponentDialog
    {
        public LogoutDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        /// <summary>
        /// Configured connection name in Azure Bot service.
        /// </summary>
        protected string ConnectionName { get; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the parent's dialog stack.
        /// </summary>
        /// <param name="innerDc">The inner DialogContext for the current turn of conversation.</param>
        /// <param name="options">Initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(
            DialogContext innerDc,
            object options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the user replies with a new activity.
        /// </summary>
        /// <param name="innerDc">The inner DialogContext for the current turn of conversation.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        /// <summary>
        /// Called when the dialog is interrupted, where it is the active dialog and the user replies with a new activity.
        /// </summary>
        /// <param name="innerDc">The inner DialogContext for the current turn of conversation.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InterruptAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                // Allow logout anywhere in the command
                if (text.IndexOf("logout") >= 0)
                {
                    // The bot adapter encapsulates the authentication processes.
                    var botAdapter = (BotFrameworkAdapter)innerDc.Context.Adapter;
                    await botAdapter.SignOutUserAsync(innerDc.Context, ConnectionName, null, cancellationToken);
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);

                    return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}