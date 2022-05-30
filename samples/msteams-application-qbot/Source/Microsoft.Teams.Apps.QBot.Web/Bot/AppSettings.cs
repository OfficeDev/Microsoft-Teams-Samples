namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;

    /// <summary>
    /// App Settings.
    /// </summary>
    public sealed class AppSettings : IAppSettings
    {
        /// <summary>
        /// Gets or sets App base url.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets App Id.
        /// </summary>
        public string GraphAppId { get; set; }

        /// <summary>
        /// Gets or sets tenant Id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets bot name.
        /// </summary>
        public string BotName { get; set; }

        /// <summary>
        /// Gets or sets bot app id.
        /// </summary>
        public string BotAppId { get; set; }

        /// <summary>
        /// Gets or sets bot app certificate name in keyvault.
        /// </summary>
        public string BotCertificateName { get; set; }

        /// <summary>
        /// Gets or sets app id in the manifest.
        /// </summary>
        public string ManifestAppId { get; set; }
    }
}
