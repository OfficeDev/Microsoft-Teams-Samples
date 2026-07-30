// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ChangeNotification.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ChangeNotification.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;
        protected readonly ILogger<SubscriptionManager> SubscriptionLogger;
        protected readonly IConfiguration Config;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, ILogger<SubscriptionManager> subscriptionLogger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;
            SubscriptionLogger = subscriptionLogger;
            Config = configuration;
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                await ExecuteAsync(tokenResponse.Token, stepContext.Context, cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Type anything to subscribe user presence") }, cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        protected async Task ExecuteAsync(string token, ITurnContext turnContext, CancellationToken stoppingToken)
        {
            TokenStore.Token = token;
            var userId = turnContext.Activity.From.AadObjectId;
            var subscriptionManager = new SubscriptionManager(Config, SubscriptionLogger, token, userId);
            await subscriptionManager.InitializeAllSubscription();
        }
    }
}