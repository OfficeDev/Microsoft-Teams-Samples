// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Samples.BotAttachments.Models;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;

const string ContentTypeFileDownload = "application/vnd.microsoft.teams.file.download.info";
const string ContentTypeFileConsent = "application/vnd.microsoft.teams.card.file.consent";
const string ContentTypeFileInfo = "application/vnd.microsoft.teams.card.file.info";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.AddTeams();

var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

var httpClientFactory = webApp.Services.GetRequiredService<IHttpClientFactory>();
var pendingUploads = new ConcurrentDictionary<string, byte[]>();

// Handle incoming messages
teamsApp.OnMessage(async context =>
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
                    var response = await httpClient.GetAsync(fileDownloadInfo.DownloadUrl);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync();

                    var fileId = Guid.NewGuid().ToString();
                    pendingUploads[fileId] = content;

                    var fileName = attachment.Name ?? $"image_{Guid.NewGuid()}.png";
                    var receivedMessage = new MessageActivity()
                        .WithText($"Received <b>{fileName}</b>. Requesting permission to save to your OneDrive...")
                        .WithTextFormat(TextFormat.Xml);
                    await context.Send(receivedMessage);

                    await SendFileConsentCard(context, fileName, fileId, content.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download attachment: {ex}");
            }
            return;
        }
    }

    await context.Send("Welcome to the Bot Attachments sample! Please attach a file or image to save to your OneDrive!");
});

// Handle file consent responses
teamsApp.OnFileConsent(async context =>
{
    var fileConsentResponse = context.Activity.Value;
    if (fileConsentResponse == null) return;

    var contextData = fileConsentResponse.Context != null
        ? JsonSerializer.Deserialize<Dictionary<string, string>>((JsonElement)fileConsentResponse.Context)
        : null;

    var fileName = contextData?["filename"] ?? "file";
    var fileId = contextData?["file_id"] ?? string.Empty;

    if (fileConsentResponse.Action == Microsoft.Teams.Api.Action.Accept)
    {
        var acceptedMessage = new MessageActivity()
            .WithText($"Accepted. Uploading <b>{fileName}</b>...")
            .WithTextFormat(TextFormat.Xml);
        await context.Send(acceptedMessage);

        _ = Task.Run(async () =>
        {
            try
            {
                if (!pendingUploads.TryRemove(fileId, out var fileData))
                {
                    Console.WriteLine($"File data not found for fileId: {fileId}");
                    return;
                }

                var uploadInfo = fileConsentResponse.UploadInfo;
                var httpClient = httpClientFactory.CreateClient();
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileData.Length - 1, fileData.Length);

                var uploadResponse = await httpClient.PutAsync(uploadInfo!.UploadUrl, fileContent);
                uploadResponse.EnsureSuccessStatusCode();

                var fileInfoAttachment = new Attachment
                {
                    ContentType = new ContentType(ContentTypeFileInfo),
                    Name = uploadInfo.Name ?? fileName,
                    ContentUrl = uploadInfo.ContentUrl,
                    Content = new { uniqueId = uploadInfo.UniqueId, fileType = uploadInfo.FileType }
                };

                var successMessage = new MessageActivity()
                    .WithText($"<b>{uploadInfo.Name ?? fileName}</b> has been successfully uploaded.")
                    .WithTextFormat(TextFormat.Xml);
                successMessage.Attachments = [fileInfoAttachment];
                await context.Send(successMessage);
            }
            catch (Exception ex)
            {
                pendingUploads.TryRemove(fileId, out _);
                Console.WriteLine($"File upload failed: {ex}");
            }
        });
    }
    else if (fileConsentResponse.Action == Microsoft.Teams.Api.Action.Decline)
    {
        pendingUploads.TryRemove(fileId, out _);
        var declineMessage = new MessageActivity()
            .WithText($"Declined. We won't upload file <b>{fileName}</b>.")
            .WithTextFormat(TextFormat.Xml);
        await context.Send(declineMessage);
    }
});

webApp.Run();

// Send a file consent card to request permission to upload a received file to OneDrive
async Task SendFileConsentCard<T>(IContext<T> context, string fileName, string fileId, int fileSize) where T : IActivity
{
    var consentContext = new { filename = fileName, file_id = fileId };

    var fileCard = new FileConsentCard
    {
        Description = "This is the file I want to send you",
        SizeInBytes = fileSize,
        AcceptContext = consentContext,
        DeclineContext = consentContext
    };

    var message = new MessageActivity
    {
        Attachments =
        [
            new Attachment
            {
                Content = fileCard,
                ContentType = new ContentType(ContentTypeFileConsent),
                Name = fileName
            }
        ]
    };
    await context.Send(message);
}
