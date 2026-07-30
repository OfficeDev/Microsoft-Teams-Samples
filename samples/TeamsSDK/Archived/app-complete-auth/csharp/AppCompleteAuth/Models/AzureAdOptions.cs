// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AppCompleteAuth.Models
{
    /// <summary>
    /// Azure AD configuration options.
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets the Azure AD instance URL.
        /// </summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Microsoft App ID.
        /// </summary>
        public string MicrosoftAppId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Microsoft App Password.
        /// </summary>
        public string MicrosoftAppPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Application ID URI.
        /// </summary>
        public string ApplicationIdURI { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Auth URL.
        /// </summary>
        public string AuthUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the valid issuers.
        /// </summary>
        public string ValidIssuers { get; set; } = string.Empty;
    }
}
