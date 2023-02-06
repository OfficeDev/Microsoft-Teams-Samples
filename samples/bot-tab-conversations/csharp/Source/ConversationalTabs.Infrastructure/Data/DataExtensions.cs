// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;
using Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.Data.Repositories.InMemory;

public static class DataExtensions
{
    /// <summary>
    /// Injects concrete implementations of repositories defined for InMemory implementation.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddInMemoryDataStorage(this IServiceCollection services)
    {
        services.AddSingleton<SupportDepartmentAndInquiryRepository>();
        services.AddSingleton<IRepository<SupportDepartment>>(s => s.GetService<SupportDepartmentAndInquiryRepository>());
        services.AddSingleton<ISubEntityRepository<CustomerInquiry>>(s => s.GetService<SupportDepartmentAndInquiryRepository>());
        services.AddSingleton<IRepository<MsTeamsBotData>, MsTeamsBotDataRepository>();
        return services;
    }
}
