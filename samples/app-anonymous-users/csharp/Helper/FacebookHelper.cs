// <copyright file="FacebookHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AnonymousUsers.Helper
{
    /// <summary>
    /// Entity Class to store the User Profile information
    /// </summary>
    public class FacebookProfile
    {
        /// <summary>
        /// Assign facebook username name value in this Name property
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Assign facebook picture in this ProfilePicture property
        /// </summary>
        [JsonProperty(PropertyName = "picture")]
        public Picture ProfilePicture { get; set; }

        /// <summary>
        /// Create a class named "Data" with a property url
        /// Assgin facebook image in this url property. 
        /// </summary>
        public class Data
        {
            public string url { get; set; }
        }

        /// <summary>
        /// Create a class named "Picture" with a property data
        /// Aassgin facebook Picture information in this data property. 
        /// </summary>
        public class Picture
        {
            public Data data { get; set; }
        }
    }

    /// <summary>
    /// Retrieved from facebook profile information after a successful authentication process.
    /// </summary>
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

            var userProfileInfo = await FacebookRequest<FacebookProfile>(uri);

            return userProfileInfo;
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