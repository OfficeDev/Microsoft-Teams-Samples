namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity
{
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Newtonsoft.Json;

    /// <summary>
    /// Checked learning content article entity class.
    /// </summary>
    public class ArticleCheckBoxEntity : ITableEntity
    {
        /// <summary>
        /// Gets or sets id of selection type to learning content, 0 = GettingStarted, 1= Scenarios, 2 = Trending Now.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the rowKey.
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
        /// Gets or sets the unique learning content id.
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
        /// Gets or sets comma separated tasgs upto 3 .
        /// </summary>
        [JsonProperty("primaryTag")]
        public string PrimaryTag { get; set; }

        /// <summary>
        /// Gets or sets comma separated tasgs upto 3.
        /// </summary>
        [JsonProperty("secondaryTag")]
        public string SecondaryTag { get; set; }

        /// <summary>
        /// Gets or sets item link url.
        /// </summary>
        [JsonProperty("itemlink")]
        public string Itemlink { get; set; }

        /// <summary>
        /// Gets or sets know more link url external web url in string .
        /// </summary>
        [JsonProperty("knowmoreLink")]
        public string KnowmoreLink { get; set; }

        /// <summary>
        /// Gets or sets length to read the article.
        /// </summary>
        [JsonProperty("length")]
        public string? Length { get; set; }

        /// <summary>
        /// Gets or sets tile image link image link used in video thumbnail.
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

        // <summary>
        /// Gets or sets flag if article selected for share.
        /// </summary>
        [JsonProperty("isChecked")]
        public Boolean IsChecked { get; set; }
    }
}