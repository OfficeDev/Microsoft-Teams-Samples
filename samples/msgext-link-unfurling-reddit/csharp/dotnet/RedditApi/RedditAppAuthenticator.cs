// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The RedditAppAuthenticator is responsible for getting application-only tokens for
    /// api.reddit.com.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/reddit-archive/reddit/wiki/OAuth2#application-only-oauth .
    /// </remarks>
    public sealed class RedditAppAuthenticator : IRedditAuthenticator
    {
        private const string CacheKey = "RedditAppToken";

        private readonly string redditAppId;

        private readonly string redditAppPassword;

        private readonly HttpClient httpClient;

        private readonly IDistributedCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditAppAuthenticator"/> class.
        /// </summary>
        /// <param name="cache">The distributed cache for storing tokens between requests.</param>
        /// <param name="httpClient">The underlying http client.</param>
        /// <param name="options">The configuration options.</param>
        public RedditAppAuthenticator(
            IDistributedCache cache,
            HttpClient httpClient,
            IOptions<RedditOptions> options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            this.redditAppId = options.Value.AppId;
            this.redditAppPassword = options.Value.AppPassword;

            this.cache = cache;
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Get an access token given the current call context.
        /// </summary>
        /// <returns>A task resolving to a token.</returns>
        public async Task<string> GetAccessTokenAsync()
        {
            string accessToken = await this.cache.GetStringAsync(RedditAppAuthenticator.CacheKey);
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }

            JObject contentJObject;
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token"))
            {
                // Configure the authentication per
                // https://github.com/reddit-archive/reddit/wiki/OAuth2#application-only-oauth
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.redditAppId}:{this.redditAppPassword}")));

                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "read" },
                });

                var response = await this.httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RedditUnauthorizedException($"Unable to fetch token for api.reddit.com: ${response.ReasonPhrase}");
                }

                var contentString = await response.Content.ReadAsStringAsync();
                contentJObject = JObject.Parse(contentString);
            }

            accessToken = contentJObject["access_token"].ToString();

            await this.cache.SetStringAsync(RedditAppAuthenticator.CacheKey, accessToken, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30), // Duration is 1hr, store in cache for half-life.
            });

            return accessToken;
        }
    }
}