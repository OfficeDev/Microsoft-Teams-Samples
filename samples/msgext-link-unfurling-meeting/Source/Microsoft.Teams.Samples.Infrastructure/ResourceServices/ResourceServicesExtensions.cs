// <copyright file="ResourceServicesExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Resource services extensions.
    /// </summary>
    public static class ResourceServicesExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects resource services.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddResourceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IResourceProvider, ResourceProvider>();
            services.AddTransient<IUrlParser, UrlParser>();
            return services;
        }
    }
}
