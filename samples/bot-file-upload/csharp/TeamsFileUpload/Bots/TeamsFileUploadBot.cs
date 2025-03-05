// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsFileUploadBot : TeamsActivityHandler
    {
        // Private fields for the HttpClient and bot credentials.
        private readonly IHttpClientFactory _clientFactory;
        private static HttpClient _client;
        private static string microsoftAppId;
        private static string microsoftAppPassword;

        // Constructor that initializes the bot with the configuration and client factory.
        public TeamsFileUploadBot(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;

            // Initialize static variables only once.
            if (_client == null)
            {
                _client = _clientFactory.CreateClient();
            }

            microsoftAppId = configuration["MicrosoftAppId"];
            microsoftAppPassword = configuration["MicrosoftAppPassword"];
        }

        // Handles incoming message activities and processes file uploads or inline images.
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            bool messageWithFileDownloadInfo = turnContext.Activity.Attachments?[0].ContentType == FileDownloadInfo.ContentType;

            if (messageWithFileDownloadInfo)
            {
                // Process file download.
                var file = turnContext.Activity.Attachments[0];
                var fileDownload = JObject.FromObject(file.Content).ToObject<FileDownloadInfo>();
                string filePath = Path.Combine("Files", file.Name);

                var response = await _client.GetAsync(fileDownload.DownloadUrl);
                await SaveFileAsync(filePath, response);

                var reply = MessageFactory.Text($"<b>{file.Name}</b> received and saved.");
                reply.TextFormat = "xml";
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (turnContext.Activity.Attachments?[0].ContentType.Contains("image/*") == true)
            {
                // Process inline image.
                await ProcessInlineImage(turnContext, cancellationToken);
            }
            else
            {
                // Send a default file.
                string filename = "teams-logo.png";
                string filePath = Path.Combine("Files", filename);
                long fileSize = new FileInfo(filePath).Length;
                await SendFileCardAsync(turnContext, filename, fileSize, cancellationToken);
            }
        }

        // Helper method to save file to disk.
        private async Task SaveFileAsync(string filePath, HttpResponseMessage response)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fileStream);
            }
        }

        // Processes inline image attachments and saves them.
        private async Task ProcessInlineImage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var attachment = turnContext.Activity.Attachments[0];
            var token = await new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword).GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var responseMessage = await _client.GetAsync(attachment.ContentUrl);

            // Save the inline image to Files directory.
            var filePath = Path.Combine("Files", "ImageFromUser.png");
            await SaveFileAsync(filePath, responseMessage);

            // Create reply with image.
            var reply = MessageFactory.Text($"Attachment of {attachment.ContentType} type and size of {responseMessage.Content.Headers.ContentLength} bytes received.");
            reply.Attachments = new List<Attachment> { GetInlineAttachment() };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Creates an inline attachment for the image that was received.
        private static Attachment GetInlineAttachment()
        {
            var imagePath = Path.Combine("Files", "ImageFromUser.png");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            return new Attachment
            {
                Name = @"ImageFromUser.png",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{imageData}",
            };
        }

        // Sends a file consent card to the user.
        private async Task SendFileCardAsync(ITurnContext turnContext, string filename, long filesize, CancellationToken cancellationToken)
        {
            var consentContext = new Dictionary<string, string>
            {
                { "filename", filename },
            };

            var fileCard = new FileConsentCard
            {
                Description = "This is the file I want to send you",
                SizeInBytes = filesize,
                AcceptContext = consentContext,
                DeclineContext = consentContext,
            };

            var asAttachment = new Attachment
            {
                Content = fileCard,
                ContentType = FileConsentCard.ContentType,
                Name = filename,
            };

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments = new List<Attachment> { asAttachment };
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        // Handles file consent acceptance.
        protected override async Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            try
            {
                JToken context = JObject.FromObject(fileConsentCardResponse.Context);
                string filePath = Path.Combine("Files", context["filename"].ToString());
                long fileSize = new FileInfo(filePath).Length;

                using (var fileStream = File.OpenRead(filePath))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentLength = fileSize;
                    fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileSize - 1, fileSize);
                    await _client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, fileContent, cancellationToken);
                }

                await FileUploadCompletedAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }
            catch (Exception e)
            {
                await FileUploadFailedAsync(turnContext, e.ToString(), cancellationToken);
            }
        }

        // Handles file consent decline.
        protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            JToken context = JObject.FromObject(fileConsentCardResponse.Context);
            var reply = MessageFactory.Text($"Declined. We won't upload file <b>{context["filename"]}</b>.");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Sends a reply indicating that file upload is completed.
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

            var reply = MessageFactory.Text($"<b>File uploaded.</b> Your file <b>{fileConsentCardResponse.UploadInfo.Name}</b> is ready to download");
            reply.TextFormat = "xml";
            reply.Attachments = new List<Attachment> { asAttachment };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Sends a reply indicating that file upload has failed.
        private async Task FileUploadFailedAsync(ITurnContext turnContext, string error, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"<b>File upload failed.</b> Error: <pre>{error}</pre>");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
