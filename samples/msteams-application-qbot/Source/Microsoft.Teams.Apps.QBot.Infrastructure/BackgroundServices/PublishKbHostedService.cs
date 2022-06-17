// <copyright file="PublishKbHostedService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.BackgroundServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;

    /// <summary>
    /// Background service that publishes Kb every fixed intervel.
    /// </summary>
    public class PublishKbHostedService : BackgroundService
    {
        private readonly BackgroundServicesSettings settings;
        private readonly IQnAService qnAService;
        private readonly ILogger<PublishKbHostedService> logger;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishKbHostedService"/> class.
        /// </summary>
        /// <param name="settings">background services settings.</param>
        /// <param name="qnAService">QnA service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="telemetryClient">Telemetry client.</param>
        public PublishKbHostedService(
            BackgroundServicesSettings settings,
            IQnAService qnAService,
            ILogger<PublishKbHostedService> logger,
            TelemetryClient telemetryClient)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.qnAService = qnAService ?? throw new ArgumentNullException(nameof(qnAService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (this.telemetryClient.StartOperation<RequestTelemetry>("PublishKb"))
                {
                    try
                    {
                        this.logger.LogInformation($"Running {nameof(PublishKbHostedService)}");
                        await this.qnAService.PublishKbAsync();
                        this.telemetryClient.TrackEvent("Published KB successfully");
                    }
                    catch (QBotException exception)
                    {
                        this.logger.LogWarning(exception, $"Failed to publish Kb. Error code: {exception.Code}. Status Code {exception.StatusCode}.");
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogWarning(exception, "Failed to publish Kb.");
                    }
                }

                // Publish again in x minutes.
                await Task.Delay(System.TimeSpan.FromMinutes(this.settings.PublishKbFrequencyInMinutes));
            }
        }
    }
}
