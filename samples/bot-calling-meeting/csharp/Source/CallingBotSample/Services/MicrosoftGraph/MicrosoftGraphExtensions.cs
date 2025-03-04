// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Azure.Identity;
using CallingBotSample.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public static class MicrosoftGraphExtensions
    {
        /// <summary>
        /// Adds the services that are available in this project to Dependency Injection.
        /// Include this in your Startup.cs ConfigureServices if you need to access these services.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="azureAdOptionsAction">AzureAD Options.</param>
        /// <returns>Service collections.</returns>
        public static IServiceCollection AddMicrosoftGraphServices(this IServiceCollection services, Action<AzureAdOptions> azureAdOptionsAction)
        {
            var options = new AzureAdOptions();
            azureAdOptionsAction(options);

            //var options = new ClientSecretCredentialOptions {     AuthorityHost = AzureAuthorityHosts.AzureGovernment, }; // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredentialvar clientSecretCredential = new ClientSecretCredential(     tenantId, clientId, clientSecret, options);

            ClientSecretCredential authenticationProvider = new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret, new ClientSecretCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzureGovernment });

            var authProvider = new TokenCredentialAuthProvider(authenticationProvider, new[] { "https://graph.microsoft.us/.default" } );
            services.AddScoped<GraphServiceClient, GraphServiceClient>(sp =>
            {
                return new GraphServiceClient("https://graph.microsoft.us/v1.0", authProvider, null);
            });

            services.AddTransient<ICallService, CallService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IOnlineMeetingService, OnlineMeetingService>();
            services.AddSingleton<AudioRecordingConstants>();
            return services;
        }
    }
}
