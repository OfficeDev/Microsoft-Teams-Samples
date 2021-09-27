using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FetchGroupChatMessagesWithRSC.helper
{
    public class GetChatHelper
    {
        private async Task<GraphServiceClient> GetAuthenticatedClient(string tenantId, string MicrosoftAppId, string MicrosoftAppPassword)
        {
            var accessToken = await GetToken(tenantId, MicrosoftAppId, MicrosoftAppPassword);
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }

        public async Task<string> GetToken(string tenantId, string MicrosoftAppId, string MicrosoftAppPassword)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(MicrosoftAppId)
                                                  .WithClientSecret(MicrosoftAppPassword)
                                                  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task<IChatMessagesCollectionPage> GetGroupChatMessage(string MicrosoftTenantId, string MicrosoftAppId, string MicrosoftAppPassword, string Chatid)
        {
            GraphServiceClient graphClient = await GetAuthenticatedClient(MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword);

            try
            {
                var messages = await graphClient.Chats[Chatid].Messages
                        .Request()
                        .GetAsync();
                return messages;
            }
            catch (ServiceException ex)
            {
                // This is where app is already installed but we don't have conversation reference.
                throw ex;
            }
        }
    }
}
