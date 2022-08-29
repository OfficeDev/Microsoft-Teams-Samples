namespace ChangeNotification.Model
{
    using Newtonsoft.Json;
    using System;

    public class TeamResource
    {
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }
        /// <summary>
        /// Id of meeting resource notification.
        /// </summary>
       
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The type of meeting resource notification.
        /// </summary>
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
        public  DateTime? CreatedDate { get; set; }
    }
}
