// <copyright file="SecretsProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Secrets
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using global::Azure;
    using global::Azure.Security.KeyVault.Certificates;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;

    /// <summary>
    /// Secrets provider implementation.
    /// </summary>
    internal class SecretsProvider : ISecretsProvider
    {
        private readonly IAppSettings appSettings;
        private readonly CertificateClient certificateClient;
        private readonly ILogger<SecretsProvider> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsProvider"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        /// <param name="certificateClient">Certificate client.</param>
        /// <param name="logger">Logger.</param>
        public SecretsProvider(
            IAppSettings appSettings,
            CertificateClient certificateClient,
            ILogger<SecretsProvider> logger)
        {
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.certificateClient = certificateClient ?? throw new ArgumentNullException(nameof(certificateClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<AppCredentials> GetBotAppCredentialsAsync()
        {
            try
            {
                var response = await this.certificateClient.DownloadCertificateAsync(this.appSettings.BotCertificateName);
                var clientCredentials = new CertificateAppCredentials(response.Value, this.appSettings.BotAppId);
                return clientCredentials;
            }
            catch (InvalidDataException exception)
            {
                this.logger.LogError(exception, $"Certificate not found. Cert name: {this.appSettings.BotCertificateName} ");
            }
            catch (RequestFailedException exception)
            {
                this.logger.LogError(exception, $"Failed to fetch certificate. ErrorCode: {exception.ErrorCode} Cert name: {this.appSettings.BotCertificateName}.");
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Failed to fetch certificate. Cert name: {this.appSettings.BotCertificateName}.");
            }

            throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Failed to get bot app certificate.");
        }
    }
}
