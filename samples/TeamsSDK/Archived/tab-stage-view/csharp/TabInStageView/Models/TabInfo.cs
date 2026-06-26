// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TabInStageView.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Tab info model class.
    /// </summary>
    public class TabInfo
    {
        /// <summary>
        /// Gets or sets content url.
        /// </summary>
        [JsonProperty("contentUrl")]
        public required string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets website url.
        /// </summary>
        [JsonProperty("websiteUrl")]
        public required string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets name.
        /// </summary>
        [JsonProperty("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets entity id.
        /// </summary>
        [JsonProperty("entityId")]
        public required string EntityId { get; set; }
    }
}