namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;

    /// <summary>
    /// UserResourceRequirement authorization handler.
    /// </summary>
    public class UserResourceHandler : AuthorizationHandler<UserResourceRequirement, string>
    {
        private readonly AuthorizationSettings authorizationSettings;
        private readonly ILogger<UserResourceHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserResourceHandler"/> class.
        /// </summary>
        /// <param name="authorizationSettings">AuthZ settings.</param>
        /// <param name="logger">Logger.</param>
        public UserResourceHandler(
            AuthorizationSettings authorizationSettings,
            ILogger<UserResourceHandler> logger)
        {
            this.authorizationSettings = authorizationSettings ?? throw new ArgumentNullException(nameof(authorizationSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserResourceRequirement requirement, string resource)
        {
            // Ensure user is authenticated.
            if (!context.User.Identity.IsAuthenticated)
            {
                throw new QBotException(HttpStatusCode.Unauthorized, ErrorCode.Forbidden, "User is not authenticated.");
            }

            // Same user accessing the resource.
            var userId = context.User.GetUserId();
            if (resource.Equals(userId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // check for admin.
            var userUpn = context.User.GetPreferredUserName();
            if (this.authorizationSettings.AdminUpnList.Contains(userUpn))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            this.logger.LogWarning("User is not authorized.");
            throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "User is not authorized.");
        }
    }
}
