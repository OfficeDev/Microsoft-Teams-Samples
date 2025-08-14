namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Newtonsoft.Json;

    /// <summary>
    /// User feeback entity class.
    /// </summary>
    public class FeedbackEntity : ITableEntity
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
        /// Gets or sets the feedback id .
        /// </summary>
        [Key]
        [JsonProperty("feedbackId")]
        public string FeedbackId
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
        /// Gets or sets id of feedback type to. 0 = GeneralFeedback, 1= LearningContentFeedback 
        /// </summary>
        [JsonProperty("feedbackType")]
        public FeedbackType FeedbackType { get; set; }

        /// <summary>
        /// Gets or sets title of learning content id.
        /// </summary>
        [JsonProperty("learningContentId")]
        public string LearningContentId { get; set; }

        /// <summary>
        /// Gets or sets helpfulStatus 0 = Super, 1= Medium  2 = not helpful
        /// </summary>
        [JsonProperty("helpfulStatus")]
        public FeedbackHelpfulStatus HelpfulStatus { get; set; }

        /// <summary>
        /// Gets or sets is helpful.
        /// </summary>
        [JsonProperty("isHelpful")]
        public bool IsHelpful { get; set; }

        /// <summary>
        /// Gets or sets rating 1 to 5.
        /// </summary>
        [JsonProperty("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets feedback .
        /// </summary>
        [JsonProperty("feedback")]
        public string Feedback { get; set; }

        // <summary>
        /// Gets or sets created on datetime in UTC.
        /// </summary>
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }

        // <summary>
        /// Gets or sets created by user AAD id.
        /// </summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
    }
}