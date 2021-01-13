// <copyright file="PostNotificationServiceRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using Newtonsoft.Json;

    /// <summary>
    /// Post token status change model.
    /// </summary>
    public class PostNotificationServiceRequest
    {
        /// <summary>
        /// Gets or sets a value indicating the type of activity.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the text message for the activity.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the ChannelData for the notification request.
        /// </summary>
        [JsonProperty("channelData")]
        public ChannelData ChannelData { get; set; }
    }
}