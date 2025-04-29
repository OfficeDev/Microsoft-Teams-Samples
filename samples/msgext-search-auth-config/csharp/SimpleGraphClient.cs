// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Threading;

namespace Microsoft.BotBuilderSamples
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

        // Searches the user's mail Inbox using the Microsoft Graph API
        public async Task<List<Message>> SearchMailInboxAsync(string search)
        {
            var graphClient = GetAuthenticatedClient();

            var response = await graphClient
                .Me
                .MailFolders["Inbox"]
                .Messages
                .GetAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.Search = search;
                    requestConfig.QueryParameters.Top = 10;
                });

            return response?.Value?.ToList() ?? new List<Message>();
        }

        //Fetching user's profile 
        public async Task<User> GetMyProfile()
        {
            var graphClient = GetAuthenticatedClient();
            return await graphClient.Me.GetAsync();
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
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
