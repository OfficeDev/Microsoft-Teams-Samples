namespace ChangeNotification.Model
{
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using System;

    public class ChannelResource
    {
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// SubscriptionId of notification.
        /// </summary
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The type of notification.
        /// </summary>
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        /// <summary>
        /// The date and time of resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "dataSignature")]
        public string DataSignature { get; set; }

        /// <summary>
        /// The Property resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        /// <summary>
        /// The DisplayName of resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// The webUrl of resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "webUrl")]
        public string webUrl { get; set; }

        /// <summary>
        /// The CreatedDate for change notification.
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
