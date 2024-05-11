// <copyright file="TabInfoAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabInStageView.Models
{
    using Newtonsoft.Json;
    using System;

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

        /// <summary>
        /// Gets or sets tab info.
        /// </summary>
        [JsonProperty("threadId")]
        public string? ThreadId { get; set; } = string.Empty;

        public TabInfoAction()
        {
            ThreadId = "";
        }
    }
}