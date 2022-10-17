namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// This interface will contain the helper methods to generate Azure Active Directory user access token for given resource, e.g. Microsoft Graph.
    /// </summary>
    public interface ITokenHelper
    {
        /// <summary>
        /// Gets the application token.
        /// </summary>
        /// <param name="tenantId">Unique id of tenant.</param>
        /// <param name="clientId">The application client id.</param>
        /// <param name="clientSecret">The application client secret.</param>
        /// <returns>The application token.</returns>
        Task<GraphTokenResponse> ObtainApplicationTokenAsync(
            string tenantId,
            string clientId,
            string clientSecret);

        /// <summary>
        /// Method to generate token.
        /// </summary>
        /// <param name="encodedToken">Encoded access token.</param>
        /// <returns>Token.</returns>
        Task<string> ObtainDelegatedGraphTokenAsync(StringValues encodedToken);
    }
}