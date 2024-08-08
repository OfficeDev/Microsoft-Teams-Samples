// <copyright file="SimpleGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TagMentionBot
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

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        public async Task<ITeamTagsCollectionPage> GetTag( string teamId)
        {
            var graphClient = GetAuthenticatedClient();
            var tag = await graphClient.Teams[teamId].Tags.Request().GetAsync();
            return tag;
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