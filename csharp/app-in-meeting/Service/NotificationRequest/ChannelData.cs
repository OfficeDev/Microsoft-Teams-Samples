// <copyright file="ChannelData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using Newtonsoft.Json;

    /// <summary>
    /// Post token status change model.
    /// </summary>
    public class ChannelData
    {
        /// <summary>
        /// Gets or sets a value indicating the notification object for the request.
        /// </summary>
        [JsonProperty("notification")]
        public Notification Notification { get; set; }
    }
}