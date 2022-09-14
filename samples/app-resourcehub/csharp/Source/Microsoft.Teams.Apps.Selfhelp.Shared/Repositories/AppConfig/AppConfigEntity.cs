namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Newtonsoft.Json;

    /// <summary>
    /// Application configuration entity class.
    /// </summary>
    public class AppConfigEntity : ITableEntity
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
        /// Gets or sets the the service url.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        [Key]
        [JsonProperty("id")]
        public string Id
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
    }
}