// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace FetchGroupChatMessagesWithRSC
{
    /// <summary>
    /// This class is a wrapper for the Microsoft Graph API.
    /// See: https://developer.microsoft.com/en-us/graph
    /// </summary>
    public class SimpleGraphClient
    {
        private readonly string _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGraphClient"/> class.
        /// </summary>
        /// <param name="token">The access token.</param>
        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        /// <summary>
        /// Fetches chat messages from a group chat.
        /// </summary>
        /// <param name="chatId">The chat ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat messages collection.</returns>
        public async Task<IChatMessagesCollectionPage> GetGroupChatMessages(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                throw new ArgumentNullException(nameof(chatId));
            }

            var graphClient = GetAuthenticatedClient();
            var messages = await graphClient.Chats[chatId].Messages
                        .Request()
                        .GetAsync();
            return messages;
        }

        /// <summary>
        /// Gets an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>The authenticated GraphServiceClient.</returns>
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }
    }
}