namespace Microsoft.Teams.Selfhelp.Models.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class which represent article request object.
    /// </summary>
    public class ShareArticleRequest
    {
        /// <summary>
        /// Gets or sets learning id.
        /// </summary>
        [JsonProperty("learningId")]
        public string LearningId { get; set; }

        /// <summary>
        /// Gets or sets the managers. JSON format value for all users.
        /// </summary>
        [JsonProperty("users")]
        public string Users { get; set; }

        /// <summary>
        /// Gets or sets team id.
        /// </summary>
        [JsonProperty("teamId")]
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets channel id.
        /// </summary>
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets is share to user.
        /// </summary>
        [JsonProperty("isShareToUser")]
        public bool IsShareToUser { get; set; }

        /// <summary>
        /// Gets or sets a message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}