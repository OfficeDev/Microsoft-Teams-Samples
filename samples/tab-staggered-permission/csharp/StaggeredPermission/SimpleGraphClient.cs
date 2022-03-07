// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace StaggeredPermission
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

        // Get information about the user's mails.
        public async Task<IUserMessagesCollectionPage> GetMailsAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Messages.Request().GetAsync();
            return me;
        }

        // Get information about the user.
        public async Task<User> GetMeAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Request().GetAsync();
            return me;
        }

        // Gets the user's photo
        public async Task<string> GetPhotoAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var photo = await graphClient.Me.Photo.Content.Request().GetAsync();
            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.CopyTo(ms);
                byte[] buffers = ms.ToArray();
                string imgDataURL = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(buffers));
                return imgDataURL;
            }
            else
            {
                return "";
            }
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