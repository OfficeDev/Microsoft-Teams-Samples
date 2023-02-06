namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using Azure;
    using Azure.Data.Tables;
    using Newtonsoft.Json;

    /// <summary>
    /// Team entity class.
    /// </summary>
    public class TeamEntity : ITableEntity
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
        /// Gets or sets id of team id between user and bot.
        /// </summary>
        [JsonProperty("TeamId")]
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets id of team id between user and bot.
        /// </summary>
        [JsonProperty("TeamName")]
        public string TeamName { get; set; }

        /// <summary>
        /// Gets or sets id of channel id  between user and bot.
        /// </summary>
        [JsonProperty("ChannelId ")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets tenant id.
        /// </summary>
        [JsonProperty("TenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets date on which bot was installed for user.
        /// </summary>
        [JsonProperty("BotInstalledOn")]
        public DateTime BotInstalledOn { get; set; }
    }
}