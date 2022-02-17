// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCompleteAuth.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace AppCompleteAuth.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly string _applicationBaseUrl;
        private readonly ConcurrentDictionary<string, Token> _Token;
        public MainDialog(IConfiguration configuration, ConcurrentDictionary<string, Token> token)
            : base(nameof(MainDialog))
        {
            _Token = token;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new FacebookAuthDialog(configuration["FacebookConnectionName"], _Token));
            AddDialog(new BotSsoAuthDialog(configuration["ConnectionName"], _Token));
            AddDialog(new UsernamePasswordAuthDialog(configuration["ApplicationBaseUrl"]));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Method to invoke auth flow.
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Context.Activity.Text == null || stepContext.Context.Activity.Text.ToLower().Trim() == "usingcredentials")
            {
                return await stepContext.BeginDialogAsync(nameof(UsernamePasswordAuthDialog));
            }
            if (stepContext.Context.Activity.Text.ToLower().Trim() == "sso" || stepContext.Context.Activity.Text.ToLower().Trim() == "logoutsso")
            {
                return await stepContext.BeginDialogAsync(nameof(BotSsoAuthDialog));
            }
            else if (stepContext.Context.Activity.Text.ToLower().Trim() == "facebooklogin" || stepContext.Context.Activity.Text.ToLower().Trim() == "logoutfacebook")
            {
                return await stepContext.BeginDialogAsync(nameof(FacebookAuthDialog));
            }
            else {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetLoginOptionCard()), cancellationToken);

                return await stepContext.EndDialogAsync();
            }
        }

        // Get login option card.
        private static Attachment GetLoginOptionCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Login options",
                Text = "Select a login option",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack,title:"AAD SSO authentication", value: "sso", text:"sso", displayText:"AAD SSO authentication"),
                    new CardAction(ActionTypes.MessageBack,title:"Facebook login (OAuth 2)", value: "facebooklogin", text:"facebooklogin", displayText:"Facebook login (OAuth 2)"),
                    new CardAction(ActionTypes.MessageBack,title:"User Id/password login", value: "usingcredentials", text:"usingcredentials", displayText:"User Id/password login"),
                }
            };

            return heroCard.ToAttachment();
        }

        // Get sign in card.
        private Attachment GetPopUpSignInCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Sign in card",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Signin, "Sign in", value: _applicationBaseUrl + "/popUpSignin?from=bot&height=535&width=600"),
                }
            };

            return heroCard.ToAttachment();
        }
    }
}