// <copyright file="AzureSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Models.Configuration
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
    }
}
