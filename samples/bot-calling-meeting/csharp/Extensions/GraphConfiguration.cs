// < copyright file = "GraphConfiguration.cs" company = "Microsoft Corporation" >
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace CallingBotSample.Extensions
{
    using Azure.Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Graph;
    using System;

    /// <summary>
    /// Graph Configuration.
    /// </summary>
    public static class GraphConfiguration
    {
        /// <summary>
        /// Configure Graph Component.
        /// </summary>
        /// <param name="services">IServiceCollection .</param>
        /// <param name="configuration">IConfiguration .</param>
        /// <returns>..</returns>
        public static IServiceCollection ConfigureGraphComponent(this IServiceCollection services, Action<AzureAdOptions> azureAdOptionsAction)
        {
            var options = new AzureAdOptions();
            azureAdOptionsAction(options);

            ClientSecretCredential authenticationProvider = new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);

            services.AddScoped<GraphServiceClient, GraphServiceClient>(sp =>
            {
                return new GraphServiceClient(authenticationProvider);
            });

            return services;
        }
    }
}