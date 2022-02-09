// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace AppCompleteAuth.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly string _applicationBaseUrl;

        public MainDialog(IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new FacebookAuthDialog(configuration["FacebookConnectionName"]));
            AddDialog(new BotSsoAuthDialog(configuration["ConnectionName"]));
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
            if (stepContext.Context.Activity.Text != null)
            {
                if (stepContext.Context.Activity.Text.ToLower().Trim() == "sso" || stepContext.Context.Activity.Text.ToLower().Trim() == "logoutsso")
                {
                    return await stepContext.BeginDialogAsync(nameof(BotSsoAuthDialog));
                }
                else if (stepContext.Context.Activity.Text.ToLower().Trim() == "usingcredentials")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetPopUpSignInCard()), cancellationToken);
                    
                    return await stepContext.EndDialogAsync();
                }
                else if (stepContext.Context.Activity.Text.ToLower().Trim() == "facebooklogin" || stepContext.Context.Activity.Text.ToLower().Trim() == "logoutfacebook")
                {
                    return await stepContext.BeginDialogAsync(nameof(FacebookAuthDialog));
                }
            }
            return await stepContext.EndDialogAsync();
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