// <copyright file="AdaptiveCardAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace BotRequestApproval.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class AdaptiveCardAction<T>
    {
        /// <summary>
        /// Gets or sets id value of request title.
        /// </summary>
        [JsonProperty("requestTitle")]
        public string RequestTitle { get; set; }

        /// <summary>
        /// Gets or sets id value of request description.
        /// </summary>
        [JsonProperty("requestDescription")]
        public string RequestDescription { get; set; }

        /// <summary>
        /// Gets or sets id value of manager.
        /// </summary>
        [JsonProperty("people-picker")]
        public string ManagerDetails { get; set; }
    }
}
