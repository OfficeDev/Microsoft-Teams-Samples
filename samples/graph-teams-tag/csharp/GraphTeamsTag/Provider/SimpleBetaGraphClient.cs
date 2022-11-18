// <copyright file="SimpleBetaGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Provider
{
    using Azure.Identity;
    using GraphTeamsTag.Models.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;

    public class SimpleBetaGraphClient
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IConfiguration _configuration;

        private static readonly string ClientIdConfigurationSettingsKey = "AzureAd:ClientId";

        private static readonly string ClientSecretConfigurationSettingsKey = "AzureAd:AppSecret";

        private static readonly string TenantIdConfigurationSettingsKey = "AzureAd:TenantId";

        public SimpleBetaGraphClient(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public GraphServiceClient GetGraphClientforApp()
        {
            // The client credentials flow requires that you request the
            // /.default scope, and preconfigure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                _configuration[TenantIdConfigurationSettingsKey],
                _configuration[ClientIdConfigurationSettingsKey],
                _configuration[ClientSecretConfigurationSettingsKey],
                options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}