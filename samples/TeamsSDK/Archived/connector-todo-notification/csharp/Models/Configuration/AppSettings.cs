namespace TeamsToDoAppConnector.Models.Configuration
{
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets BaseUrl.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets Connector Id.
        /// </summary>
        public string? ConnectorAppId { get; set; }

        /// <summary>
        /// Gets or sets Tenant Id for single-tenant configuration.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Host suffixes that a webhook destination is allowed to target. Used to
        /// prevent Server-Side Request Forgery (SSRF) by restricting outbound calls
        /// to trusted Microsoft Teams / Office 365 connector endpoints only.
        /// </summary>
        public string[]? AllowedWebhookHostSuffixes { get; set; }
    }
}
