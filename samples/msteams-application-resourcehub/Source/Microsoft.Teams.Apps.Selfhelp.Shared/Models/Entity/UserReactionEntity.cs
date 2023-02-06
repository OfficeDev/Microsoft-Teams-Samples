namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Newtonsoft.Json;

    /// <summary>
    /// User reaction entity details class.
    /// </summary>
    public class UserReactionEntity : ITableEntity
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets storage Etag.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        [Key]
        [JsonProperty("reactionId")]
        public string ReactionId
        {
            get
            {
                return this.RowKey;
            }

            set
            {
                this.RowKey = value;
            }
        }

        /// <summary>
        /// Gets or sets id of learning content.
        /// </summary>
        [JsonProperty("learningContentId")]
        public string LearningContentId { get; set; }

        /// <summary>
        /// Gets or sets state of complet state 0 = line, 1= dislike.
        /// </summary>
        [JsonProperty("reactionState")]
        public ReactionState ReactionState { get; set; }

        // <summary>
        /// Gets or sets last modified  datetime in UTC.
        /// </summary>
        [JsonProperty("lastModifiedOn")]
        public DateTime LastModifiedOn { get; set; }

        // <summary>
        /// Gets or sets assigned user AAD id.
        /// </summary>
        [JsonProperty("userAadId ")]
        public string UserAadId { get; set; }
    }
}