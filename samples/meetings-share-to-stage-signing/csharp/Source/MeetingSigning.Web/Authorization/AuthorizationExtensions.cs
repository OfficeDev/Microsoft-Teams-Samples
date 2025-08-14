// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="services">Service collection.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddTransient<IAuthorizationHandler, GetDocumentHandler>();
            services.AddTransient<IAuthorizationHandler, SignDocumentHandler>();
            services.AddAuthorization(configure =>
            {
                configure.AddPolicy(AuthZPolicy.GetDocumentPolicy, policy =>
                    policy.Requirements.Add(new GetDocumentRequirement()));
                configure.AddPolicy(AuthZPolicy.SignDocumentPolicy, policy =>
                    policy.Requirements.Add(new SignDocumentRequirement()));
            });

            return services;
        }
    }
}
