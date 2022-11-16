// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Model.Configuration
{
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Gets or sets Microsoft Id.
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets Microsoft password.
        /// </summary>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets Microsoft tenant Id.
        /// </summary>
        public string MicrosoftAppTenantId { get; set; }

        /// <summary>
        /// Base URL where the application is hosted.
        /// </summary>
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// Base64 string self-signed certificate string.
        /// </summary>
        public string Base64EncodedCertificate { get; set; }

        /// <summary>
        ///  Custom certificate Id.
        /// </summary>
        public string EncryptionCertificateId { get; set; }

        /// <summary>
        /// Cretificate thumb print string.
        /// </summary>
        public string CertificateThumbprint { get; set; }
    }
}
