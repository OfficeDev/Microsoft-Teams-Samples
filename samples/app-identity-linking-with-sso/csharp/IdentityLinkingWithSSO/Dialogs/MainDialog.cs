// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityLinkingWithSSO.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace IdentityLinkingWithSSO.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly string _applicationBaseUrl;
        private readonly ConcurrentDictionary<string, Token> _Token;
        private readonly ConcurrentDictionary<string, List<UserMapData>> mappingData;
        public MainDialog(IConfiguration configuration, ConcurrentDictionary<string, Token> token, ConcurrentDictionary<string, List<UserMapData>> data)
            : base(nameof(MainDialog))
        {
            _Token = token;
            mappingData = data;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new FacebookAuthDialog(configuration["FacebookConnectionName"], mappingData));
            AddDialog(new GoogleAuthDialog(configuration["GoogleConnectionName"], mappingData));
            AddDialog(new BotSsoAuthDialog(configuration["ConnectionName"], _Token, mappingData));
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
                return await stepContext.BeginDialogAsync(nameof(BotSsoAuthDialog));
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