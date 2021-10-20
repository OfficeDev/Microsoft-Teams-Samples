// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace BotWithSharePointFileViewer
{
    // This class is a wrapper for the Microsoft Graph API
    // See: https://developer.microsoft.com/en-us/graph
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        // Get share point file.
        public async Task<DriveItem> GetSharePointFile(string itemId)
        {
            var graphClient = GetAuthenticatedClient();
            var fileInfo = await graphClient.Me.Drive.Items[itemId]
                                .Request()
                                .GetAsync();
            return fileInfo;
        }

        // Send file to chat.
        public async void SendFileToChat(string chatid, DriveItem driveItem)
        {
            var graphClient = GetAuthenticatedClient();
            var weburl = driveItem.WebUrl.Substring(0, driveItem.WebUrl.IndexOf("&action"));

            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = $"Here's the share point file. <attachment id=\"{driveItem.ETag.Substring(2,36)}\"></attachment>"
                },
                Attachments = new List<ChatMessageAttachment>()
                {
                    new ChatMessageAttachment
                    {
                        Id = driveItem.ETag.Substring(2,36),
                        ContentType = "reference",
                        ContentUrl = weburl,
                        Name = driveItem.Name
                    }
                }
            };

            await graphClient.Chats[chatid].Messages
                .Request()
                .AddAsync(chatMessage);
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }
    }
}