using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Teams.TemplateBotCSharp
{
    /// <summary>
    /// This is Entity Class for VSTS Profile Information
    /// </summary>
    class VSTSProfile
    {
        public VSTSProfile()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Entity Class to store the Access Token for Auth Flow
    /// </summary>
    public class VSTSAcessToken
    {
        public VSTSAcessToken()
        {

        }

        [JsonProperty(PropertyName = "access_token")]
        public String accessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public String tokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public String expiresIn { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public String refreshToken { get; set; }

    }
    /// <summary>
    /// Entity Class to store the Work Item details
    /// </summary>
    public class WorkItem
    {
        public int Id { get; set; }
        public int Rev { get; set; }
        public string Title { get; set; }
        public string TeamProject { get; set; }
        public string Url { get; set; }
    }

    public static class VSTSHelpers
    {
        /// <summary>
        /// Execute the VSTS Api call
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="IsCallback"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static String PerformTokenRequest(String postData, bool IsCallback, out VSTSAcessToken token)
        {
            var error = String.Empty;
            var strResponseData = String.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(
                ConfigurationManager.AppSettings["TokenUrl"]
                );

            webRequest.Method = "POST";
            webRequest.ContentLength = postData.Length;
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter swRequestWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                swRequestWriter.Write(postData);
            }

            try
            {
                HttpWebResponse hwrWebResponse = (HttpWebResponse)webRequest.GetResponse();

                if (hwrWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader srResponseReader = new StreamReader(hwrWebResponse.GetResponseStream()))
                    {
                        strResponseData = srResponseReader.ReadToEnd();
                    }

                    token = JsonConvert.DeserializeObject<VSTSAcessToken>(strResponseData);
                    return null;
                }
            }
            catch (WebException wex)
            {
                error = Strings.VSTSApiRequestError + wex.Message;
            }
            catch (Exception ex)
            {
                error = Strings.VSTSApiIssue + ex.Message;
            }

            token = new VSTSAcessToken();
            return error;
        }

        /// <summary>
        /// Create authorization the url
        /// </summary>
        /// <param name="conversationReference"></param>
        /// <returns></returns>
        public static String GenerateAuthorizeUrl(ConversationReference conversationReference)
        {
            UriBuilder uriBuilder = new UriBuilder(ConfigurationManager.AppSettings["AuthUrl"]);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query ?? String.Empty);

            string stateData = JsonConvert.SerializeObject(conversationReference);

            queryParams["client_id"] = ConfigurationManager.AppSettings["AppId"];
            queryParams["response_type"] = "Assertion";
            queryParams["state"] = stateData;
            queryParams["scope"] = ConfigurationManager.AppSettings["Scope"];
            queryParams["redirect_uri"] = ConfigurationManager.AppSettings["CallbackUrl"];

            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.ToString();
        }

        /// <summary>
        /// Get the VSTS Profile
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<string> GetVSTSProfile(string accessToken)
        {
            var uri = GetUri("https://teamsbot.visualstudio.com/DefaultCollection/_apis/profile/profiles/me");
            var res = await VSTSRequestAPI(uri, accessToken);
            return res;
        }

        /// <summary>
        /// Get VSTS Work Item
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="workItemId"></param>
        /// <returns></returns>
        public static async Task<WorkItem> GetWorkItem(string accessToken, string workItemId)
        {
            WorkItem item = new WorkItem();
            var uri = GetUri("https://teamsbot.visualstudio.com/DefaultCollection/_apis/wit/workitems?",
            //Tuple.Create("access_token", accessToken));
            Tuple.Create("id", workItemId),
            Tuple.Create("api-version", "1.0"));

            var res = await VSTSRequestAPI(uri, accessToken);
            
            var jsonObject= JObject.Parse(res); // parse as array

            if (jsonObject != null)
            {
                item.Title = jsonObject["fields"]["System.Title"].ToString();
                item.TeamProject = jsonObject["fields"]["System.TeamProject"].ToString();
                item.Url = jsonObject["url"].ToString();
            }
            
            return item;
        }

        /// <summary>
        /// Get Create Api Post Data
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GenerateRequestPostData(string code)
        {
            return string.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                HttpUtility.UrlEncode(ConfigurationManager.AppSettings["AppSecret"]),
                HttpUtility.UrlEncode(code),
                ConfigurationManager.AppSettings["CallbackUrl"]
                );
        }

        /// <summary>
        /// Get the Refresh Token Api Post Data
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static string GenerateRefreshPostData(string refreshToken)
        {
            return string.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}",
                HttpUtility.UrlEncode(ConfigurationManager.AppSettings["AppSecret"]),
                HttpUtility.UrlEncode(refreshToken),
                ConfigurationManager.AppSettings["CallbackUrl"]
                );

        }

        /// <summary>
        /// Process the Api call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static async Task<T> VSTSRequest<T>(Uri uri)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(uri).ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the VSTS response.", ex);
            }
        }

        /// <summary>
        /// Process the Api call
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private static async Task<string> VSTSRequestAPI(Uri uri, string accessToken)
        {
            string json = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", "bearer " + accessToken);
                json = await client.GetStringAsync(uri).ConfigureAwait(false);
            }

            return json;
        }

        /// <summary>
        /// Create Uri
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        private static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var queryparam in queryParams)
            {
                queryString[queryparam.Item1] = queryparam.Item2;
            }

            var builder = new UriBuilder(endPoint);
            builder.Query = queryString.ToString();
            return builder.Uri;
        }
    }
}