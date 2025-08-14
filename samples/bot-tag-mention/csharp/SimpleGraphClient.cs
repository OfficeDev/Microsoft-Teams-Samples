// <copyright file="SimpleGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TagMentionBot
{
    /// <summary>
    /// This class is a wrapper for the Microsoft Graph API.
    /// See: https://developer.microsoft.com/en-us/graph
    /// </summary>
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

        /// <summary>
        /// Gets an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <param name="teamId">The ID of the team.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the team tags collection page.</returns>
        public async Task<ITeamTagsCollectionPage> GetTag(string teamId)
        {
            var graphClient = GetAuthenticatedClient();
            return await graphClient.Teams[teamId].Tags.Request().GetAsync();
        }

        /// <summary>
        /// Gets an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>The authenticated Microsoft Graph client.</returns>
        private GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                        return Task.CompletedTask;
                    }));
        }
    }
}