// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.GraphService;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

/// <summary>
/// Graph service extensions.
/// </summary>
public static class GraphServiceExtensions
{
    /// <summary>
    /// Service Collection extension.
    ///
    /// Injects concrete implementations of teams services defined in the Domain project.
    /// </summary>
    /// <param name="services">Servie collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddGraphServices(this IServiceCollection services)
    {
        services.AddTransient<IUserTeamsService, UsersTeamService>();
        return services;
    }
}
