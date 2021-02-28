using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace TabActivityFeed
{
    public class SimpleGraphClient
    {
        
        public static GraphServiceClient GetGraphClient(string appId, string appPassword, string tenantId)
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
