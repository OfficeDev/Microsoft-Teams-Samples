// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GraphChatMigration.GraphClient
{
    public class SimpleGraphClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        ///Get Authenticated Client
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

        public static GraphServiceClient GetAuthenticatedClient(string accessToken)
        {
            var tokenProvider = new SimpleAccessTokenProvider(accessToken);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }

        public static async Task<string> GetAccessToken(string appId, string appPassword, string tenantId)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(appId)
              .WithClientSecret(appPassword)
              .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
              .WithRedirectUri("https://daemon")
              .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }

        // HTTP-based API calls for migration operations
        public static async Task<HttpResponseMessage> StartMigration(string accessToken, string conversationType, string id)
        {
            string endpoint = BuildMigrationEndpoint(conversationType, id, "startMigration");
            return await CallGraphApi(accessToken, endpoint, HttpMethod.Post);
        }

        public static async Task<HttpResponseMessage> CompleteMigration(string accessToken, string conversationType, string id)
        {
            string endpoint = BuildMigrationEndpoint(conversationType, id, "completeMigration");
            return await CallGraphApi(accessToken, endpoint, HttpMethod.Post);
        }

        public static async Task<HttpResponseMessage> PostMessageWithTimestamp(string accessToken, string conversationType, string id, object messageData)
        {
            string endpoint = BuildMigrationEndpoint(conversationType, id, "messages");
            return await CallGraphApi(accessToken, endpoint, HttpMethod.Post, messageData);
        }

        public static async Task<HttpResponseMessage> GetConversationDetails(string accessToken, string conversationType, string id)
        {
            string endpoint = BuildMigrationEndpoint(conversationType, id, "");
            return await CallGraphApi(accessToken, endpoint, HttpMethod.Get);
        }

        private static string BuildMigrationEndpoint(string conversationType, string id, string operation)
        {
            if (conversationType.Equals("channel", StringComparison.OrdinalIgnoreCase))
            {
                // For channel endpoints, we need teamId/channelId
                string[] parts = id.Split('|');
                if (parts.Length == 2)
                {
                    return $"https://graph.microsoft.com/beta/teams/{parts[0]}/channels/{parts[1]}/{operation}";
                }
                return $"https://graph.microsoft.com/beta/teams/{id}/{operation}";
            }
            else if (conversationType.Equals("groupchat", StringComparison.OrdinalIgnoreCase))
            {
                return $"https://graph.microsoft.com/beta/chats/{id}/{operation}";
            }
            
            throw new ArgumentException($"Unsupported conversation type: {conversationType}");
        }

        private static async Task<HttpResponseMessage> CallGraphApi(string accessToken, string endpoint, HttpMethod method, object data = null)
        {
            using var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            if (data != null)
            {
                string jsonContent = JsonSerializer.Serialize(data);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
            
            return await httpClient.SendAsync(request);
        }
    }
}