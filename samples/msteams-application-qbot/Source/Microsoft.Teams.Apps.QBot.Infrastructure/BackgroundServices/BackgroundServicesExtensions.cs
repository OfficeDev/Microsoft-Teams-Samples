// <copyright file="BackgroundServicesExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.BackgroundServices
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Background services extensions.
    /// </summary>
    public static class BackgroundServicesExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects backgorund services.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new BackgroundServicesSettings
            {
                PublishKbFrequencyInMinutes = configuration.GetValue<int>("BackgroundServices:PublishKbFrequencyInMinutes"),
                DeleteUserDataFrequencyInDays = configuration.GetValue<int>("BackgroundServices:DeleteUserDataFrequencyInDays"),
            };

            services.AddSingleton<BackgroundServicesSettings>(settings);
            services.AddHostedService<PublishKbHostedService>();
            services.AddHostedService<DeleteUserDataHostedService>();

            return services;
        }
    }
}
