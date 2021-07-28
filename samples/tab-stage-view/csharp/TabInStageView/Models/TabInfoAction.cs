// <copyright file="TabInfoAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabInStageView.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class TabInfoAction
    {
        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("tabInfo")]
        public TabInfo TabInfo { get; set; }
    }
}
