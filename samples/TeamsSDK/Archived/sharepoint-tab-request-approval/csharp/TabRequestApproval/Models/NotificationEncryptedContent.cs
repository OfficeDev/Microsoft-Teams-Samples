// <copyright file="NotificationEncryptedContent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Change notification encrypted content.
    /// </summary>
    public class NotificationEncryptedContent
    {
        /// <summary>
        /// Gets or sets the encrypted data.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the data signature.
        /// </summary>
        [JsonProperty(PropertyName = "dataSignature")]
        public string DataSignature { get; set; }

        /// <summary>
        /// Gets or sets the encrypted data key used to encrypt the data.
        /// </summary>
        [JsonProperty(PropertyName = "dataKey")]
        public string DataKey { get; set; }

        /// <summary>
        /// Gets or sets the encryption certificate Id used to encrypt data key.
        /// </summary>
        [JsonProperty(PropertyName = "encryptionCertificateId")]
        public string EncryptionCertificateId { get; set; }

        /// <summary>
        /// Gets or sets the encryption certificate thumbprint used to encrypt data key.
        /// </summary>
        [JsonProperty(PropertyName = "encryptionCertificateThumbprint")]
        public string EncryptionCertificateThumbprint { get; set; }
    }
}
