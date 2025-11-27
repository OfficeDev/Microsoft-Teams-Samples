// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Api;
using System.Net.Http.Headers;
using FileConsentActivity = Microsoft.Teams.Api.Activities.Invokes.FileConsentActivity;
using FileConsentCardResponse = Microsoft.Teams.Api.FileConsentCardResponse;
using FileUploadInfo = Microsoft.Teams.Api.FileUploadInfo;
using FileConsentCard = TeamsFileUpload.Models.FileConsentCard;
using FileDownloadInfo = TeamsFileUpload.Models.FileDownloadInfo;
using FileInfoCard = TeamsFileUpload.Models.FileInfoCard;

namespace TeamsFileUpload.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _filesPath = Path.Combine(Environment.CurrentDirectory, "Files");

        /// <summary>
        /// Initializes a new instance of the Controller class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory for making HTTP requests.</param>
        public Controller(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            
            // Ensure Files directory exists
            if (!Directory.Exists(_filesPath))
            {
                Directory.CreateDirectory(_filesPath);
            }
        }

        /// <summary>
        /// Handles incoming messages from Teams users.
        /// Processes file attachments (downloads and inline images) or sends a file consent card if no attachment is present.
        /// </summary>
        /// <param name="activity">The message activity received from Teams.</param>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="log">Logger for tracking message processing.</param>
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");

            // Check if message contains actual file attachments (not text/html which is just message content)
            bool hasFileAttachment = activity.Attachments != null && 
                                     activity.Attachments.Count > 0 && 
                                     activity.Attachments[0].ContentType?.Value != "text/html";

            if (hasFileAttachment)
            {
                var attachment = activity.Attachments[0];
                var contentTypeValue = attachment.ContentType?.Value ?? attachment.ContentType?.ToString() ?? "";

                log.Info($"Received attachment with ContentType: {contentTypeValue}");

                // Handle file downloads (Teams sends files with this content type)
                if (contentTypeValue == "application/vnd.microsoft.teams.file.download.info")
                {
                    await ProcessFileDownload(client, attachment, log);
                }
                // Handle inline images
                else if (contentTypeValue.StartsWith("image/"))
                {
                    await ProcessInlineImage(activity, client, attachment, log);
                }
                else
                {
                    log.Info($"Unsupported attachment type: {contentTypeValue}");
                    await client.Send($"File attachment received but type '{contentTypeValue}' not supported for processing.");
                }
            }
            else
            {
                // Send a file consent card to upload a file
                await SendFileConsentCard(client, "teams-logo.png", log);
            }
        }

        /// <summary>
        /// Handles file consent card responses from users.
        /// Routes to appropriate handler based on whether user accepted or declined the file upload.
        /// </summary>
        /// <param name="activity">The file consent activity containing user's response.</param>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="log">Logger for tracking consent processing.</param>
        [FileConsent]
        public async Task OnFileConsent([Context] FileConsentActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("File consent invoke received");

            var fileConsentResponse = activity.Value;

            if (fileConsentResponse?.Action == "accept")
            {
                await OnFileConsentAccept(fileConsentResponse, client, log);
            }
            else if (fileConsentResponse?.Action == "decline")
            {
                await OnFileConsentDecline(fileConsentResponse, client, log);
            }
        }

        /// <summary>
        /// Processes inline image attachments received in Teams messages.
        /// Downloads the image, saves it locally, and sends it back as a base64-encoded inline attachment.
        /// </summary>
        /// <param name="activity">The message activity containing the image.</param>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="attachment">The image attachment to process.</param>
        /// <param name="log">Logger for tracking image processing.</param>
        private async Task ProcessInlineImage(MessageActivity activity, IContext.Client client, Attachment attachment, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                
                // Download the image from Teams
                var response = await httpClient.GetAsync(attachment.ContentUrl);
                response.EnsureSuccessStatusCode();

                // Save the image locally
                var fileName = $"ImageFromUser_{DateTime.Now.Ticks}.png";
                var filePath = Path.Combine(_filesPath, fileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                log.Info($"Saved image to {filePath}");

                // Read the saved image and send it back as inline attachment
                var imageData = Convert.ToBase64String(File.ReadAllBytes(filePath));
                var inlineAttachment = new Attachment
                {
                    Name = fileName,
                    ContentType = new ContentType("image/png"),
                    ContentUrl = $"data:image/png;base64,{imageData}"
                };

                var replyMessage = new MessageActivity($"Received and saved your image. File size: {response.Content.Headers.ContentLength} bytes");
                replyMessage.Attachments = new List<Attachment> { inlineAttachment };
                await client.Send(replyMessage);
            }
            catch (Exception ex)
            {
                log.Error($"Error processing inline image: {ex.Message}");
                await client.Send($"Error processing image: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes file download attachments received in Teams messages.
        /// Downloads the file from Teams and saves it to the local Files directory.
        /// </summary>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="attachment">The file attachment containing download information.</param>
        /// <param name="log">Logger for tracking file download processing.</param>
        private async Task ProcessFileDownload(IContext.Client client, Attachment attachment, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var fileDownloadInfo = System.Text.Json.JsonSerializer.Deserialize<FileDownloadInfo>(
                    System.Text.Json.JsonSerializer.Serialize(attachment.Content));

                if (fileDownloadInfo != null)
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(fileDownloadInfo.DownloadUrl);
                    response.EnsureSuccessStatusCode();

                    var fileName = attachment.Name ?? $"download_{DateTime.Now.Ticks}";
                    var filePath = Path.Combine(_filesPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    log.Info($"Downloaded file to {filePath}");
                    await client.Send($"File <b>{fileName}</b> downloaded successfully!");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error downloading file: {ex.Message}");
                await client.Send($"Error downloading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a file consent card to the user requesting permission to upload a file.
        /// Creates a sample file if it doesn't exist in the Files directory.
        /// </summary>
        /// <param name="client">The Teams client for sending the consent card.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="log">Logger for tracking consent card operations.</param>
        private async Task SendFileConsentCard(IContext.Client client, string fileName, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_filesPath, fileName);
                
                // Create a sample file if it doesn't exist
                if (!File.Exists(filePath))
                {
                    await File.WriteAllTextAsync(filePath, "Sample file content for Teams file upload demo.");
                }

                var fileInfo = new FileInfo(filePath);
                var fileSize = fileInfo.Length;

                var fileConsentCard = new FileConsentCard
                {
                    Name = fileName,
                    Description = "This is the file I want to send you",
                    SizeInBytes = fileSize,
                    AcceptContext = new { fileName = fileName },
                    DeclineContext = new { fileName = fileName }
                };

                var attachment = new Attachment
                {
                    ContentType = new ContentType(FileConsentCard.ContentType),
                    Name = fileName,
                    Content = fileConsentCard
                };

                log.Info($"Sending file consent card for {fileName}");
                var message = new MessageActivity("Please accept the file");
                message.Attachments = new List<Attachment> { attachment };
                await client.Send(message);
            }
            catch (Exception ex)
            {
                log.Error($"Error sending file consent card: {ex.Message}");
                await client.Send($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the user accepting the file upload consent.
        /// Uploads the file to Teams/OneDrive and sends appropriate response based on file type.
        /// For images: sends inline base64-encoded attachment. For other files: sends OneDrive link.
        /// </summary>
        /// <param name="response">The file consent response containing upload information.</param>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="log">Logger for tracking file upload operations.</param>
        private async Task OnFileConsentAccept(FileConsentCardResponse response, IContext.Client client, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var context = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                    System.Text.Json.JsonSerializer.Serialize(response.Context));
                
                var fileName = context?["fileName"] ?? "file.txt";
                var filePath = Path.Combine(_filesPath, fileName);

                if (!File.Exists(filePath))
                {
                    await client.Send($"File {fileName} not found.");
                    return;
                }

                var fileData = await File.ReadAllBytesAsync(filePath);
                var uploadInfo = response.UploadInfo;

                // Upload the file using PUT request
                var httpClient = _httpClientFactory.CreateClient();
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileData.Length - 1, fileData.Length);

                var uploadResponse = await httpClient.PutAsync(uploadInfo.UploadUrl, fileContent);
                uploadResponse.EnsureSuccessStatusCode();

                log.Info($"File {fileName} uploaded successfully");

                // Extract file extension (without the dot) for FileType
                var fileExtension = Path.GetExtension(fileName)?.TrimStart('.') ?? 
                                   uploadInfo.FileType ?? 
                                   "file";

                // For images, send as inline attachment
                var lowerFileName = fileName.ToLower();
                if (lowerFileName.EndsWith(".png") || lowerFileName.EndsWith(".jpg") || 
                    lowerFileName.EndsWith(".jpeg") || lowerFileName.EndsWith(".gif"))
                {
                    // Read the file and encode as base64
                    var imageData = Convert.ToBase64String(fileData);
                    var mimeType = lowerFileName.EndsWith(".png") ? "image/png" :
                                  lowerFileName.EndsWith(".gif") ? "image/gif" : "image/jpeg";

                    var imageAttachment = new Attachment
                    {
                        Name = fileName,
                        ContentType = new ContentType(mimeType),
                        ContentUrl = $"data:{mimeType};base64,{imageData}"
                    };

                    var successMessage = new MessageActivity($"<b>File uploaded successfully.</b> Your file <b>{fileName}</b> has been uploaded to OneDrive.");
                    successMessage.TextFormat = Microsoft.Teams.Api.TextFormat.Xml;
                    successMessage.Attachments = new List<Attachment> { imageAttachment };
                    await client.Send(successMessage);
                }
                else
                {
                    // For non-image files, just send confirmation with link
                    var successMessage = new MessageActivity($"<b>File uploaded successfully.</b> Your file <b>{fileName}</b> has been uploaded to <a href=\"{uploadInfo.ContentUrl}\">OneDrive</a>. Click the link to view or download.");
                    successMessage.TextFormat = Microsoft.Teams.Api.TextFormat.Xml;
                    await client.Send(successMessage);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error uploading file: {ex.Message}");
                await client.Send($"Error uploading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the user declining the file upload consent.
        /// Sends a confirmation message indicating the file will not be uploaded.
        /// </summary>
        /// <param name="response">The file consent response containing decline information.</param>
        /// <param name="client">The Teams client for sending responses.</param>
        /// <param name="log">Logger for tracking decline operations.</param>
        private async Task OnFileConsentDecline(FileConsentCardResponse response, IContext.Client client, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var context = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                    System.Text.Json.JsonSerializer.Serialize(response.Context));
                
                var fileName = context?["fileName"] ?? "file";

                log.Info($"File consent declined for {fileName}");
                
                var declineMessage = new MessageActivity($"Declined. We won't upload file <b>{fileName}</b>.");
                declineMessage.TextFormat = Microsoft.Teams.Api.TextFormat.Xml;
                await client.Send(declineMessage);
            }
            catch (Exception ex)
            {
                log.Error($"Error in decline handler: {ex.Message}");
                await client.Send("You declined the file upload.");
            }
        }
    }
}