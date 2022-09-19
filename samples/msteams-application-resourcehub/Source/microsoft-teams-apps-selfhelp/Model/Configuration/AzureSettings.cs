namespace Microsoft.Teams.Selfhelp.Models.Configuration
{
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;

    /// <summary>
    /// A class which helps to provide Azure settings for application.
    /// </summary>
    public class AzureSettings : AzureADOptions
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
        /// Gets or sets LogicAppUrl.
        /// </summary>
        public string LogicAppUrl { get; set; }
    }
}