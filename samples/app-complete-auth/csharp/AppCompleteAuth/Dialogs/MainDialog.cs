// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AppCompleteAuth.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace AppCompleteAuth.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        private readonly string _applicationBaseUrl;
        private readonly ConcurrentDictionary<string, TokenState> _Token;

        public MainDialog(IConfiguration configuration, ConcurrentDictionary<string, TokenState> token)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
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
            AddDialog(new FacebookAuthDialog(configuration["FacebookConnectionName"]));
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
            if (stepContext.Context.Activity.Text != null)
            {
                if (stepContext.Context.Activity.Text.ToLower().Trim() == "sso")
                {
                    return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
                }
                else if (stepContext.Context.Activity.Text.ToLower().Trim() == "usingcredentials")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetPopUpSignInCard()), cancellationToken);
                    return await stepContext.EndDialogAsync();
                }
                else if (stepContext.Context.Activity.Text.ToLower().Trim() == "otheridentityprovider")
                {
                    return await stepContext.BeginDialogAsync(nameof(FacebookAuthDialog));
                }
            }
            return await stepContext.EndDialogAsync();
        }

        // Invoked after success of prompt step async.
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);

                // Pull in the data from the Microsoft Graph.
                var client = new SimpleGraphClient(tokenResponse.Token);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); you job title is: {title}; your photo is: ");
                var photo = await client.GetPhotoAsync();
                var cardImage = new CardImage(photo);
                var card = new ThumbnailCard(images: new List<CardImage>() { cardImage });
                var reply = MessageFactory.Attachment(card.ToAttachment());
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.EndDialogAsync();                  
            }

            return await stepContext.EndDialogAsync();
        }

        private Attachment GetPopUpSignInCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Sign in card",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Signin, "Sign in", value: _applicationBaseUrl + "/popUpSignin?height=400&width=400"),
                }
            };

            return heroCard.ToAttachment();
        }
    }
}