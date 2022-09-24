namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Authorization requirement - validates member role requirement.
    /// </summary>
    public class MemberRoleRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRoleRequirement"/> class.
        /// </summary>
        /// <param name="allowedMemberRoles">List of member roles that meet the requirement.</param>
        /// <param name="allowAdmins">This is an optional configuration to allow admins.</param>
        public MemberRoleRequirement(
            IList<MemberRole> allowedMemberRoles,
            bool allowAdmins = false)
        {
            this.AllowedMemberRoles = allowedMemberRoles ?? throw new System.ArgumentNullException(nameof(allowedMemberRoles));
            this.AllowGlobalAdmins = allowAdmins;
        }

        /// <summary>
        /// Gets a list of member roles that meet the requirement.
        /// </summary>
        public IList<MemberRole> AllowedMemberRoles { get; }

        /// <summary>
        /// Gets a value indicating whether global admins meet the requirement.
        /// </summary>
        public bool AllowGlobalAdmins { get; }
    }
}
