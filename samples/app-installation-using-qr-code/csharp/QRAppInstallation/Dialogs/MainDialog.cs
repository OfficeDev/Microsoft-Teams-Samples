// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using QRAppInstallation.Models;

namespace QRAppInstallation.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        public readonly IConfiguration _configuration;
        protected readonly IStatePropertyAccessor<TokenState> _conversationState;

        public MainDialog(IConfiguration configuration, ConversationState conversationState)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;
            this._conversationState = conversationState.CreateProperty<TokenState>(nameof(TokenState));

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
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

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Conversation.ConversationType != "personal")
                {
                    //Set the Last Dialog in Conversation Data
                    var token = await this._conversationState.GetAsync(stepContext.Context, () => new TokenState());
                    token.AccessToken = tokenResponse.Token;
                    await this._conversationState.SetAsync(stepContext.Context, token);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync("Login successfully");
            }

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
                        Text = "Generate QR for app and install",
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
                            Id="generate"
                        },
                    },
                     new AdaptiveSubmitAction
                    {
                        Title = "Install App",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id="install"
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
