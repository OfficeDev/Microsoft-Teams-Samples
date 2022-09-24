namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Authorization extensions.
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects authorization logic.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            var serializedList = configuration.GetValue<string>("Authorization:AdminUpnList", string.Empty);
            var adminUpnList = new HashSet<string>(serializedList.Split(',').ToList());
            services.AddSingleton(new AuthorizationSettings(adminUpnList));
            services.AddTransient<IAuthorizationHandler, MemberRoleHandler>();
            services.AddTransient<IAuthorizationHandler, PostAnswerHandler>();
            services.AddSingleton<IAuthorizationHandler, AdminRoleHandler>();
            services.AddAuthorization(configure =>
            {
                configure.AddPolicy(AuthZPolicy.CourseMemberPolicy, policy =>
                    policy.Requirements.Add(new MemberRoleRequirement(
                        new List<MemberRole>()
                        {
                            MemberRole.Student,
                            MemberRole.Tutor,
                            MemberRole.Educator,
                        },
                        true/*allowAdmins*/)));

                configure.AddPolicy(AuthZPolicy.CourseManagerPolicy, policy =>
                    policy.Requirements.Add(new MemberRoleRequirement(
                        new List<MemberRole>()
                        {
                            MemberRole.Educator,
                        },
                        true/*allowAdmins*/)));

                configure.AddPolicy(AuthZPolicy.UserResourcePolicy, policy =>
                    policy.Requirements.Add(new UserResourceRequirement()));

                configure.AddPolicy(AuthZPolicy.AdminPolicy, policy =>
                    policy.Requirements.Add(new AdminRoleRequirement()));

                configure.AddPolicy(AuthZPolicy.PostAnswerPolicy, policy =>
                    policy.Requirements.Add(new PostAnswerRequirement()));
            });

            return services;
        }
    }
}
