// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace ChangeNotification.Dialogs
{
    public class LogoutDialog : ComponentDialog
    {
        public LogoutDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        protected string ConnectionName { get; }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                if (text.Contains("logout"))
                {
                    var userTokenClient = innerDc.Context.TurnState.Get<UserTokenClient>();
                    await userTokenClient.SignOutUserAsync(innerDc.Context.Activity.From.Id, ConnectionName, innerDc.Context.Activity.ChannelId, cancellationToken);
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                    return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}
