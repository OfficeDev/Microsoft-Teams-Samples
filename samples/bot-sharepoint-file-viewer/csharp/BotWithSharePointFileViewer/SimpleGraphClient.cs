// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
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

        // Get SharePoint file list.
        public async Task<List<string>> GetSharePointFile(string sharepointSiteName, string sharepointTenantName)
        {
            var graphClient = GetAuthenticatedClient();

            try
            {
                var site = await graphClient.Sites[sharepointTenantName].Sites[sharepointSiteName]
                                  .Request()
                                  .GetAsync();
                if (site != null)
                {
                    var drive = await graphClient.Sites[site.Id].Drives
                                        .Request()
                                        .GetAsync();

                    if (drive != null)
                    {
                        var children = await graphClient.Sites[site.Id].Drives[drive.CurrentPage[0].Id].Root.Children
                                                .Request()
                                                .GetAsync();

                        var fileName = new List<string>();
                        foreach (var file in children.CurrentPage)
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
            catch(Exception e)
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
                             .Request()
                             .GetAsync();

            if (site != null)
            {
                var drive = await graphClient.Sites[site.Id].Drives
                                    .Request()
                                    .GetAsync();
                if (drive != null)
                {
                    await graphClient.Sites[site.Id].Drives[drive.CurrentPage[0].Id].Root.ItemWithPath(fileName).Content
                            .Request()
                            .PutAsync<DriveItem>(stream);
                }
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