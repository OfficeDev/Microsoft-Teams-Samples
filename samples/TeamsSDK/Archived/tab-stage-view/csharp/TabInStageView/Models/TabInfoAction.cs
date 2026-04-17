// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TabInStageView.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Tab info action model class.
    /// </summary>
    public class TabInfoAction
    {
        /// <summary>
        /// Gets or sets type of tab.
        /// </summary>
        [JsonProperty("type")]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets tab info.
        /// </summary>
        [JsonProperty("tabInfo")]
        public required TabInfo TabInfo { get; set; }

        /// <summary>
        /// Gets or sets tab info.
        /// </summary>
        [JsonProperty("threadId")]
        public string? ThreadId { get; set; } = string.Empty;
    }
}