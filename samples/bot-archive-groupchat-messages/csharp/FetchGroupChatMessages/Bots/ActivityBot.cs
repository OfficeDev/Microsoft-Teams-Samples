// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FetchGroupChatMessagesWithRSC.Bots
{
    /// <summary>
    /// Activity bot for handling user interactions and file uploads.
    /// </summary>
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _clientFactory;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityBot{T}"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="env">The web host environment.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="conversationState">The conversation state.</param>
        /// <param name="dialog">The dialog instance.</param>
        public ActivityBot(IConfiguration configuration, IWebHostEnvironment env, IHttpClientFactory clientFactory, ConversationState conversationState, T dialog)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _env = env;
            _conversationState = conversationState;
            _dialog = dialog;
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = StripAtMentionText((Activity)turnContext.Activity);
            var userCommand = activity.Text.ToLower().Trim();

            if (userCommand == "getchat" || userCommand == "logout" || userCommand == "login")
            {
                // Run the Dialog with the new message Activity.
                await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Type 'getchat' to get archived messages."), cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when a file consent card activity is received.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user acts on a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            try
            {
                string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "chat.txt");
                long fileSize = new FileInfo(filePath).Length;
                var client = _clientFactory.CreateClient();

                using (var fileStream = File.OpenRead(filePath))
                {
                    var fileContent = new StreamContent(fileStream)
                    {
                        Headers =
                            {
                                ContentLength = fileSize,
                                ContentRange = new ContentRangeHeaderValue(0, fileSize - 1, fileSize)
                            }
                    };
                    await client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, fileContent, cancellationToken);
                }

                await FileUploadCompletedAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }
            catch (Exception e)
            {
                await FileUploadFailedAsync(turnContext, e.ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when a file consent card is declined by the user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user declines a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            JToken context = JObject.FromObject(fileConsentCardResponse.Context);

            var reply = MessageFactory.Text($"Declined. We won't upload file <b>{context["filename"]}</b>.");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handle file upload completion.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user accepts a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task FileUploadCompletedAsync(ITurnContext turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            var downloadCard = new FileInfoCard
            {
                UniqueId = fileConsentCardResponse.UploadInfo.UniqueId,
                FileType = fileConsentCardResponse.UploadInfo.FileType,
            };

            var asAttachment = new Attachment
            {
                Content = downloadCard,
                ContentType = FileInfoCard.ContentType,
                Name = fileConsentCardResponse.UploadInfo.Name,
                ContentUrl = fileConsentCardResponse.UploadInfo.ContentUrl,
            };

            var reply = MessageFactory.Text($"<b>File uploaded.</b> Your file <b>{fileConsentCardResponse.UploadInfo.Name}</b> is ready to download.");
            reply.TextFormat = "xml";
            reply.Attachments = new List<Attachment> { asAttachment };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handle file upload failure.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="error">Error while uploading the file.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task FileUploadFailedAsync(ITurnContext turnContext, string error, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"<b>File upload failed.</b> Error: <pre>{error}</pre>");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Remove mention text from the activity.
        /// </summary>
        /// <param name="activity">The activity containing the mention text.</param>
        /// <returns>The activity with the mention text removed.</returns>
        private Activity StripAtMentionText(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            foreach (var mention in activity.GetMentions())
            {
                if (mention.Mentioned.Id == activity.Recipient.Id)
                {
                    // Bot is in the @mention list.
                    // The below example will strip the bot name out of the message, so you can parse it as if it wasn't included.
                    // Note that the Text object will contain the full bot name, if applicable.
                    if (mention.Text != null)
                    {
                        activity.Text = activity.Text.Replace(mention.Text, "").Trim();
                    }
                }
            }
            return activity;
        }
    }
}