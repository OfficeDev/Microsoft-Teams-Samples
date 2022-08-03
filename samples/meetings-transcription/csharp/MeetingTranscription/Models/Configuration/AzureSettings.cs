// <copyright file="AzureSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscription.Models.Configuration
{
    public class AzureSettings
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
        /// App base Url.
        /// </summary>
        public string AppBaseUrl { get; set; }

        /// <summary>
        /// Id of User for which policy is granted.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Graph API endpoint.
        /// </summary>
        public string GraphApiEndpoint { get; set; }
    }
}
