namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper
{
    using Microsoft.Graph;

    /// <summary>
    /// Get the from Graph API.
    /// </summary>
    public interface IGraphApiHelper
    {
        /// <summary>
        /// Get user profile details from Microsoft Graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="query">The query.</param>
        /// <returns>List of user profile details.</returns>
        Task<IEnumerable<User>> SearchAsync(string token, string query);

        /// <summary>
        /// Fetching public url of user profile photo.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userAadId">Aad id of user.</param>
        /// <returns>Returns public url of user profile photo.</returns>
        Task<string> GetPublicURLForProfilePhoto(string token, string userAadId);

        /// <summary>
        /// Install the application for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <param name="userId">Unique id of user.</param>
        /// <returns>Return the installed application for user</returns>
        Task InstallAppForUserAsync(string token, string appId, string userId);

        /// <summary>
        /// Get the chat thread id for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userId">Unique id of user.</param>
        /// <param name="appId">Application id.</param>
        /// <returns>Returns the chat thread id for user.</returns>
        Task<string> GetChatThreadIdAsync(string token, string userId, string appId);

        /// <summary>
        /// Get the team application installed id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="teamId">Id of team.</param>
        /// <param name="appId">Application id.</param>
        /// <returns>Return the team application installed id.</returns>
        Task<string> GetteamAppInstalledIdAsync(string token, string teamId, string appId);

        /// <summary>
        /// Get appliccation installation id for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <param name="userId">Id of user.</param>
        /// <returns>Returns appliccation installation id for user.</returns>
        Task<string> GetAppInstallationIdForUserAsync(string token, string appId, string userId);

        /// <summary>
        /// Appliccation installation for team.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Id of application.</param>
        /// <param name="teamId">Team id.</param>
        /// <returns>Returns the install application for team.</returns>
        Task InstallAppForTeamAsync(string token, string appId, string teamId);

        /// <summary>
        /// get user by teams and channel.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="UserAadId">The UserAadId.</param>
        /// <returns>Team data.</returns>
        Task<List<Team>> GetMyJoinedTeamsDataAsync(string token, string UserAadId);

        /// <summary>
        /// Get external application id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Id of application.</param>
        /// <returns>Returns the external application id.</returns>
        Task<string?> GetExternalAppIdAsync(string token, string appId);
    }
}