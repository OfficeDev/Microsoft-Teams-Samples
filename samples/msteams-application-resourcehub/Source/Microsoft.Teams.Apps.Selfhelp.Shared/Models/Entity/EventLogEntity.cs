namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Newtonsoft.Json;

    /// <summary>
    /// Event log entity class.
    /// </summary>
    public class EventLogEntity : ITableEntity
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
        /// Gets or sets the storage Etag.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the unique event id.
        /// </summary>
        [Key]
        [JsonProperty("eventId")]
        public string EventId
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
        /// Gets or sets Id of learning content id.
        /// </summary>
        [JsonProperty("learningContentId")]
        public string LearningContentId { get; set; }

        /// <summary>
        /// Gets or sets state of complet state 0 = line, 1= dislike.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        // <summary>
        /// Gets or sets last modified  datetime in UTC.
        /// </summary>
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }

        // <summary>
        /// Gets or sets assigned user AAD id.
        /// </summary>
        [JsonProperty("userAadId ")]
        public string UserAadId { get; set; }

        // <summary>
        /// Gets or sets the search key.
        /// </summary>
        [JsonProperty("searchkey ")]
        public string SearchKey { get; set; }

        // <summary>
        /// Gets or sets the tanant id.
        /// </summary>
        [JsonProperty("tenantId ")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the managers, JSON format value for all users.
        /// </summary>
        [JsonProperty("sharedToUserIds")]
        public string SharedToUserIds { get; set; }

        /// <summary>
        /// Gets or sets the shared to channel id URI.
        /// </summary>
        [JsonProperty("sharedToChannelIds")]
        public string SharedToChannelIds { get; set; }
    }
}