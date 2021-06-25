// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ChangeNotification.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeNotification.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;
        protected readonly ILogger<SubscriptionManager> sLogger;
        protected readonly IConfiguration Config;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, ILogger<SubscriptionManager> Slogger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;
            sLogger = Slogger;
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
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
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
            SubscriptionManager subscriptionManager = new SubscriptionManager(Config, sLogger, token, turnContext);
            await subscriptionManager.InitializeAllSubscription();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken).ConfigureAwait(false);
                await subscriptionManager.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }
    }
}