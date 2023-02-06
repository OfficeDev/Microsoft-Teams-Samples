namespace Microsoft.Teams.Selfhelp.Authentication
{
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// A class which helps to provide Azure settings for application.
    /// </summary>
    public class AzureSettings : AuthenticationOptions
    {
        /// <summary>
        /// Gets or sets application id URI.
        /// </summary>
        public string ApplicationIdURI { get; set; }

        /// <summary>
        /// Gets or sets valid issuer URL.
        /// </summary>
        public string ValidIssuers { get; set; }

        /// <summary>
        /// Gets or sets Graph API scope.
        /// </summary>
        public string GraphScope { get; set; }

        /// <summary>
        /// Gets or sets instance.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets tenant Id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets client Id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets cient secret.
        /// </summary>
        public string ClientSecret { get; set; }
    }
}