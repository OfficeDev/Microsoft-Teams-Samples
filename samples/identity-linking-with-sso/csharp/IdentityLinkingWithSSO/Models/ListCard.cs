
namespace IdentityLinkingWithSSO.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represents list card model.
    /// </summary>
    public class ListCard
    {
        /// <summary>
        /// Gets or sets title of list card.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets items of list card.
        /// </summary>
        [JsonProperty("items")]
        public List<ListCardItem> Items { get; set; }
    }
}