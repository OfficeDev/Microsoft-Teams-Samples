// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Model
{
    using Newtonsoft.Json;
    
    public class MeetingResource
    {
        /// <summary>
        /// The type of response.
        /// </summary>
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Id of meeting resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The type of meeting resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// The date and time of meeting resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "eventDateTime")]
        public string EventDateTime { get; set; }

        /// <summary>
        /// The state of meeting resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}
