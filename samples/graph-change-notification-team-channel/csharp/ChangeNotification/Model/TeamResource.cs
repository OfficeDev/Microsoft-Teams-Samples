namespace ChangeNotification.Model
{
    using Newtonsoft.Json;
    using System;

    public class TeamResource
    {
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// SubscriptionId of resource SubscriptionId.
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The type of resource ChangeType.
        /// </summary>
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        /// <summary>
        /// The type of resource Team DisplayName.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Custom property Datetime for change notification.
        /// </summary>
        public  DateTime? CreatedDate { get; set; }
    }
}
