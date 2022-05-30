namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// MemberRoleRequirement authorization handler.
    /// </summary>
    public class MemberRoleHandler : AuthorizationHandler<MemberRoleRequirement, Course>
    {
        private readonly ICourseReader courseReader;
        private readonly AuthorizationSettings authorizationSettings;
        private readonly ILogger<MemberRoleHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRoleHandler"/> class.
        /// </summary>
        /// <param name="courseReader">Course reader.</param>
        /// <param name="authorizationSettings">AuthZ settings.</param>
        /// <param name="logger">Logger.</param>
        public MemberRoleHandler(
            ICourseReader courseReader,
            AuthorizationSettings authorizationSettings,
            ILogger<MemberRoleHandler> logger)
        {
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.authorizationSettings = authorizationSettings ?? throw new ArgumentNullException(nameof(authorizationSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberRoleRequirement requirement, Course resource)
        {
            // Ensure user is authenticated.
            if (!context.User.Identity.IsAuthenticated)
            {
                throw new QBotException(HttpStatusCode.Unauthorized, ErrorCode.Forbidden, "User is not authenticated.");
            }

            var userId = context.User.GetUserId();
            try
            {
                var member = await this.courseReader.GetMemberAsync(resource.Id, userId);
                if (requirement.AllowedMemberRoles.Contains(member.Role))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            catch (Exception exception)
            {
                // Failed to validate user role.
                this.logger.LogWarning(exception, "Failed to validate user role.");
            }

            // check if global admins are allowed.
            if (requirement.AllowGlobalAdmins)
            {
                var userUpn = context.User.GetPreferredUserName();
                if (this.authorizationSettings.AdminUpnList.Contains(userUpn))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "User is not authorized.");
        }
    }
}
