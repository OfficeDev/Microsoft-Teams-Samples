// <copyright file="IAppSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    /// <summary>
    /// App settings contract.
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// Gets Graph app Id.
        /// </summary>
        string GraphAppId { get; }

        /// <summary>
        /// Gets App base url.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Gets tenant Id.
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// Gets bot name.
        /// </summary>
        string BotName { get; }

        /// <summary>
        /// Gets bot app id.
        /// </summary>
        string BotAppId { get; }

        /// <summary>
        /// Gets bot app certificate name in keyvault.
        /// </summary>
        string BotCertificateName { get; }

        /// <summary>
        /// Gets graph connection name to get delegated user token.
        /// </summary>
        string GraphConnectionName { get; }

        /// <summary>
        /// Gets app id in catalog / app store.
        /// </summary>
        string CatalogAppId { get; }
    }
}
