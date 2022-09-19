namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Newtonsoft.Json;

    /// <summary>
    /// Learning content article entity class.
    /// This entity holds the information about a new learning content request and response.
    /// </summary>
    public class ArticleEntity : ITableEntity
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
        /// Gets or sets timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets storage Etag.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets the unique id of each learning content.
        /// </summary>
        [Key]
        [JsonProperty("learningId")]
        public string LearningId
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
        /// Gets or sets id of selection type to learning content. 0 = GettingStarted, 1= Scenarios, 2 = Trending Now.
        /// </summary>
        [JsonProperty("sectionType")]
        public SelectionType SectionType { get; set; }

        /// <summary>
        /// Gets or sets title of learning content.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        // <summary>
        /// Gets or sets description of learning content.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets id of item type to learning content.0 = video, 1= Image, 2= Search Result, 3 = Articles.
        /// </summary>
        [JsonProperty("itemType")]
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets id of source of learning content. 0 = Internal, 1= external.
        /// </summary>
        [JsonProperty("source")]
        public SourceType Source { get; set; }

        /// <summary>
        /// Gets or sets primary tags.
        /// </summary>
        [JsonProperty("primaryTag")]
        public string PrimaryTag { get; set; }

        /// <summary>
        /// Gets or sets secondary tags .
        /// </summary>
        [JsonProperty("secondaryTag")]
        public string SecondaryTag { get; set; }

        /// <summary>
        /// Gets or sets external url of article or video.
        /// </summary>
        [JsonProperty("itemlink")]
        public string Itemlink { get; set; }

        /// <summary>
        /// Gets or sets know more link external web url in string .
        /// </summary>
        [JsonProperty("knowmoreLink")]
        public string KnowmoreLink { get; set; }

        /// <summary>
        /// Gets or sets length of article to read.
        /// </summary>
        [JsonProperty("length")]
        public string Length { get; set; }

        /// <summary>
        /// Gets or sets tile image link image link used in video/article thumbnail.
        /// </summary>
        [JsonProperty("tileImageLink")]
        public string TileImageLink { get; set; }

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