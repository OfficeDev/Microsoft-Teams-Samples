// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Main dialog for handling user authentication and displaying token information.
    /// </summary>
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _logger = logger;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                    EndOnInvalidMessage = true
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                    PromptStepAsync,
                    LoginStepAsync,
                    DisplayTokenPhase1Async,
                    DisplayTokenPhase2Async,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Prompts the user to sign in.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        /// <summary>
        /// Handles the login step and retrieves the user's token.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                var client = new SimpleGraphClient(tokenResponse.Token);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ? me.JobTitle : "Unknown";

                await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); your job title is: {title}");

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful, please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Displays a thank you message and prompts the user to view their token.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Displays the user's token.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token: {tokenResponse.Token}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
