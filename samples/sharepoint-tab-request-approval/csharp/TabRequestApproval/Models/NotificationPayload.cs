// <copyright file="NotificationPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Graph;
    using Newtonsoft.Json;

    /// <summary>
    /// Received notification payload.
    /// </summary>
    public class NotificationPayload
    {
        /// <summary>
        /// Gets or sets the subscription Id.
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the resource change type (created, updated, deleted).
        /// </summary>
        [JsonProperty(PropertyName = "changeType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle event type (e.g., subscriptionRemoved).
        /// </summary>
        [JsonProperty(PropertyName = "lifecycleEvent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LifecycleEvent { get; set; }

        /// <summary>
        /// Gets or sets the Id of the tenant for which notification was sent.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the client state set during subscription creation.
        /// </summary>
        [JsonProperty(PropertyName = "clientState")]
        public string ClientState { get; set; }

        /// <summary>
        /// Gets or sets the subscription expiration time.
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
        public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the resource for which notification was sent.
        /// </summary>
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        /// <summary>
        /// Gets the resource data for the notification (used in non-rich notifications).
        /// </summary>
        [JsonProperty(PropertyName = "resourceData")]
        public IDictionary<string, object> ResourceData { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the decrypted content.
        /// </summary>
        [JsonProperty(PropertyName = "contentDecryptedBySimulator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object DecryptedContent { get; internal set; }

        /// <summary>
        /// Gets or sets the encrypted content (for rich notifications).
        /// </summary>
        [JsonProperty(PropertyName = "encryptedContent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public NotificationEncryptedContent EncryptedContent { get; set; }
    }
}
