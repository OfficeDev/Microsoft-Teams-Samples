// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

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

        // Get SharePoint file list.
        public async Task<List<string>> GetSharePointFile(string sharepointSiteName, string sharepointTenantName)
        {
            var graphClient = GetAuthenticatedClient();

            try
            {
                var site = await graphClient.Sites[sharepointTenantName].Sites[sharepointSiteName]
                                  .GetAsync();
                if (site != null)
                {
                    var drive = await graphClient.Sites[site.Id].Drives
                                        .GetAsync();

                    if (drive != null)
                    {
                        var driveId = drive.Value[0].Id;
                        var children = await graphClient.Drives[driveId].Items["root"].Children.GetAsync();

                        var fileName = new List<string>();
                        foreach (var file in children.Value)
                        {
                            fileName.Add(file.Name);
                        }

                        return fileName;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception is " + e);
                return null;
            }
        }

        // Upload file to SharePoint site.
        public async void UploadFileInSharePointSite(string sharepointSiteName, string sharepointTenantName, string fileName, Stream stream)
        {
            var graphClient = GetAuthenticatedClient();

            var site = await graphClient.Sites[sharepointTenantName].Sites[sharepointSiteName]
                             .GetAsync();

            if (site != null)
            {
                var drive = await graphClient.Sites[site.Id].Drives
                                    .GetAsync();

                if (drive != null && drive.Value != null && drive.Value.Count > 0)
                {
                    var driveId = drive.Value[0].Id; // Ensure CurrentPage has at least one item
                    await graphClient.Drives[driveId].Root.ItemWithPath(fileName).Content.PutAsync(stream);
                }
                else
                {
                    // Handle the case where no drives are found
                    // throw new Exception("No drives found for the specified site.");
                    Console.WriteLine("No drives found for the specified site.");
                }
            }
            else
            {
                throw new Exception("Site is null.");
            }
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var accessTokenProvider = new SimpleAccessTokenProvider(_token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
            return new GraphServiceClient(authProvider);
        }

        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _token;

            public SimpleAccessTokenProvider(string token)
            {
                _token = token;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_token);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }
    }
}