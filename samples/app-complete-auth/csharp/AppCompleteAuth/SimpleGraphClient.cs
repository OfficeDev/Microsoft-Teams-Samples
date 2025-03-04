// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace AppCompleteAuth
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
        /// Get information about the user.
        /// </summary>
        /// <returns>User information.</returns>
        public async Task<User> GetMeAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Request().GetAsync();
            return me;
        }

        /// <summary>
        /// Gets the user's photo.
        /// </summary>
        /// <returns>User's photo as a base64 string.</returns>
        public async Task<string> GetPhotoAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var photo = await graphClient.Me.Photo.Content.Request().GetAsync();
            if (photo != null)
            {
                using var ms = new MemoryStream();
                photo.CopyTo(ms);
                byte[] buffers = ms.ToArray();
                string imgDataURL = $"data:image/png;base64,{Convert.ToBase64String(buffers)}";
                return imgDataURL;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get user profile by user principal name.
        /// </summary>
        /// <param name="userPrincipalName">User principal name.</param>
        /// <returns>User profile.</returns>
        public async Task<User> GetUserProfile(string userPrincipalName)
        {
            var graphClient = GetAuthenticatedClient();
            var user = await graphClient.Users[userPrincipalName].Request().GetAsync();
            return user;
        }

        /// <summary>
        /// Get an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>Authenticated GraphServiceClient.</returns>
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }
    }
}