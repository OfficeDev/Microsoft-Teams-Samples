namespace Microsoft.Teams.Apps.SelfHelp.AdaptiveCard.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Exposes methods for creating adaptive cards.
    /// </summary>
    public interface IAdaptiveCardService
    {
        /// <summary>
        /// Get application installation and coversation id.
        /// </summary>
        /// <param name="learningId">learning id.</param>
        /// <param name="userAadId">Aad id of user.</param>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory.</param>
        /// <param name="externalAppId">Id of external application.</param>
        /// <returns>Returns the install application and conversation id.</returns>
        Task<string> InstallAppAndGetConversationId(string learningId, string userAadId, string token, string environmentCurrentDirectory, string externalAppId);

        /// <summary>
        /// Send teams assignment task notification details
        /// </summary>
        /// <param name="learningId">learning id.</param>
        /// <param name="teamId">id of team.</param>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory.</param>
        /// <param name="externalAppId">Id of external application.</param>
        /// <returns>Returns the teams assignment task notification details.</returns>
        Task<string> SendTeamsAssignemntTaskNotification(string learningId, string teamId, string token, string environmentCurrentDirectory, string externalAppId);

        /// <summary>
        /// Install the application and user conversation id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userAadId">Aad id of user.</param>
        /// <param name="externalAppId">Id of external application.</param>
        /// <returns>Return the installed application and user conversation id.</returns>
        Task<string> InstallAppAndGetUserConversationId(string token, string userAadId, string externalAppId);

        /// <summary>
        /// Install the application and team conversation id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <param name="externalAppId">Id of external application.</param>
        /// <returns>Return the installed application and team conversation id.</returns>
        Task<string> InstallAppAndGetTeamConversationId(string token, string teamId, string externalAppId);
    }
}