namespace Microsoft.Teams.Apps.QBot.Infrastructure.BackgroundServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;

    /// <summary>
    /// Background service that periodically deletes user data if the user is no longer part of any course.
    /// </summary>
    public class DeleteUserDataHostedService : BackgroundService
    {
        private readonly BackgroundServicesSettings settings;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<DeleteUserDataHostedService> logger;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUserDataHostedService"/> class.
        /// </summary>
        /// <param name="settings">background services settings.</param>
        /// <param name="scopeFactory">Service scope factory.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="telemetryClient">Telemetry client.</param>
        public DeleteUserDataHostedService(
            BackgroundServicesSettings settings,
            IServiceScopeFactory scopeFactory,
            ILogger<DeleteUserDataHostedService> logger,
            TelemetryClient telemetryClient)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc/>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (this.telemetryClient.StartOperation<RequestTelemetry>("DeleteUserData"))
                {
                    try
                    {
                        this.logger.LogInformation($"Running {nameof(DeleteUserDataHostedService)}");
                        using var scope = this.scopeFactory.CreateScope();
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var users = await userRepository.GetUsersWithNoCourseAsync();
                        foreach (var user in users)
                        {
                            this.logger.LogInformation($"Deleting user {user.AadId} from the database");
                            await this.DeleteUserAsync(userRepository, user.AadId);
                        }

                        this.logger.LogInformation("Successfully deleted all the users who are not part of any course.");
                        this.telemetryClient.TrackEvent("DeleteUserData completed successfully");
                    }
                    catch (QBotException exception)
                    {
                        this.logger.LogWarning(exception, $"Failed to fetch users to be deleted. Error code: {exception.Code}. Status Code {exception.StatusCode}.");
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogWarning(exception, "Failed to fetch users to be deleted.");
                    }
                }

                // Publish again in x days.
                await Task.Delay(TimeSpan.FromDays(this.settings.DeleteUserDataFrequencyInDays));
            }
        }

        private async Task DeleteUserAsync(IUserRepository userRepository, string userAadId)
        {
            try
            {
                await userRepository.DeleteUserAsync(userAadId);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to delete user {userAadId}");
            }
        }
    }
}
