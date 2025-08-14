namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Newtonsoft.Json;

    /// <summary>
    /// Learning path content entity class.
    /// </summary>
    public class LearningPathEntity : ITableEntity
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the storage Etag.
        /// </summary>
        [JsonProperty("eTag")]
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        [Key]
        [JsonProperty("learningPathId")]
        public string LearningPathId
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
        /// Gets or sets id of learning content id.
        /// </summary>
        [JsonProperty("learningContentId")]
        public string LearningContentId { get; set; }

        /// <summary>
        /// Gets or sets state of complet state 0 = not started, 1= in progress, 2 = completed.
        /// </summary>
        [JsonProperty("completeState")]
        public CompleteState CompleteState { get; set; }

        // <summary>
        /// Gets or sets last modified  datetime in UTC.
        /// </summary>
        [JsonProperty("lastModifiedOn ")]
        public DateTime LastModifiedOn { get; set; }

        // <summary>
        /// Gets or sets assigned user AAD id.
        /// </summary>
        [JsonProperty("userAadId ")]
        public string UserAadId { get; set; }
    }
}