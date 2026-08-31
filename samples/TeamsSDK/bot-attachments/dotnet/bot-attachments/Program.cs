// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Files;
using Microsoft.Teams.Apps.Schema;
using Microsoft.Teams.Core.Hosting;
using Microsoft.Teams.Samples.BotAttachments.Models;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

const string ContentTypeFileDownload = "application/vnd.microsoft.teams.file.download.info";

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddTeamsBotApplication();

var webApp = builder.Build();
var teamsApp = webApp.UseTeamsBotApplication();

var httpClientFactory = webApp.Services.GetRequiredService<IHttpClientFactory>();
var pendingUploads = new ConcurrentDictionary<string, byte[]>();

// Handle incoming messages
teamsApp.OnMessage(async (context, cancellationToken) =>
{
    var activity = context.Activity;
    var attachment = activity.Attachments?.FirstOrDefault();

    if (attachment != null)
    {
        var contentTypeValue = attachment.ContentType?.Value ?? attachment.ContentType?.ToString() ?? "";

        if (contentTypeValue == ContentTypeFileDownload)
        {
            try
            {
                var fileDownloadInfo = attachment.Content != null
                    ? JsonSerializer.Deserialize<FileDownloadInfo>((JsonElement)attachment.Content)
                    : null;

                if (fileDownloadInfo?.DownloadUrl != null)
                {
                    var httpClient = httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(fileDownloadInfo.DownloadUrl, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                    var fileId = Guid.NewGuid().ToString();
                    pendingUploads[fileId] = content;

                    var fileName = attachment.Name ?? $"image_{Guid.NewGuid()}.png";
                    var receivedMessage = new MessageActivityInput()
                        .WithText($"Received <b>{fileName}</b>. Requesting permission to save to your OneDrive...")
                        .WithTextFormat(TextFormats.Xml);
                    await context.SendAsync(receivedMessage, cancellationToken);

                    await SendFileConsentCard(context, fileName, fileId, content.Length, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download attachment: {ex}");
            }
            return;
        }
    }

    await context.SendAsync("Welcome to the Bot Attachments sample! Please attach a file or image to save to your OneDrive!", cancellationToken);
});

// Handle file consent responses
teamsApp.OnFileConsent(async (context, cancellationToken) =>
{
    var fileConsentResponse = context.Activity.Value;
    if (fileConsentResponse == null)
    {
        return AdaptiveCardResponse.CreateBuilder().WithStatusCode(400).Build();
    }

    var contextData = fileConsentResponse.Context != null
        ? JsonSerializer.Deserialize<Dictionary<string, string>>((JsonElement)fileConsentResponse.Context)
        : null;

    contextData?.TryGetValue("filename", out var fileName);
    contextData?.TryGetValue("file_id", out var fileId);

    var fileName = contextData != null && contextData.TryGetValue("filename", out var name) && !string.IsNullOrEmpty(name)
        ? name
        : "file";
    var fileId = contextData != null && contextData.TryGetValue("file_id", out var id) && id != null
        ? id
        : string.Empty;

    if (fileConsentResponse.Action == "accept")
    {
        var acceptedMessage = new MessageActivityInput()
            .WithText($"Accepted. Uploading <b>{fileName}</b>...")
            .WithTextFormat(TextFormats.Xml);
        await context.SendAsync(acceptedMessage, cancellationToken);

        try
        {
            if (!pendingUploads.TryRemove(fileId, out var fileData))
            {
                Console.WriteLine($"File data not found for fileId: {fileId}");
                return AdaptiveCardResponse.CreateBuilder().WithStatusCode(404).Build();
            }

            var uploadInfo = fileConsentResponse.UploadInfo;
            var httpClient = httpClientFactory.CreateClient();
            using var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileData.Length - 1, fileData.Length);

            var uploadResponse = await httpClient.PutAsync(uploadInfo!.UploadUrl, fileContent, cancellationToken);
            uploadResponse.EnsureSuccessStatusCode();

            var fileInfoAttachment = TeamsAttachment.CreateBuilder()
                .WithContentType(AttachmentContentTypes.FileInfoCard)
                .WithName(uploadInfo.Name ?? fileName)
                .WithContentUrl(uploadInfo.ContentUrl)
                .WithContent(new { uniqueId = uploadInfo.UniqueId, fileType = uploadInfo.FileType })
                .Build();

            var successMessage = new MessageActivityInput()
                .WithText($"<b>{uploadInfo.Name ?? fileName}</b> has been successfully uploaded.")
                .WithTextFormat(TextFormats.Xml)
                .AddAttachment(fileInfoAttachment);
            await context.SendAsync(successMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            pendingUploads.TryRemove(fileId, out _);
            Console.WriteLine($"File upload failed: {ex}");
        }
    }
    else if (fileConsentResponse.Action == "decline")
    {
        pendingUploads.TryRemove(fileId, out _);
        var declineMessage = new MessageActivityInput()
            .WithText($"Declined. We won't upload file <b>{fileName}</b>.")
            .WithTextFormat(TextFormats.Xml);
        await context.SendAsync(declineMessage, cancellationToken);
    }

    return AdaptiveCardResponse.CreateBuilder().WithStatusCode(200).Build();
});

webApp.Run();

// Send a file consent card to request permission to upload a received file to OneDrive
async Task SendFileConsentCard(Context<MessageActivity> context, string fileName, string fileId, int fileSize, CancellationToken cancellationToken)
{
    var consentContext = new JsonObject
    {
        ["filename"] = fileName,
        ["file_id"] = fileId
    };

    var fileCard = new JsonObject
    {
        ["description"] = "This is the file I want to send you",
        ["sizeInBytes"] = fileSize,
        ["acceptContext"] = consentContext.DeepClone(),
        ["declineContext"] = consentContext.DeepClone()
    };

    var attachment = TeamsAttachment.CreateBuilder()
        .WithContent(fileCard)
        .WithContentType(AttachmentContentTypes.FileConsentCard)
        .WithName(fileName)
        .Build();

    await context.SendAsync(new MessageActivityInput().AddAttachment(attachment), cancellationToken);
}
