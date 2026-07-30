// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace JoinTeamByQR.Dialogs
{
    public class TokenExchangeOAuthPrompt : OAuthPrompt
    {
        private readonly OAuthPromptSettings _settings;

        public TokenExchangeOAuthPrompt(string dialogId, OAuthPromptSettings settings)
            : base(dialogId, settings)
        {
            _settings = settings;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // If the token was successfully exchanged already, it should be cached in TurnState along with the TokenExchangeInvokeRequest
            var cachedTokenResponse = dc.Context.TurnState.Get<TokenResponse>(nameof(TokenResponse));

            if (cachedTokenResponse != null)
            {
                var tokenExchangeRequest = dc.Context.TurnState.Get<TokenExchangeInvokeRequest>(nameof(TokenExchangeInvokeRequest));
                if (tokenExchangeRequest == null)
                {
                    throw new InvalidOperationException("TokenResponse is present in TurnState, but TokenExchangeInvokeRequest is missing.");
                }

                var exchangeResponse = new TokenExchangeInvokeResponse
                {
                    Id = tokenExchangeRequest.Id,
                    ConnectionName = _settings.ConnectionName,
                };

                await dc.Context.SendActivityAsync(
                    new Activity
                    {
                        Type = ActivityTypesEx.InvokeResponse,
                        Value = new InvokeResponse
                        {
                            Status = (int)HttpStatusCode.OK,
                            Body = exchangeResponse,
                        },
                    }, cancellationToken);

                return await dc.EndDialogAsync(new TokenResponse
                {
                    ChannelId = cachedTokenResponse.ChannelId,
                    ConnectionName = cachedTokenResponse.ConnectionName,
                    Token = cachedTokenResponse.Token,
                }, cancellationToken);
            }

            return await base.ContinueDialogAsync(dc, cancellationToken);
        }
    }
}