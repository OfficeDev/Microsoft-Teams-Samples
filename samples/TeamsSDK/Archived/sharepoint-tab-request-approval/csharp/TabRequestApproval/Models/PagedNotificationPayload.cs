// <copyright file="PagedNotificationPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Paged notification payload.
    /// </summary>
    public class PagedNotificationPayload
    {
        /// <summary>
        /// Gets or sets the collection of notifications.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<NotificationPayload> Value { get; set; }

        /// <summary>
        /// Gets or sets the validation tokens.
        /// </summary>
        [JsonProperty(PropertyName = "validationTokens")]
        public IEnumerable<string> ValidationTokens { get; set; }
    }
}