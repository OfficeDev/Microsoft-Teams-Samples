namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Admin role handler.
    /// </summary>
    public class AdminRoleHandler : AuthorizationHandler<AdminRoleRequirement>
    {
        private readonly AuthorizationSettings authorizationSettings;
        private readonly ILogger<AdminRoleHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminRoleHandler"/> class.
        /// </summary>
        /// <param name="authorizationSettings">AuthZ settings.</param>
        /// <param name="logger">Logger.</param>
        public AdminRoleHandler(
            AuthorizationSettings authorizationSettings,
            ILogger<AdminRoleHandler> logger)
        {
            this.authorizationSettings = authorizationSettings ?? throw new ArgumentNullException(nameof(authorizationSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
        {
            // Ensure user is authenticated.
            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            var userUpn = context.User.GetPreferredUserName();
            if (this.authorizationSettings.AdminUpnList.Contains(userUpn))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            this.logger.LogInformation("User is not authorized.");
            return Task.CompletedTask;
        }
    }
}
