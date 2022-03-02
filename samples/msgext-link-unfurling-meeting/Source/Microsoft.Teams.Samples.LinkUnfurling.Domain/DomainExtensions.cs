// <copyright file="DomainExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.OnlineMeetings;

    /// <summary>
    /// Domain service extensions.
    /// </summary>
    public static class DomainExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects domain services.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddTransient<IMeetingSetup, OnlineMeetingSetup>();
            return services;
        }
    }
}
