// <copyright file="AzureSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace meetings_transcription.Models.Configuration
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
        /// Id of User for which policy is granted (UPN/email).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Azure AD User ID (GUID) - if provided, skips UPN resolution.
        /// </summary>
        public string AzureAdUserId { get; set; }

        /// <summary>
        /// Graph API endpoint.
        /// </summary>
        public string GraphApiEndpoint { get; set; }
    }
}
