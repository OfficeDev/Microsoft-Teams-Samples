using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Authentication;
using Microsoft.Kiota.Abstractions.Authentication;

namespace BotWithSharePointFileViewer
{
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            _token = string.IsNullOrWhiteSpace(token) ? throw new ArgumentNullException(nameof(token)) : token;
        }

        // Updated method to fix the CS1061 error by replacing the incorrect 'Children' property access
        // with the correct approach to retrieve children items from the root of the drive.

        public async Task<List<string>> GetSharePointFile(string sharepointSiteName, string sharepointTenantName)
        {
            var graphClient = GetAuthenticatedClient();

            try
            {
                // Format: domain:/sites/site-name
                var sitePath = $"{sharepointTenantName}.sharepoint.com:/sites/{sharepointSiteName}";
                var site = await graphClient.Sites[sitePath].GetAsync();

                if (site == null) return null;

                var drive = await graphClient.Sites[site.Id].Drive.GetAsync();
                if (drive == null) return null;

                // Corrected code: Use the 'Children' property of the DriveItemRequestBuilder for the root item.
                var rootItem = await graphClient.Drives[drive.Id].Root.GetAsync();
                if (rootItem == null) return null;

                var children = await graphClient.Drives[drive.Id].Items[rootItem.Id].Children.GetAsync();

                var fileNames = new List<string>();
                foreach (var file in children.Value)
                {
                    fileNames.Add(file.Name);
                }

                return fileNames;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception is " + e.Message);
                return null;
            }
        }

        // Upload file to SharePoint site.
        public async Task UploadFileInSharePointSite(string sharepointSiteName, string sharepointTenantName, string fileName, Stream stream)
        {
            var graphClient = GetAuthenticatedClient();

            try
            {
                var sitePath = $"{sharepointTenantName}.sharepoint.com:/sites/{sharepointSiteName}";
                var site = await graphClient.Sites[sitePath].GetAsync();
                if (site == null)
                {
                    Console.WriteLine("Site not found.");
                    return;
                }

                var drive = await graphClient.Sites[site.Id].Drive.GetAsync();
                if (drive == null)
                {
                    Console.WriteLine("Drive not found.");
                    return;
                }

                await graphClient
                    .Drives[drive.Id]
                    .Root
                    .ItemWithPath(fileName)
                    .Content
                    .PutAsync(stream);

                Console.WriteLine("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upload failed: " + ex.Message);
            }
        }

        // Custom Token Provider for SDK v5
        private class TokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;
            public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();

            public TokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri,
                Dictionary<string, object> additionalAuthenticationContext = null,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }
        }

        // Get Graph client authenticated with v5 pattern
        private GraphServiceClient GetAuthenticatedClient()
        {
            var tokenProvider = new TokenProvider(_token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
            return new GraphServiceClient(authProvider);
        }
    }
}
