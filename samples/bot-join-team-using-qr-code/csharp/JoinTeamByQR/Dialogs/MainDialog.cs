// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using JoinTeamByQR.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace JoinTeamByQR.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        public readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, string> _Token;

        public MainDialog(IConfiguration configuration,ConcurrentDictionary<string, string> token)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;
            _Token = token;

            AddDialog(new TokenExchangeOAuthPrompt(
                nameof(TokenExchangeOAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Sign In",
                    Timeout = 1000 * 60 * 1, // User has 5 minutes to login (1000 * 60 * 5)

                    //EndOnInvalidMessage = true
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Method to invoke oauth flow.
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
        }

        // Invoked after success of prompt step async.
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Text!= null)
                {
                    if (stepContext.Context.Activity.Text.ToLower().Trim() == "generate")
                    {
                        
                        //Set the Last Dialog in Conversation Data
                        _Token.AddOrUpdate("Token", tokenResponse.Token, (key, newValue) => tokenResponse.Token);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);

                        return await stepContext.EndDialogAsync();
                    }   
                }                  
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Sample Adaptive card for Meeting Start event.
        /// </summary>
        private Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Generate QR for team",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Generate QR code",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id = "generate"
                        },
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}