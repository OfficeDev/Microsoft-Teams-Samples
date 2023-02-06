// <copyright file="SecretsExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Secrets
{
    using System;
    using AzureSamples.Security.KeyVault.Proxy;
    using global::Azure.Core;
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Certificates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Secrets provider extensions.
    /// </summary>
    public static class SecretsExtensions
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects secrets provider.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddSecretsProvider(this IServiceCollection services, IConfiguration configuration)
        {
            var keyVaultUrl = configuration.GetValue<string>("KeyVault:Url");
            var options = new CertificateClientOptions();
            options.AddPolicy(new KeyVaultProxy(), HttpPipelinePosition.PerCall);
            services.AddSingleton(new CertificateClient(new Uri(keyVaultUrl), new DefaultAzureCredential(), options));
            services.AddSingleton<ISecretsProvider, SecretsProvider>();

            return services;
        }
    }
}
