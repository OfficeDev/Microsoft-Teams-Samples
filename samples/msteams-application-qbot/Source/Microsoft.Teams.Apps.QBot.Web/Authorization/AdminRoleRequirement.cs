namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Admin role requirement.
    /// </summary>
    public class AdminRoleRequirement : IAuthorizationRequirement
    {
    }
}
