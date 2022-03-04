// <copyright file="TeamsServicesExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.Mappings;

    /// <summary>
    /// Teams service extensions.
    /// </summary>
    public static class TeamsServicesExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects teams services.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddTeamsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(Profiles));

            services.AddSingleton<IGraphClientFactory, GraphClientFactory>();
            services.AddSingleton<ITeamsServicesFactory, TeamsServicesFactory>();
            services.AddTransient<IMeetingsService, MeetingsService>();
            services.AddTransient<IConversationService, ConversationService>();
            services.AddSingleton<ICardFactory, CardFactory>();
            return services;
        }
    }
}
