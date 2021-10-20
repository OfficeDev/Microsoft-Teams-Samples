// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotWithSharePointFileViewer.helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace BotWithSharePointFileViewer.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;
        public readonly IConfiguration _configuration;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, IWebHostEnvironment env)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _logger = logger;
            _env = env;
            _configuration = configuration;

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
                    GetSharePointFileHelper.GetSharePointFile(stepContext.Context, tokenResponse, stepContext.Context.Activity.Conversation.Id);

                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please type 'getchat' command in the groupchat where the bot is added to fetch messages."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }
    }
}