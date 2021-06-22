// <copyright file="AzureAdOptions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// The Azure AD options class.
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets the application id as auth client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the application secret as auth client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public string TenantId { get; set; }
    }
}
