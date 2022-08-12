// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;
using Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.AdaptiveCards;
using Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.Data;
using Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.GraphService;

public static class InfrastructureExtensions
{
    /// <summary>
    /// Adds the services that are available in this project to Dependency Injection.
    /// Include this in your Startup.cs ConfigureServices if you need to access these services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collections.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IAdaptiveCardFactory, AdaptiveCardFactory>();
        services.AddInMemoryDataStorage();
        services.AddGraphServices();
        return services;
    }
}
