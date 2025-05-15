using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using ReleaseManagement.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReleaseManagement.Helpers
{
    public class GraphHelper
    {
        private readonly IOptions<AzureSettings> azureSettings;

        public GraphHelper(IOptions<AzureSettings> azureSettings)
        {
            this.azureSettings = azureSettings;
        }

        /// <summary>
        /// Generates an authenticated GraphServiceClient using a token.
        /// </summary>
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

        private GraphServiceClient GetAuthenticatedClient(string token)
        {
            var tokenProvider = new SimpleAccessTokenProvider(token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }

        /// <summary>
        /// Acquires an access token using client credentials.
        /// </summary>
        public async Task<string> GetToken()
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(azureSettings.Value.MicrosoftAppId)
                .WithClientSecret(azureSettings.Value.MicrosoftAppPassword)
                .WithAuthority($"https://login.microsoftonline.com/{azureSettings.Value.MicrosoftAppTenantId}")
                .Build();

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        /// <summary>
        /// Installs a Teams app in a group chat.
        /// </summary>
        public async Task AppinstallationforGroupAsync(string groupId)
        {
            var token = await GetToken();
            var graphClient = GetAuthenticatedClient(token);

            try
            {
                var appInternalId = await GetApplicationInternalId(graphClient);

                var installation = new TeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["teamsApp@odata.bind"] = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appInternalId}"
                    }
                };

                await graphClient.Chats[groupId].InstalledApps.PostAsync(installation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppinstallationforGroupAsync] {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new group chat and returns the chat ID.
        /// </summary>
        public async Task<string> CreateGroupChatAsync(IEnumerable<string> userMails, string groupTitle)
        {
            var token = await GetToken();
            var graphClient = GetAuthenticatedClient(token);

            int retry = 0;
            const int maxRetries = 5;

            while (retry < maxRetries)
            {
                try
                {
                    var members = userMails
                        .Distinct()
                        .Select(mail => new AadUserConversationMember
                        {
                            Roles = new List<string> { "owner" },
                            AdditionalData = new Dictionary<string, object>
                            {
                                ["user@odata.bind"] = $"https://graph.microsoft.com/v1.0/users/{mail}"
                            }
                        }).Cast<ConversationMember>().ToList();

                    var chat = new Chat
                    {
                        ChatType = ChatType.Group,
                        Topic = groupTitle,
                        Members = members
                    };

                    var createdChat = await graphClient.Chats.PostAsync(chat);
                    return createdChat?.Id;
                }
                catch (Exception ex)
                {
                    retry++;
                    Console.WriteLine($"[CreateGroupChatAsync] Retry {retry}: {ex.Message}");
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a user's profile picture in base64 format.
        /// </summary>
        public async Task<string> GetProfilePictureByUserPrincipalNameAsync(string userPrincipalName)
        {
            var token = await GetToken();
            var graphClient = GetAuthenticatedClient(token);

            try
            {
                var user = await graphClient.Users[userPrincipalName].GetAsync();
                if (user == null) return string.Empty;

                var stream = await graphClient.Users[user.Id].Photo.Content.GetAsync();
                if (stream == null) return string.Empty;

                var bytes = ReadFully(stream);
                return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProfilePictureByUserPrincipalNameAsync] {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the internal Teams app ID by its external (client) ID.
        /// </summary>
        private async Task<string> GetApplicationInternalId(GraphServiceClient graphClient)
        {
            try
            {
                var result = await graphClient.AppCatalogs.TeamsApps.GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"externalId eq '{azureSettings.Value.MicrosoftAppId}'";
                });

                return result?.Value?.FirstOrDefault()?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetApplicationInternalId] {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts a stream to byte array.
        /// </summary>
        private byte[] ReadFully(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
