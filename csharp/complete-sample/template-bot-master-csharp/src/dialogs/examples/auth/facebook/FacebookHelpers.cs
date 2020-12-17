using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Teams.TemplateBotCSharp
{

    /// <summary>
    /// Entity Class to store the Access Token for Auth Flow
    /// </summary>
    public class FacebookAcessToken
    {
        public FacebookAcessToken()
        {
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public long ExpiresIn { get; set; }
    }

    /// <summary>
    /// Entity Class to store the User Profile information
    /// </summary>
    public class FacebookProfile
    {
        public FacebookProfile()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "picture")]
        public Picture ProfilePicture { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string link { get; set; }

        public class Data
        {
            public bool is_silhouette { get; set; }
            public string url { get; set; }
        }

        public class Picture
        {
            public Data data { get; set; }
        }
    }

    /// <summary>
    /// Helper class for implementing Facebook API calls.
    /// </summary>
    public static class FacebookHelpers
    {
        // The Facebook App Id
        public static readonly string FacebookAppId = ConfigurationManager.AppSettings["FBAppId"].ToString();

        // The Facebook App Secret
        public static readonly string FacebookAppSecret = ConfigurationManager.AppSettings["FBAppSecret"].ToString();

        /// <summary>
        /// Get the Authentication Token from Api code
        /// </summary>
        /// <param name="conversationReference"></param>
        /// <param name="code"></param>
        /// <param name="facebookOauthCallback"></param>
        /// <returns></returns>
        public async static Task<FacebookAcessToken> ExchangeCodeForAccessToken(ConversationReference conversationReference, string code, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(conversationReference, facebookOauthCallback);
            var uri = GetUri(ConfigurationManager.AppSettings["FBTokenUrl"].ToString(),
                Tuple.Create("client_id", FacebookAppId),
                Tuple.Create("redirect_uri", redirectUri),
                Tuple.Create("client_secret", FacebookAppSecret),
                Tuple.Create("code", code)
                );

            return await FacebookRequest<FacebookAcessToken>(uri);
        }

        /// <summary>
        /// Validate the Access Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<bool> ValidateAccessToken(string accessToken)
        {
            var uri = GetUri(ConfigurationManager.AppSettings["FBDebugUrl"].ToString(),
                Tuple.Create("input_token", accessToken),
                Tuple.Create("access_token", $"{FacebookAppId}|{FacebookAppSecret}"));

            var res = await FacebookRequest<object>(uri).ConfigureAwait(false);
            return (((dynamic)res)?.data)?.is_valid;
        }

        /// <summary>
        /// Get the User Profile information using valid Access Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<FacebookProfile> GetFacebookProfileName(string accessToken)
        {
            var uri = GetUri(ConfigurationManager.AppSettings["FBProfileUrl"].ToString(),
                Tuple.Create("fields", "id,name,gender,picture,link"),
                Tuple.Create("access_token", accessToken));

            var res = await FacebookRequest<FacebookProfile>(uri);

            return res;
        }

        /// <summary>
        /// Get the Profile Picture detail using valid Access Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<FacebookProfile> GetFacebookProfilePicture(string accessToken)
        {
            var uri = GetUri("https://graph.facebook.com/v2.10/me/picture?width=200&height=200",
                Tuple.Create("access_token", accessToken));
            var res = await FacebookRequest<FacebookProfile>(uri);
            return res;
        }

        /// <summary>
        /// Create the Auth URL    
        /// </summary>
        /// <param name="conversationReference"></param>
        /// <param name="facebookOauthCallback"></param>
        /// <returns></returns>
        private static string GetOAuthCallBack(ConversationReference conversationReference, string facebookOauthCallback)
        {
            var uri = GetUri(facebookOauthCallback,
                Tuple.Create("userId", TokenEncoder(conversationReference.User.Id)),
                Tuple.Create("botId", TokenEncoder(conversationReference.Bot.Id)),
                Tuple.Create("conversationId", TokenEncoder(conversationReference.Conversation.Id)),
                Tuple.Create("serviceUrl", TokenEncoder(conversationReference.ServiceUrl)),
                Tuple.Create("channelId", conversationReference.ChannelId)
                );
            return uri.ToString();
        }

        /// <summary>
        /// Create Facebook Login URL
        /// </summary>
        /// <param name="conversationReference"></param>
        /// <param name="facebookOauthCallback"></param>
        /// <returns></returns>
        public static string GetFacebookLoginURL(ConversationReference conversationReference, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(conversationReference, facebookOauthCallback);
            var uri = GetUri(ConfigurationManager.AppSettings["FBAuthUrl"].ToString(),
                Tuple.Create("client_id", FacebookAppId),
                Tuple.Create("redirect_uri", redirectUri),
                Tuple.Create("response_type", "code"),
                Tuple.Create("scope", "public_profile,email"),
                Tuple.Create("state", Convert.ToString(new Random().Next(9999)))
                );

            return uri.ToString();
        }

        /// <summary>
        /// Purpose of this request is to process the Api Calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static async Task<T> FacebookRequest<T>(Uri uri)
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
                throw new ArgumentException(Strings.FBAuthDeserializeError, ex);
            }
        }

        /// <summary>
        /// Helper method to create URL
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

        // because of a limitation on the characters in Facebook redirect_uri, we don't use the serialization of the cookie.
        // http://stackoverflow.com/questions/4386691/facebook-error-error-validating-verification-code
        public static string TokenEncoder(string token)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(token));
        }

        public static string TokenDecoder(string token)
        {
            return Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(token));
        }

        
    }
}