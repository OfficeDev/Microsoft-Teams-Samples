// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FetchGroupChatMessagesWithRSC.helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace FetchGroupChatMessagesWithRSC.Dialogs
{
    /// <summary>
    /// Main dialog for handling user interactions and authentication.
    /// </summary>
    public class MainDialog : LogoutDialog
    {
        private readonly ILogger<MainDialog> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="env">The web host environment.</param>
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

        /// <summary>
        /// Prompts the user to login.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        /// <summary>
        /// Handles the login step and processes the token response.
        /// </summary>
        /// <param name="stepContext">The waterfall step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                if (stepContext.Context.Activity.Conversation.ConversationType != "personal")
                {
                    var message = await GetChatHelper.GetGroupChatMessage(stepContext.Context, tokenResponse, stepContext.Context.Activity.Conversation.Id);
                    CreateChatFile(message);
                    string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "chat.txt");
                    long fileSize = new FileInfo(filePath).Length;
                    await GetChatHelper.SendGroupChatMessage(stepContext.Context, fileSize, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], cancellationToken);
                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please type 'getchat' command in the group chat where the bot is added to fetch messages."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        /// <summary>
        /// Creates a chat file with the provided chat messages.
        /// </summary>
        /// <param name="activity">The chat messages collection.</param>
        private void CreateChatFile(List<ChatMessage> activity)
        {
            var fileName = Path.Combine(_env.ContentRootPath, "wwwroot", "chat.txt");
            FileInfo fi = new FileInfo(fileName);

            // Check if file already exists.
            if (fi.Exists)
            {
                if (!string.IsNullOrEmpty(System.IO.File.ReadAllText(fileName)))
                {
                    System.IO.File.WriteAllText(fileName, string.Empty);
                }
                foreach (var chat in activity)
                {
                    if (chat.MessageType.ToString() == "Message")
                    {
                        using (StreamWriter sw = new StreamWriter(fileName, append: true))
                        {
                            sw.WriteLine("from: {0}", chat.From.User != null ? chat.From.User.DisplayName : "bot");
                            sw.WriteLine("text: {0}", chat.Body.Content.ToString());
                            sw.WriteLine("at: {0}", chat.LastModifiedDateTime);
                        }
                    }
                }
            }
        }
    }
}