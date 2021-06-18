using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProactiveBot.Bots
{
    public class ProactiveAppIntallationHelper
    {
        public GraphServiceClient GetAuthenticatedClient(string token)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

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
                    .Request()
                    .AddAsync(userScopeTeamsAppInstallation);
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                // This is where app is already installed but we don't have conversation reference.
                if (ex.Error.Code == "Conflict")
                {
                    await TriggerConversationUpdate(Userid, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                }
                else throw ex;
            }
        }

        public async Task TriggerConversationUpdate(string Userid, string MicrosoftTenantId, string MicrosoftAppId, string MicrosoftAppPassword, string MicrosoftTeamAppid)
        {
            string accessToken = await GetToken(MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword);
            GraphServiceClient graphClient = GetAuthenticatedClient(accessToken);

            try
            {
                // Docs here: https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages#-retrieve-the-conversation-chatid
                var installedApps = await graphClient.Users[Userid].Teamwork.InstalledApps
                    .Request()
                    .Filter($"teamsApp/externalId eq '{MicrosoftAppId}'")
                    .Expand("teamsAppDefinition")
                    .GetAsync();

                var installedApp = installedApps.FirstOrDefault();

                if (installedApp != null)
                    await graphClient.Users[Userid].Teamwork.InstalledApps[installedApp.Id].Chat
                        .Request()
                        .GetAsync();
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                throw ex;
            }
        }

        
    }
}