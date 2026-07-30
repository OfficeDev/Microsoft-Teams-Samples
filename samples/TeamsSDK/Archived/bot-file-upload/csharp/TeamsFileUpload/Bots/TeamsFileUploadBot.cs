// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots;

public class TeamsFileUploadBot : TeamsActivityHandler
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _microsoftAppId;
    private readonly string _microsoftAppPassword;

    public TeamsFileUploadBot(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _microsoftAppId = configuration["MicrosoftAppId"] ?? string.Empty;
        _microsoftAppPassword = configuration["MicrosoftAppPassword"] ?? string.Empty;
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var attachments = turnContext.Activity.Attachments;

        if (attachments?[0].ContentType == FileDownloadInfo.ContentType)
        {
            var file = attachments[0];
            var fileDownload = JObject.FromObject(file.Content!).ToObject<FileDownloadInfo>()!;
            var filePath = Path.Combine("Files", file.Name);

            using var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(fileDownload.DownloadUrl, cancellationToken);
            await SaveFileAsync(filePath, response);

            var reply = MessageFactory.Text($"<b>{file.Name}</b> received and saved.");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        else if (attachments?[0].ContentType?.Contains("image/*") == true)
        {
            await ProcessInlineImageAsync(turnContext, cancellationToken);
        }
        else
        {
            var filename = "teams-logo.png";
            var filePath = Path.Combine("Files", filename);
            var fileSize = new FileInfo(filePath).Length;
            await SendFileCardAsync(turnContext, filename, fileSize, cancellationToken);
        }
    }

    private static async Task SaveFileAsync(string filePath, HttpResponseMessage response)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }

    private async Task ProcessInlineImageAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var attachment = turnContext.Activity.Attachments![0];
        var token = await new MicrosoftAppCredentials(_microsoftAppId, _microsoftAppPassword).GetTokenAsync();

        using var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var responseMessage = await client.GetAsync(attachment.ContentUrl, cancellationToken);

        var filePath = Path.Combine("Files", "ImageFromUser.png");
        await SaveFileAsync(filePath, responseMessage);

        var reply = MessageFactory.Text($"Attachment of {attachment.ContentType} type and size of {responseMessage.Content.Headers.ContentLength} bytes received.");
        reply.Attachments = new List<Attachment> { GetInlineAttachment() };
        await turnContext.SendActivityAsync(reply, cancellationToken);
    }

    private static Attachment GetInlineAttachment()
    {
        var imagePath = Path.Combine("Files", "ImageFromUser.png");
        var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

        return new Attachment
        {
            Name = "ImageFromUser.png",
            ContentType = "image/png",
            ContentUrl = $"data:image/png;base64,{imageData}",
        };
    }

    private static async Task SendFileCardAsync(ITurnContext turnContext, string filename, long filesize, CancellationToken cancellationToken)
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

    protected override async Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
    {
        try
        {
            var context = JObject.FromObject(fileConsentCardResponse.Context!);
            var filePath = Path.Combine("Files", context["filename"]!.ToString());
            var fileSize = new FileInfo(filePath).Length;

            using var client = _clientFactory.CreateClient();
            using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentLength = fileSize;
            fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileSize - 1, fileSize);
            await client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, fileContent, cancellationToken);

            await FileUploadCompletedAsync(turnContext, fileConsentCardResponse, cancellationToken);
        }
        catch (Exception e)
        {
            await FileUploadFailedAsync(turnContext, e.ToString(), cancellationToken);
        }
    }

    protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
    {
        var context = JObject.FromObject(fileConsentCardResponse.Context!);
        var reply = MessageFactory.Text($"Declined. We won't upload file <b>{context["filename"]}</b>.");
        reply.TextFormat = "xml";
        await turnContext.SendActivityAsync(reply, cancellationToken);
    }

    private static async Task FileUploadCompletedAsync(ITurnContext turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
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

    private static async Task FileUploadFailedAsync(ITurnContext turnContext, string error, CancellationToken cancellationToken)
    {
        var reply = MessageFactory.Text($"<b>File upload failed.</b> Error: <pre>{error}</pre>");
        reply.TextFormat = "xml";
        await turnContext.SendActivityAsync(reply, cancellationToken);
    }
}
