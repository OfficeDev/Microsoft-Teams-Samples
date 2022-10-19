namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface represents the teams repository details.
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// Inserts or updates a new user team details when user installs a Bot.
        /// </summary>
        /// <param name="teamEntity">The user team details.</param>
        /// <returns>Returns true if team details inserted or updated successfully,Else returns false.</returns>
        Task<bool> CreateTeamsAsync(TeamEntity teamEntity);

        /// <summary>
        /// Gets teams details by team id.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<TeamEntity>> GetTeamByTeamsIdAsync(string teamId,string channelId);

        /// <summary>
        /// Gets all teams details.
        /// </summary>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<TeamEntity>> GetAllTeamsAsync();

        /// <summary>
        /// Get teams details by channel id.
        /// </summary>
        /// <param name="channelId">Teams channel id.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<TeamEntity>> GetTeamByChannelIdAsync(string channelId);

        /// <summary>
        /// Get teams detail by tenant id.
        /// </summary>
        /// <param name="tenantId">Unique id of tenent.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<TeamEntity>> GetTeamByTenantIdAsync(string tenantId);
    }
}