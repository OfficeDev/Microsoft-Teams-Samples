// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class LogoutDialog : ComponentDialog
    {
        protected string ConnectionName { get; }
        public LogoutDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {

                var text = innerDc.Context.Activity.Text;
                if (text != null)
                {
                    text = innerDc.Context.Activity.Text.ToLowerInvariant();

                    // Allow logout anywhere in the command
                    if (text.IndexOf("logout") >= 0)
                    {
                        // The bot adapter encapsulates the authentication processes.
                        var botAdapter = innerDc.Context.Adapter;
                        var userTokenClient = innerDc.Context.TurnState.Get<UserTokenClient>();
                        await userTokenClient.SignOutUserAsync(innerDc.Context.Activity.From.Id, ConnectionName, innerDc.Context.Activity.ChannelId, cancellationToken);
                        await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                    }
                }
            }

            return null;
        }
    }
}
