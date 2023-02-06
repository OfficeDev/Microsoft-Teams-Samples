namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;

    /// <summary>
    /// Bot Middleware - authorizes access from MS Teams channel and specified tenant only.
    /// </summary>
    public class BotAuthMiddleware : IMiddleware
    {
        private const string TeamsChannel = "msteams";
        private const string ContinueConversationActivityName = "ContinueConversation";

        private readonly IAppSettings appSettings;
        private readonly IAppSettingsRepository repository;
        private readonly ILogger<BotAuthMiddleware> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAuthMiddleware"/> class.
        /// </summary>
        /// <param name="appSettings">App Settings.</param>
        /// <param name="repository">App settings repository.</param>
        /// <param name="logger">Logger.</param>
        public BotAuthMiddleware(
            IAppSettings appSettings,
            IAppSettingsRepository repository,
            ILogger<BotAuthMiddleware> logger)
        {
            this.appSettings = appSettings ?? throw new System.ArgumentNullException(nameof(appSettings));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            // Don't need to run authorization logic for outgoing messages.
            if (ContinueConversationActivityName.Equals(turnContext?.Activity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                await next(cancellationToken).ConfigureAwait(false);
                return;
            }

            // Listen to MS Teams channel only.
            if (!TeamsChannel.Equals(turnContext?.Activity?.ChannelId, StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogWarning($"Received a message from a non Teams Channel: {turnContext?.Activity?.ChannelId}");
                return;
            }

            // Messages from specified tenant only.
            if (!this.appSettings.TenantId.Equals(turnContext?.Activity?.Conversation?.TenantId, StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogWarning($"Bot accepts messages from tenant: {this.appSettings.TenantId} only. Received message from different tenant: {turnContext?.Activity?.Conversation?.TenantId}.");
                return;
            }

            // Update service url if required.
            await this.UpdateServiceUrlAsync(turnContext);

            // Continue
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateServiceUrlAsync(ITurnContext turnContext)
        {
            var updatedServiceUrl = turnContext?.Activity?.ServiceUrl;
            if (string.IsNullOrEmpty(updatedServiceUrl))
            {
                return;
            }

            try
            {
                var cachedServiceUrl = await this.repository.GetServiceUrlAsync();
                if (cachedServiceUrl != updatedServiceUrl)
                {
                    await this.repository.AddOrUpdateServiceUrlAsync(updatedServiceUrl);
                }
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, "Failed to store serviceUrl.");
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Failed to store serviceUrl.");
            }
        }
    }
}
