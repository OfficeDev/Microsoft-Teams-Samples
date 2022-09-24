// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Provider
{
    using Azure.Identity;
    using MeetingNotification.Model.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;

    public class GraphBetaClient
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<BotConfiguration> botSettings;

        public GraphBetaClient(IOptions<BotConfiguration> botSettings)
        {
            this.botSettings = botSettings;
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
                this.botSettings.Value.MicrosoftAppTenantId,
                this.botSettings.Value.MicrosoftAppId,
                this.botSettings.Value.MicrosoftAppPassword,
                options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}
