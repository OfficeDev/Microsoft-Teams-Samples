namespace Microsoft.Teams.Selfhelp.Models.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class which represents bing search query object.
    /// </summary>
    public class SearchQuery
    {
        /// <summary>
        /// Gets or sets the query data details.
        /// </summary>
        [JsonProperty("query")]
        public string Query { get; set; }
    }
}