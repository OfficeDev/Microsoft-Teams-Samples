// < copyright file = "GraphConfiguration.cs" company = "Microsoft Corporation" >
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace CallingBotSample.Extensions
{
    using CallingBotSample.Configuration;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Graph.Auth;
    using Microsoft.Identity.Client;
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

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(options.ClientId)
            .WithTenantId(options.TenantId)
            .WithClientSecret(options.ClientSecret)
            .Build();

            ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(confidentialClientApplication);

            services.AddScoped<IGraphServiceClient, GraphServiceClient>(sp =>
            {
                return new GraphServiceClient(authenticationProvider);
            });

            return services;
        }
    }
}