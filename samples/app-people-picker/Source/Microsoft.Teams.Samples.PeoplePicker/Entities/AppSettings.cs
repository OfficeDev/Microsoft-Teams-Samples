// <copyright file="AppSettings.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Entities
{
    using Microsoft.Extensions.Options;

    /// <summary>
    /// App settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Teams Bot.
        /// </summary>
        public const string AzureAd = "AzureAd";

        /// <summary>
        /// Teams Bot.
        /// </summary>
        public const string TeamsBot = "TeamsBot";

        /// <summary>
        /// Graph Api.
        /// </summary>
        public const string GraphApi = "GraphApi";

        /// <summary>
        /// Gets or sets ClientId.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets TeamsBot : CatalogAppId.
        /// </summary>
        public string CatalogAppId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets GraphApi : BaseUrl.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }
}
