// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

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
        public async Task<List<ChatMessage>> GetGroupChatMessages(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                throw new ArgumentNullException(nameof(chatId));
            }

            var graphClient = GetAuthenticatedClient();
            var response = await graphClient.Chats[chatId].Messages.GetAsync();

            return response?.Value ?? new List<ChatMessage>();
        }

        /// <summary>
        /// Gets an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>The authenticated GraphServiceClient.</returns>
        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public SimpleAccessTokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> context = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }

        private GraphServiceClient GetAuthenticatedClient()
        {
            var tokenProvider = new SimpleAccessTokenProvider(_token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }
    }
}