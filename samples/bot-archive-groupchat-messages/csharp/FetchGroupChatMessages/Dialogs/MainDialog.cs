// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

namespace FetchGroupChatMessagesWithRSC.Dialogs
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
                    var message = await GetChatHelper.GetGroupChatMessage(stepContext.Context, tokenResponse, stepContext.Context.Activity.Conversation.Id);
                    createChatFile(message);
                    string filePath = Path.Combine(_env.ContentRootPath, $".\\wwwroot\\chat.txt");
                    long fileSize = new FileInfo(filePath).Length;
                    await GetChatHelper.SendGroupChatMessage(stepContext.Context, fileSize, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"] ,cancellationToken);
                    return await stepContext.EndDialogAsync();
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please type 'getchat' command in the groupchat where the bot is added to fetch messages."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        private void createChatFile(IChatMessagesCollectionPage activity)
        {
            var fileName = Path.Combine(_env.ContentRootPath, $".\\wwwroot\\chat.txt");
            FileInfo fi = new FileInfo(fileName);

            // Check if file already exists.               
            if (fi.Exists)
            {
                if (!string.IsNullOrEmpty(System.IO.File.ReadAllText(fileName)))
                {
                    System.IO.File.WriteAllText(fileName, string.Empty);
                }
                foreach (var chat in activity) {
                    if(chat.MessageType.ToString() == "Message")
                    {
                        using( StreamWriter sw = new StreamWriter(fileName, append: true))
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