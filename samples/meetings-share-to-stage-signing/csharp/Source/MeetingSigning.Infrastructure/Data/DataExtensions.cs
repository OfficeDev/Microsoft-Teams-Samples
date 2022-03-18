// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Mapping;

    public static class DataExtensions
    {
        /// <summary>
        /// Service Collection extension.
        /// Injects concrete implementations of repositories defined in the Domain project.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns>Service Collection.</returns>
        public static IServiceCollection AddEntityFrameworkDataStorage(this IServiceCollection services)
        {
            // Db Context
            services.AddDbContext<MeetingSigningDbContext>(options => options.UseInMemoryDatabase("MeetingSigning"));
            services.AddAutoMapper(typeof(Profiles));
            // Repositories
            services.AddTransient<IDocumentRepository, DocumentRepository>();
            services.AddTransient<ISignatureRepository, SignatureRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IViewerRepository, ViewerRepository>();
            return services;
        }

        /// <summary>
        /// Injects concrete implementations of repositories defined for Inmemory implementation.
        /// </summary>
        /// <param name="services"></param>
        /// <returns>Service Collection</returns>
        public static IServiceCollection AddInMemoryDataStorage(this IServiceCollection services)
        {
            services.AddSingleton<IDocumentRepository, Repositories.InMemory.DocumentRepository>();
            services.AddSingleton<ISignatureRepository, Repositories.InMemory.SignatureRepository>();
            services.AddSingleton<IUserRepository, Repositories.InMemory.UserRepository>();
            services.AddSingleton<IViewerRepository, Repositories.InMemory.ViewerRepository>();
            return services;
        }
    }
}
