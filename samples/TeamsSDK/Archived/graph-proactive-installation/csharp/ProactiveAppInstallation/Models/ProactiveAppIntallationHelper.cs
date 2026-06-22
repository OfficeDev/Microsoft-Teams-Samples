using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace ProactiveBot.Bots
{
    public class ProactiveAppIntallationHelper
    {
        private static GraphServiceClient GetAuthenticatedClient(string tenantId, string appId, string appPassword)
        {
            var credential = new ClientSecretCredential(tenantId, appId, appPassword);
            return new GraphServiceClient(credential);
        }

        public async Task AppinstallationforPersonal(string userId, string tenantId, string appId, string appPassword, string teamsAppId)
        {
            var graphClient = GetAuthenticatedClient(tenantId, appId, appPassword);

            try
            {
                var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamsAppId}" }
                    }
                };

                await graphClient.Users[userId].Teamwork.InstalledApps
                    .PostAsync(userScopeTeamsAppInstallation);
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                if (odataError?.Error?.Code == "Conflict")
                {
                    await TriggerConversationUpdate(userId, graphClient, appId);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task TriggerConversationUpdate(string userId, GraphServiceClient graphClient, string appId)
        {
            var installedApps = await graphClient.Users[userId].Teamwork.InstalledApps.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"teamsApp/externalId eq '{appId}'";
                config.QueryParameters.Expand = new[] { "teamsAppDefinition" };
            });

            var installedApp = installedApps?.Value?.FirstOrDefault();

            if (installedApp != null)
            {
                await graphClient.Users[userId].Teamwork.InstalledApps[installedApp.Id].Chat
                    .GetAsync();
            }
        }
    }
}