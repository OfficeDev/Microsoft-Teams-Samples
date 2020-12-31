// <copyright file="Notification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using Newtonsoft.Json;

    /// <summary>
    /// Post token status change model.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets a value indicating whether the meeting should get this alert.
        /// </summary>
        [JsonProperty("alertInMeeting")]
        public bool AlertInMeeting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating resource to be rendered in the content bubble.
        /// </summary>
        [JsonProperty("externalResourceUrl")]
        public string ExternalResourceUrl { get; set; }
    }
}