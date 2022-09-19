namespace Microsoft.Teams.Selfhelp.Models.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class which represent article response object.
    /// </summary>
    public class ArticleHtmlResponse
    {
        /// <summary>
        /// Gets or sets published by.
        /// </summary>
        [JsonProperty("publishedby")]
        public string PublishedBy { get; set; }

        /// <summary>
        /// Gets or sets published on.
        /// </summary>
        [JsonProperty("publishedon")]
        public string PublishedOn { get; set; }

        /// <summary>
        /// Gets or sets html details.
        /// </summary>
        [JsonProperty("html")]
        public string Html { get; set; }
    }
}