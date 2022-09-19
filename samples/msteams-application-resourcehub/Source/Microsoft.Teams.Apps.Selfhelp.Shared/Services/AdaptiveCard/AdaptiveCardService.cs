namespace Microsoft.Teams.Apps.SelfHelp.AdaptiveCard.Services
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper;

    /// <summary>
    /// This class helps in creating card as an attachment.
    /// </summary>
    public class AdaptiveCardService : IAdaptiveCardService
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<AdaptiveCardService> logger;

        /// <summary>
        /// Helper class to represent graph API calls.
        /// </summary>
        private readonly IGraphApiHelper graphHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveCardService"/> class.
        /// </summary>
        /// <param name="logger">Entity represents logger object.</param>
        /// <param name="graphHelper">Entity represents graph helper.</param>
        public AdaptiveCardService(ILogger<AdaptiveCardService> logger, IGraphApiHelper graphHelper)
        {
            this.logger = logger;
            this.graphHelper = graphHelper;
        }

        /// <summary>
        /// Send task notification to users.
        /// </summary>
        /// <param name="learningId">Unique learning id.</param>
        /// <param name="userAadId">User Aad id.</param>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="environmentCurrentDirectory">Current directory of environment.</param>
        /// <param name="ExternalAppId">Id of external application.</param>
        /// <returns>Returns the installation of application and conversation id.</returns>
        public async Task<string> InstallAppAndGetConversationId(string learningId, string userAadId, string token, string environmentCurrentDirectory, string ExternalAppId)
        {
            try
            {
                // pre-install bot for user if not already installed
                string conversationId = string.Empty;
                var serviceUrl = string.Empty;
                if (userAadId != null)
                {
                    this.logger.LogInformation($"User app details not found for user {userAadId}. Pre-installing the app for user");

                    // Create conversation using bot adapter for users with teams user id.
                    // For other user, install the User's app and get conversation id.
                    conversationId = await this.InstallAppAndGetUserConversationId(token, userAadId, ExternalAppId);
                }

                return conversationId;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "problem while sending the User task notification card.");
                throw;
            }
        }

        /// <summary>
        /// Send teams assignment task notification details.
        /// </summary>
        /// <param name="learningId">learning id.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="environmentCurrentDirectory">Current directory of environment.</param>
        /// <param name="ExternalAppId">Id of external application.</param>
        /// <returns>Returns the teams assignment task notification details.</returns>
        public async Task<string> SendTeamsAssignemntTaskNotification(string learningId, string teamId, string token, string environmentCurrentDirectory, string ExternalAppId)
        {
            try
            {
                // pre-install bot for team if not already installed
                string conversationId = string.Empty;
                var serviceUrl = string.Empty;
                if (teamId != null)
                {
                    this.logger.LogInformation($"User app details not found for user {teamId}. Pre-installing the app for user");

                    // Create conversation using bot adapter for users with teams user id.
                    // For other user, install the User's app and get conversation id.
                    conversationId = await this.InstallAppAndGetTeamConversationId(token, teamId, ExternalAppId);
                }

                return conversationId;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "problem while sending the team task notification card.");
                throw;
            }
        }

        /// <summary>
        /// Install the application and user conversation id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userAadId">Aad id of user.</param>
        /// <param name="ExternalAppId">External application id.</param>
        /// <returns>Return the installed application and user conversation id.</returns>
        public async Task<string> InstallAppAndGetUserConversationId(string token, string userAadId, string ExternalAppId)
        {
            var appId = ExternalAppId;
            if (string.IsNullOrEmpty(appId))
            {
                // This may happen if the User app is not added to the organization's app catalog.
                var errorMessage = "User App Not Found";
                this.logger.LogError(errorMessage);
                throw new ArgumentNullException("User app is not added to the organization's app catalog");
            }

            // Install app.
            try
            {
                await this.graphHelper.InstallAppForUserAsync(token, appId, userAadId);
            }
            catch (ServiceException exception)
            {
                switch (exception.StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        // Note: application is already installed, we should fetch conversation id for this user.
                        this.logger.LogWarning("Application is already installed for the user.");
                        break;
                    case HttpStatusCode.NotFound:
                        // Failed to find the User app in App Catalog. This may happen if the User app is deleted from app catalog.
                        var message = $"FailedToFindUserAppInAppCatalog:{appId}";
                        this.logger.LogError(message);
                        return string.Empty;
                    default:
                        var errorMessage = $"FailedToInstallApplicationForUserFormat: {exception.Message}";
                        this.logger.LogError(exception, errorMessage);
                        return string.Empty;
                }
            }

            // Get conversation id.
            try
            {
                var conversationId = await this.graphHelper.GetChatThreadIdAsync(token, userAadId, appId);
                return conversationId;
            }
            catch (ServiceException exception)
            {
                var errorMessage = $"FailedToInstallApplicationForUserFormat: {exception.Message}";
                this.logger.LogError(exception, errorMessage);
                return string.Empty;
            }
        }

        /// <summary>
        /// Install the application and team conversation id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="ExternalAppId">External application id.</param>
        /// <returns>Return the installed application and team conversation id.</returns>
        public async Task<string> InstallAppAndGetTeamConversationId(string token, string teamId, string ExternalAppId)
        {
            var appId = ExternalAppId;
            if (string.IsNullOrEmpty(appId))
            {
                // This may happen if the User app is not added to the organization's app catalog.
                var errorMessage = "Team App Not Found";
                this.logger.LogError(errorMessage);
                throw new ArgumentNullException("Team app is not added to the organization's app catalog");
            }

            // Install app.
            try
            {
                await this.graphHelper.InstallAppForTeamAsync(token, appId, teamId);
            }
            catch (ServiceException exception)
            {
                switch (exception.StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        // Note: application is already installed, we should fetch conversation id for this Team.
                        this.logger.LogWarning("Application is already installed for the team.");
                        break;
                    case HttpStatusCode.NotFound:
                        // Failed to find the User app in App Catalog. This may happen if the Team app is deleted from app catalog.
                        var message = $"FailedToFindteamAppInAppCatalog:{appId}";
                        this.logger.LogError(message);
                        return string.Empty;
                    default:
                        var errorMessage = $"FailedToInstallApplicationForTeamFormat: {exception.Message}";
                        this.logger.LogError(exception, errorMessage);
                        return string.Empty;
                }
            }

            // Get conversation id.
            try
            {
                var conversationId = await this.graphHelper.GetteamAppInstalledIdAsync(token, teamId, appId);
                return conversationId;

            }
            catch (ServiceException exception)
            {
                var errorMessage = $"FailedToInstallApplicationForTeamFormat: {exception.Message}";
                this.logger.LogError(exception, errorMessage);
                return string.Empty;
            }
        }
    }
}