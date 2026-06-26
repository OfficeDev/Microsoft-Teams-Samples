using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.BotBuilderSamples.SPListBot.Models;

namespace Microsoft.BotBuilderSamples.SPListBot.Repositories
{
    public class SharepointRepository
    {
        private static readonly HttpClient s_httpClient = new HttpClient();

        /// <summary>
        /// Gets the authentication token for SharePoint access.
        /// </summary>
        private static async Task<string> GetAuthenticationTokenAsync()
        {
            var tenantName = SettingsConfig.AppSetting("TenantName");
            var tenantId = SettingsConfig.AppSetting("TenantID");
            var resourceId = SettingsConfig.AppSetting("ResourceID");
            var appClientId = SettingsConfig.AppSetting("AppClientID");
            var appSecret = SettingsConfig.AppSetting("AppSecret");

            var clientId = $"{appClientId}@{tenantId}";
            var url = "https://accounts.accesscontrol.windows.net/tokens/OAuth/2";
            var resource = $"{resourceId}/{tenantName}.sharepoint.com@{tenantId}";
            var body = $"grant_type=client_credentials&client_id={clientId}&client_secret={appSecret}&resource={resource}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await s_httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Authentication failed: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
            return tokenResponse?.AccessToken 
                ?? throw new InvalidOperationException("No access token received from authentication service");
        }

        /// <summary>
        /// Writes conversation data to a SharePoint list.
        /// </summary>
        public static async Task<bool> WriteConversationToSPListAsync(Values body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            try
            {
                var accessToken = await GetAuthenticationTokenAsync();

                var tenantName = SettingsConfig.AppSetting("TenantName");
                var siteName = SettingsConfig.AppSetting("SiteName");
                var listName = SettingsConfig.AppSetting("ListName");

                var endpoint = $"https://{tenantName}.sharepoint.com/sites/{siteName}/_api/web/lists/GetByTitle('{listName}')/items";

                var itemData = new
                {
                    Description = body.Description,
                    UserName = body.Username,
                    Name = body.Name,
                    Address = body.Address
                };

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(itemData)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await s_httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Failed to write to SharePoint list: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error writing conversation to SharePoint list", ex);
            }
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}