namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Secrets;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;

    /// <summary>
    /// Bot Http Adapter.
    /// </summary>
    public class BotHttpAdapter : BotFrameworkHttpAdapter
    {
        private readonly IAppSettings appSettings;
        private readonly ISecretsProvider secretsProvider;
        private readonly ILogger<BotHttpAdapter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotHttpAdapter"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        /// <param name="credentialProvider">Cred provider.</param>
        /// <param name="secretsProvider">Secrets provider..</param>
        /// <param name="middleWare">Auth middleware.</param>
        /// <param name="logger">Logger.</param>
        public BotHttpAdapter(
            IAppSettings appSettings,
            ICredentialProvider credentialProvider,
            ISecretsProvider secretsProvider,
            BotAuthMiddleware middleWare,
            ILogger<BotHttpAdapter> logger)
            : base(credentialProvider)
        {
            if (middleWare is null)
            {
                throw new ArgumentNullException(nameof(middleWare));
            }

            this.Use(middleWare);
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected async override Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            if (this.appSettings.BotAppId != appId)
            {
                var message = $"BuildCredentialsAsync: Teams bot app id mismatch. Expected: {this.appSettings.BotAppId}, Actual: {appId}";
                this.Logger.LogWarning(message);
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.Unknown, message);
            }

            var clientCredentials = await this.secretsProvider.GetBotAppCredentialsAsync();
            return clientCredentials;
        }
    }
}
