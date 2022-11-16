namespace Microsoft.Teams.Selfhelp.Models.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class which represent Article search query.
    /// </summary>
    public class ArticleQuery
    {
        /// <summary>
        /// Gets or sets learning id.
        /// </summary>
        [JsonProperty("learningId")]
        public string LearningId { get; set; }

        /// <summary>
        /// Gets or sets article url.
        /// </summary>
        [JsonProperty("articleurl")]
        public string ArticleUrl { get; set; }
    }
}