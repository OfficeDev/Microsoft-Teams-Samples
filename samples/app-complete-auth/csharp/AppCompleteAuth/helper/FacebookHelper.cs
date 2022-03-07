using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AppCompleteAuth.helper
{
    /// <summary>
    /// Entity Class to store the User Profile information
    /// </summary>
    public class FacebookProfile
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "picture")]
        public Picture ProfilePicture { get; set; }

        public class Data
        {
            public string url { get; set; }
        }

        public class Picture
        {
            public Data data { get; set; }
        }
    }

    public class FacebookHelper
    {
        /// <summary>
        /// Get the User Profile information using valid Access Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<FacebookProfile> GetFacebookProfileName(string accessToken)
        {
            var uri = GetUri("https://graph.facebook.com/v2.6/me",
                Tuple.Create("fields", "name,picture"),
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
        public static async Task<T> FacebookRequest<T>(Uri uri)
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
                throw new ArgumentException("", ex);
            }
        }

        /// <summary>
        /// Helper method to create URL
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        public static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
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