namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System.Security.Claims;
    using Microsoft.Identity.Web;

    /// <summary>
    /// ClaimsPrincipal extensions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets user oid claim from claimsPrincipal.
        /// </summary>
        /// <param name="claimsPrincipal">ClaimsPrincipal.</param>
        /// <returns>Return user's id.</returns>
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimConstants.Oid);
        }

        /// <summary>
        /// Gets user's preferred_username claim from claimsPrincipal.
        /// </summary>
        /// <param name="claimsPrincipal">ClaimsPrincipal.</param>
        /// <returns>Return user's upn.</returns>
        public static string GetPreferredUserName(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimConstants.PreferredUserName);
        }
    }
}
