// <copyright file="TabInfoAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace LinkUnfurlingInShareToTeams.Models
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
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets tab info.
        /// </summary>
        [JsonProperty("tabInfo")]
        public TabInfo TabInfo { get; set; }
    }
}