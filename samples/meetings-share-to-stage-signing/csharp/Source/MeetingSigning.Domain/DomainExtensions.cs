// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;

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
            services.AddTransient<IDocumentService, DocumentService>();
            return services;
        }
    }
}
