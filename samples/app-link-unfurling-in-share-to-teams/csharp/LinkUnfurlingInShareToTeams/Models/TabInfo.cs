// <copyright file="TabInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace LinkUnfurlingInShareToTeams.Models
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
        public string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets website url.
        /// </summary>
        [JsonProperty("websiteUrl")]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets entity id.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }
    }
}