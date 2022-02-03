// <copyright file="SimpleGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace ActivityFeedBroadcast
{
    public class SimpleGraphClient
    {
        // Get graph client for user.
        public static GraphServiceClient GetGraphClient(string accessToken)
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                return Task.CompletedTask;
            }));

            return graphClient;
        }

        // Get graph client for app.
        public static GraphServiceClient GetGraphClientforApp(string appId, string appPassword, string tenantId)
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                // get an access token for Graph
                var accessToken = GetAccessToken(appId, appPassword, tenantId).Result;

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            return graphClient;
        }

        // Get access token.
        private static async Task<string> GetAccessToken(string appId, string appPassword, string tenantId)
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
    }
}