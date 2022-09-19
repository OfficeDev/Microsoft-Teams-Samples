namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.BingSearch
{
    using System.Net;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// Bing search result details class.
    /// </summary>
    public class BingSearch : IBingSearch
    {
        /// <summary>
        /// A set of key/value bing search configuration properties.
        /// </summary>
        private readonly IOptions<BingSearchSettings> bingSearchSettings;

        /// <summary>
        /// A set of values for logger object.
        /// </summary>
        private readonly ILogger<BingSearch> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingSearch"/> class.
        /// </summary>
        /// <param name="bingSearchSettings">Entity represents bing search settings.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public BingSearch(
            IOptions<BingSearchSettings> bingSearchSettings,
            ILogger<BingSearch> logger)
        {
            this.logger = logger;
            this.bingSearchSettings = bingSearchSettings ?? throw new ArgumentNullException(nameof(bingSearchSettings));
        }

        /// <summary>
        /// Get the bing search result details.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>Returns the all bing search details.</returns>
        public async Task<BingSearchResult> GetBingSearchResultsAsync(string query)
        {
            // Create a dictionary to store relevant headers
            Dictionary<string, string> relevantHeaders = new Dictionary<string, string>();

            Console.OutputEncoding = Encoding.UTF8;

            // Construct the URI of the search request
            string querystring = $"(site:support.microsoft.com OR site:techcommunity.microsoft.com){query}";
            var uriQuery = $"{this.bingSearchSettings.Value.BingSearchEndpoint}/v7.0/search?q={Uri.EscapeDataString(querystring)}&count=50";

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = this.bingSearchSettings.Value.BingSearchSubscriptionKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Extract Bing HTTP headers
            foreach (string header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                {
                    relevantHeaders[header] = response.Headers[header];
                }
            }

            BingSearchResult searchResult = JsonConvert.DeserializeObject<BingSearchResult>(json);

            return searchResult;
        }
    }
}