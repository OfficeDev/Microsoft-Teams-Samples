// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


namespace ChangeNotification.Model
{
    using Newtonsoft.Json;
    using System;

    public class Notifications
    {
        [JsonProperty(PropertyName = "value")]
        public NotificationResponseData[] Items { get; set; }
    }

    public class NotificationResponseData
    {
        // The type of change.
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
        [JsonProperty(PropertyName = "clientState")]
        public string ClientState { get; set; }

        // The endpoint of the resource that changed. For example, a message uses the format ../Users/{user-id}/Messages/{message-id}
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        // The UTC date and time when the webhooks subscription expires.
        [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
        public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

        // The unique identifier for the webhooks subscription.
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        // Properties of the changed resource.
        [JsonProperty(PropertyName = "resourceData")]
        public ResourceData ResourceData { get; set; }

        [JsonProperty(PropertyName = "encryptedContent")]
        public Encryptedcontent EncryptedContent { get; set; }
    }
}
