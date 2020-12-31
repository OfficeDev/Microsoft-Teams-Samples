// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The RedditHttpClient is a TypedClient (https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#how-to-use-typed-clients-with-httpclientfactory)
    /// that wraps the necessary APIs from the Reddit API (https://www.reddit.com/dev/api).
    /// </summary>
    public sealed class RedditHttpClient
    {
        // Regular expression used for extracting the subreddit and 'thing id' from a reddit link.
        // https://www.reddit.com/dev/api#fullnames
        private static readonly Regex ParameterExtrator = new Regex(@"https?://\w*\.?reddit\.com\/r\/(?<subreddit>\w+)/comments/(?<id>\w+)");

        private readonly HttpClient httpClient;

        private readonly IRedditAuthenticator redditAuthenticator;

        private readonly IOptions<RedditOptions> options;

        private readonly ILogger<RedditHttpClient> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">The underlying HTTP client.</param>
        /// <param name="redditAuthenticator"> The authenticator service for reddit based on the call context. </param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public RedditHttpClient(
            HttpClient httpClient,
            IRedditAuthenticator redditAuthenticator,
            IOptions<RedditOptions> options,
            ILogger<RedditHttpClient> logger)
        {
            this.httpClient = httpClient;
            this.redditAuthenticator = redditAuthenticator;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Get the information about a post.
        /// </summary>
        /// <param name="postLink">The url to the reddit post.</param>
        /// <returns>A Task resolving to the reddit link model for the post.</returns>
        /// <exception cref="RedditUnauthorizedException"> Thrown when the call to Reddit API was unauthorized.</exception>
        /// <exception cref="RedditRequestException"> Thrown when the call to Reddit API was unsuccessful.</exception>
        /// <exception cref="ArgumentException"> Thrown when post link is malformed or not for a post.</exception>
        /// <remarks>
        /// See: https://www.reddit.com/dev/api#GET_api_info .
        /// </remarks>
        public async Task<RedditLinkModel> GetLinkAsync(string postLink)
        {
            (string subreddit, string id) = this.ExtractPostParameters(postLink);

            if (string.IsNullOrEmpty(subreddit))
            {
                throw new ArgumentException($"Unable to find subreddit in url: {postLink}", nameof(postLink));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"Unable to find 'thing-id' in url: {postLink}", nameof(postLink));
            }

            this.logger.LogInformation($"Extracted id: {id}, subreddit: {subreddit}");

            return await this.GetLinkModelAsync(subreddit: subreddit, thingId: id);
        }

        /// <summary>
        /// Get the information about a post.
        /// </summary>
        /// <param name="query"> The query string to search for.</param>
        /// <param name="pageToken">The last post of the previous result.</param>
        /// <returns>A Task resolving to the reddit link model for the post.</returns>
        /// <exception cref="RedditUnauthorizedException"> Thrown when the call to Reddit API was unauthorized.</exception>
        /// <exception cref="RedditRequestException"> Thrown when the call to Reddit API was unsuccessful.</exception>
        /// <exception cref="ArgumentException"> Thrown when post link is malformed or not for a post.</exception>
        /// <remarks>
        /// See: https://www.reddit.com/dev/api#GET_api_info .
        /// </remarks>
        public async Task<IEnumerable<RedditLinkModel>> SearchLinksAsync(string query, string pageToken = default)
        {
            query = query ?? throw new ArgumentNullException(query);

            var authToken = await this.redditAuthenticator.GetAccessTokenAsync();
            var parameters = new Dictionary<string, string>
            {
                { "q", query },
                { "type", "link" },
                { "include_facets", "true" },
            };
            var requestUrl = QueryHelpers.AddQueryString("https://oauth.reddit.com/r/all/search", parameters);

            HttpResponseMessage result;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue(this.options.Value.ClientUserAgent, "1.0"));

                result = await this.httpClient.SendAsync(request).ConfigureAwait(false);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                // throw an exception to cause the controller to get the user to log-in
                throw new RedditUnauthorizedException(result.ReasonPhrase);
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new RedditRequestException(result.ReasonPhrase);
            }

            string stringContent = await result.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            JObject root = JObject.Parse(stringContent);
            JArray posts = root?["data"]?["children"] as JArray;
            return posts.OfType<JObject>()
                .Select(o => o["data"])
                .OfType<JObject>()
                .Select(this.RedditLinkModelFromJObject);
        }

        private async Task<RedditLinkModel> GetLinkModelAsync(string subreddit, string thingId)
        {
            var authToken = await this.redditAuthenticator.GetAccessTokenAsync();

            HttpResponseMessage result;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/api/info?id=t3_{thingId}"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue(this.options.Value.ClientUserAgent, "1.0"));

                result = await this.httpClient.SendAsync(request).ConfigureAwait(false);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                // throw an exception to cause the controller to get the user to log-in
                throw new RedditUnauthorizedException(result.ReasonPhrase);
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new RedditRequestException(result.ReasonPhrase);
            }

            string stringContent = await result.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            JObject root = JObject.Parse(stringContent);
            JObject firstPost = root?["data"]?["children"]?[0]?["data"] as JObject;

            var redditLink = this.RedditLinkModelFromJObject(firstPost);

            return redditLink;
        }

        private RedditLinkModel RedditLinkModelFromJObject(JObject linkObject)
        {
            // Preview images are html encoded urls.
            string thumbnailUrl = WebUtility.HtmlDecode((string)linkObject?["preview"]?["images"]?[0]?["source"]?["url"]);

            return new RedditLinkModel
            {
                Id = (string)linkObject?["id"],
                Title = (string)linkObject?["title"],
                Score = (int)linkObject?["score"],
                Subreddit = (string)linkObject?["subreddit"],
                Thumbnail = thumbnailUrl,
                SelfText = (string)linkObject?["selftext"],
                NumComments = (int)linkObject?["num_comments"],
                Link = "https://www.reddit.com" + (string)linkObject?["permalink"],
                Author = (string)linkObject?["author"],
                Created = DateTimeOffset.FromUnixTimeSeconds((long)linkObject?["created_utc"]),
            };
        }

        private (string subreddit, string id) ExtractPostParameters(string postLink)
        {
            Match m = RedditHttpClient.ParameterExtrator.Match(postLink);
            string subreddit = m?.Groups?["subreddit"]?.Value;
            string id = m?.Groups?["id"]?.Value;
            return (subreddit, id);
        }
    }
}