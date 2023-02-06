// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

public static class DomainExtensions
{
    /// <summary>
    /// Adds the services that are available in this project to Dependency Injection.
    /// Include this in your Startup.cs ConfigureServices if you need to access these services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collections.</returns>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IConnectorClientFactory, ConnectorClientFactory>();
        services.AddTransient<IBotService, BotService>();
        services.AddTransient<IRepositoryObjectService<SupportDepartment, SupportDepartmentInput>, SupportDepartmentService>();
        services.AddTransient<IRepositoryObjectService<MsTeamsBotData, MsTeamsBotData>, MsTeamsBotDataService>();
        services.AddTransient<ISubEntityService<CustomerInquiry, CustomerInquiryInput>, CustomerInquiryService>();
        return services;
    }
}
