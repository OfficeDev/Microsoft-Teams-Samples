using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.BotBuilderSamples.SPListBot.Models;
using System.Net.Http;
using System.IO;
using System.Net;

namespace Microsoft.BotBuilderSamples.SPListBot.Repositories
{
    public class SharepointRepository
    {
        //This method is added as part of spfx changes. This method gets the authentication token
        private static async Task<string> GetAuthenticationToken()
        {
            string tenantName = SettingsConfig.AppSetting("TenantName");
            string tenantID = SettingsConfig.AppSetting("TenantID");
            string resourceID = SettingsConfig.AppSetting("ResourceID");
            string appClientID = SettingsConfig.AppSetting("AppClientID");
            string appSecret = SettingsConfig.AppSetting("AppSecret");

            string clientID = appClientID + "@" + tenantID;
            string url = "https://accounts.accesscontrol.windows.net/tokens/OAuth/2";
            string res = resourceID + "/" + tenantName + ".sharepoint.com@" + tenantID;
            string body = $"grant_type=client_credentials&client_id={clientID}&client_secret={appSecret}&resource={res}";

            HttpClient httpclient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = (new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
            HttpResponseMessage response = await httpclient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseBody);

            string accessToken = null;
            accessToken = JsonConvert.DeserializeObject<SharepointRepository.TokenResponse>(responseBody).access_token;
            return accessToken;
        }

        public static async Task<bool> WriteConversationToSPList(Values body)
        {
            try
            {
                string accessToken = await SharepointRepository.GetAuthenticationToken();

                string tenantName = SettingsConfig.AppSetting("TenantName");
                string siteName = SettingsConfig.AppSetting("SiteName");
                string listName = SettingsConfig.AppSetting("ListName");

                string endpoint = "https://" + tenantName + ".sharepoint.com/sites/" + siteName + "/_api/web/lists/GetByTitle('" + listName + "')/items";

                string itemPostBody = "{" +
                    "'Description': '" + body.Description + "', " +
                    "'UserName':'" + body.Username + "'," +
                    "'Name':'" + body.Name + "'," +
                    "'Address':'" + body.Address + "'" +
                    "}";
                Byte[] itemPostData = System.Text.Encoding.ASCII.GetBytes(itemPostBody);

                HttpWebRequest itemRequest =
                    (HttpWebRequest)HttpWebRequest.Create(endpoint.ToString());
                itemRequest.Method = "POST";
                itemRequest.ContentLength = itemPostBody.Length;
                itemRequest.ContentType = "application/json";
                itemRequest.Accept = "application/json";
                itemRequest.Accept = "*/*";
                itemRequest.Headers.Add("Authorization", "Bearer " + accessToken);
                itemRequest.Headers.Add("Host", "avadheshftc.sharepoint.com");
                Stream itemRequestStream = itemRequest.GetRequestStream();

                itemRequestStream.Write(itemPostData, 0, itemPostData.Length);
                itemRequestStream.Close();

                HttpWebResponse itemResponse = (HttpWebResponse)itemRequest.GetResponse();
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public class TokenResponse
        {
            public string access_token { get; set; }
        }
    }
}