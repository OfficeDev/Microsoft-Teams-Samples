using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking.Sample.Services.OAuth
{
    /// <summary>
    /// Service specific configuration(s) for the downstream OAuth2.0 service.
    /// </summary>
    public sealed class ExternalAuthParameters
    {
        /// <summary>
        /// User address for gmail client.
        /// </summary>
        /// <remarks>
        /// This needs to be a valid email address for <see cref="GmailServiceClient.cs" />.
        /// </remarks>
        [Required(AllowEmptyStrings = true)]
        public string GmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the downstream service type
        /// </summary>
        /// <remarks>
        /// Default is set to 'Github' in appsettings.json/>.
        /// </remarks>
        [Required(AllowEmptyStrings = false)]
        public string Service { get; set; } = string.Empty;

    }
}
