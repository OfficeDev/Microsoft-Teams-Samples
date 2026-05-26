// <copyright file="ServiceClients.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Helpers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Service client class.
    /// </summary>
    public static class ServiceClients
    {
        /// <summary>
        /// Creates a Graph service client for Graph API calls.
        /// </summary>
        /// <param name="graphAccessToken">Represents the access token used to access Graph API.</param>
        /// <returns>A Graph Service Client.</returns>
        public static GraphServiceClient GetGraphServiceClient(string graphAccessToken)
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
            {
                // Append the access token to the request.
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", graphAccessToken);

                // Get event times in the current time zone.
                requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                return Task.CompletedTask;
            }));

            return graphClient;
        }

        /// <summary>
        /// Creates a HTTP client for general API requests.
        /// </summary>
        /// <param name="accessToken">Represents the access token to use for the client.</param>
        /// <param name="responseMediaType">Represents the response media type.</param>
        /// <param name="httpClientFactory">Represents the HTTP client factory.</param>
        /// <returns>An HTTP Client.</returns>
        public static HttpClient GetHttpClient(string accessToken, string responseMediaType, IHttpClientFactory httpClientFactory)
        {
            HttpClient client = httpClientFactory.CreateClient();

            client.BaseAddress = new Uri("https://graph.microsoft.com");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            if (responseMediaType != null)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(responseMediaType));
            }

            return client;
        }
    }
}
