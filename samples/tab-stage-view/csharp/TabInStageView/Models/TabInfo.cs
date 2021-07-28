// <copyright file="TabInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabInStageView.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class TabInfo
    {
        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("websiteUrl")]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }
    }
}
