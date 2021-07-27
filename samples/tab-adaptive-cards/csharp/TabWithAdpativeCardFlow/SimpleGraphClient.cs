// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TabWithAdpativeCardFlow
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

        //Fetching user's profile 
        public async Task<User> GetUserProfile()
        {
            var graphClient = GetAuthenticatedClient();
            return await graphClient.Me.Request().GetAsync();
        }

        //Fetching public url of user profile photo
        public async Task<string> GetPublicURLForProfilePhoto(string applicationBaseUrl)
        {
            var graphClient = GetAuthenticatedClient();
            try
            {
                var stream = await graphClient.Me.Photo.Content
                            .Request()
                            .GetAsync();

                var fileName = "ProflePhoto.png";
                string imagePath = Path.Combine(".", "wwwroot", "photos");

                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);

                imagePath = Path.Combine(imagePath, fileName);

                using (var fileStream = System.IO.File.Create(imagePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
                return  $"{applicationBaseUrl}/photos/{fileName}";
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return "http://adaptivecards.io/content/cats/1.png";
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
