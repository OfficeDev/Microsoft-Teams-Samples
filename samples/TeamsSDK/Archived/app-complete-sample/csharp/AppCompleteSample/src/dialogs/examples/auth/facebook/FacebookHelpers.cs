using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web;
using AppCompleteSample;
using System.Net.Http;

namespace AppCompleteSample
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
        /// <summary>
        /// Get the User Profile information using valid Access Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<FacebookProfile> GetFacebookProfileName(string accessToken, string FBProfileUrl)
        {
            var uri = GetUri(FBProfileUrl,
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
            return System.Net.WebUtility.UrlEncode(token);
        }

        public static string TokenDecoder(string token)
        {
            return System.Net.WebUtility.UrlDecode(token);
        }

        
    }
}