namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Newtonsoft.Json;

    /// <summary>
    /// User entity details class.
    /// </summary>
    public class UserEntity : ITableEntity
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string PartitionKey { get; set ; }

        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        public string RowKey { get ; set ; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the storage Etag.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the user object id.
        /// </summary>
        [Key]
        [JsonProperty("UserId")]
        public string UserId
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
        /// Gets or sets id of conversation between user and bot.
        /// </summary>
        [JsonProperty("ConversationId")]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        [JsonProperty("ServiceUrl")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets date on which bot was installed for user.
        /// </summary>
        [JsonProperty("BotInstalledOn")]
        public DateTime BotInstalledOn { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        // <summary>
        /// Gets or sets created on datetime in UTC.
        /// </summary>
        [JsonProperty("Status")]
        public bool Status { get; set; }

        // <summary>
        /// Gets or sets created on datetime in UTC.
        /// </summary>
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }
    }
}