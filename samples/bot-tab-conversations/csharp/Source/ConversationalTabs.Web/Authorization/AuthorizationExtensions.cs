// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Authorization;

using Microsoft.AspNetCore.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Service Collection extension.
    ///
    /// Injects authorization logic.
    /// </summary>
    /// <param name="services">Servie collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddTransient<IAuthorizationHandler, IsMemberOfTeamHandler>();
        services.AddAuthorization(configure =>
        {
            configure.AddPolicy(AuthZPolicy.IsMemberOfTeamPolicy, policy =>
                policy.Requirements.Add(new IsMemberOfTeamRequirement()));
        });

        return services;
    }
}
