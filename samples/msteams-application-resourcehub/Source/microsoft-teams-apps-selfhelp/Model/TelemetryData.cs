namespace Microsoft.Teams.Selfhelp.Authentication.Model
{
    /// <summary>
    /// Telemetry data model class.
    /// </summary>
    public class TelemetryData
    {
        /// <summary>
        /// Gets or sets article title.
        /// </summary>
        public string ArticleTitle { get; set; }

        /// <summary>
        /// Gets or sets total view count.
        /// </summary>
        public int TotalViewCount { get; set; }

        /// <summary>
        /// Gets or sets toal like count.
        /// </summary>
        public int TotalLikeCount { get; set; }

        /// <summary>
        /// Gets or sets total dislike count.
        /// </summary>
        public int TotalDislikeCount { get; set; }

        /// <summary>
        /// Gets or sets item type.
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// Gets or sets article shared to user.
        /// </summary>
        public int ShareArticleToUser { get; set; }

        /// <summary>
        /// Gets or sets article shared to channel.
        /// </summary>
        public int ShareArticleToChannel { get; set; }
    }
}