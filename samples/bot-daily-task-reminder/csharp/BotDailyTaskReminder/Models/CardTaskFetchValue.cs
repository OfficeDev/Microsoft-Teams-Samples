// <copyright file="CardTaskFetchValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace BotDailyTaskReminder.Models
{
    /// <summary>
    /// Card task fetch value model class.
    /// </summary>
    public class CardTaskFetchValue<T>
    {
        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("type")]
        public object Type { get; set; } = "task/fetch";

        /// <summary>
        /// Gets or sets id value of turncontext activity.
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// Gets or sets data value.
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}