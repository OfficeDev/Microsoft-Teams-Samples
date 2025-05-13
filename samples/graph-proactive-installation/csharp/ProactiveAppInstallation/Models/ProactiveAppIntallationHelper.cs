using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ProactiveBot.Bots
{
    public class ProactiveAppIntallationHelper
    {
        public GraphServiceClient GetAuthenticatedClient(string token)
        {
            var tokenProvider = new SimpleAccessTokenProvider(token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
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

        public async Task AppinstallationforPersonal(string Userid, string MicrosoftTenantId, string MicrosoftAppId, string MicrosoftAppPassword, string MicrosoftTeamAppid)
        {
            string Access_Token = await GetToken(MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword);
            GraphServiceClient graphClient = GetAuthenticatedClient(Access_Token);

            try
            {
                var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>()
                        {
                            {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"+MicrosoftTeamAppid}
                        }
                };
                await graphClient.Users[Userid].Teamwork.InstalledApps
                    .PostAsync(userScopeTeamsAppInstallation);
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                if (odataError?.Error?.Code == "Conflict")
                {
                    await TriggerConversationUpdate(Userid, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task TriggerConversationUpdate(string Userid, string MicrosoftTenantId, string MicrosoftAppId, string MicrosoftAppPassword, string MicrosoftTeamAppid)
        {
            string accessToken = await GetToken(MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword);
            GraphServiceClient graphClient = GetAuthenticatedClient(accessToken);

            try
            {
                // Docs here: https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages#-retrieve-the-conversation-chatid
                var installedApps = await graphClient.Users[Userid].Teamwork.InstalledApps.GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"teamsApp/externalId eq '{MicrosoftAppId}'";
                    config.QueryParameters.Expand = new[] { "teamsAppDefinition" };
                });

                var installedApp = installedApps.Value.FirstOrDefault();

                if (installedApp != null)
                    await graphClient.Users[Userid].Teamwork.InstalledApps[installedApp.Id].Chat
                        .GetAsync();
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                throw ex;
            }
        }

        
    }

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
}