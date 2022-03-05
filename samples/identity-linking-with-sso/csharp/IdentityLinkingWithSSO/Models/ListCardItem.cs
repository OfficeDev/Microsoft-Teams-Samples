

namespace IdentityLinkingWithSSO.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represent list card item model.
    /// </summary>
    public class ListCardItem
    {
        /// <summary>
        /// Gets or sets type of item on list card.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets unique id of the item on list card.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets title of the item on list card.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets icon of the item on list card.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets subtitle of the item on list card.
        /// </summary>
        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets tap property of card.
        /// </summary>
        [JsonProperty("tap")]
        public TapItem Tap { get; set; }
    }
}