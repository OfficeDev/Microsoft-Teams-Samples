namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
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
        /// Gets app id in manifest.
        /// </summary>
        string ManifestAppId { get; }
    }
}