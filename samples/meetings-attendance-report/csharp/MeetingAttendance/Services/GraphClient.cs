﻿// <copyright file="GraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingAttendance.Services
{
    using Azure.Identity;
    using MeetingAttendance.Models.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;

    public class GraphClient
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> botSettings;

        public GraphClient(IOptions<AzureSettings> botSettings)
        {
            this.botSettings = botSettings;
        }

        /// <summary>
        /// Gets the graph client to make Graph API calls.
        /// </summary>
        /// <returns>Graph service client.</returns>
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
