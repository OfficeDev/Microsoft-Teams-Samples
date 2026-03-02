// <copyright file="SimpleBetaGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace EventMeeting.Provider
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
        private readonly IOptions<AzureSettings> azureSettings;

        public SimpleBetaGraphClient(IOptions<AzureSettings> azureSettings)
        {
            this.azureSettings = azureSettings;
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
                this.azureSettings.Value.MicrosoftAppTenantId, 
                this.azureSettings.Value.MicrosoftAppId, 
                this.azureSettings.Value.MicrosoftAppPassword, 
                options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}