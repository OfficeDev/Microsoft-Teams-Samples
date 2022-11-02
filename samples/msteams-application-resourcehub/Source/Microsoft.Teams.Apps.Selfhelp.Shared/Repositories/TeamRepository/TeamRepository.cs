namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This class manages storage operations related to user configurations.
    /// </summary>
    public class TeamRepository : BaseRepository<TeamEntity>, ITeamRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store user configurations.
        /// </summary>
        private const string TeamConfigurationTable = "TeamEntity";

        /// <summary>
        /// Represents the partitionKey used to store user configurations.
        /// </summary>
        private const string TablePartitionKey = "Team";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRepository"/> class.
        /// Teams repository details.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public TeamRepository(
          IOptions<BlobStorageSetting> options,
          ILogger<TeamRepository> logger)
          : base(options?.Value.ConnectionString, TeamConfigurationTable, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="teamId">Id of team.</param>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<IEnumerable<TeamEntity>> GetTeamByTeamsIdAsync(string teamId, string channelId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(TeamEntity.ChannelId)} eq '{channelId}' and ${ nameof(TeamEntity.TeamId)} eq '{teamId}'";
            return await this.GetWithFilterAsync<TeamEntity>(query);
        }

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="channelId">Id of channel.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<IEnumerable<TeamEntity>> GetTeamByChannelIdAsync(string channelId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(TeamEntity.ChannelId)} eq '{channelId}'";
            return await this.GetWithFilterAsync<TeamEntity>(query);
        }

        /// <summary>
        /// Gets conversation details.
        /// </summary>
        /// <param name="tenantId">Unique id of tenant.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<IEnumerable<TeamEntity>> GetTeamByTenantIdAsync(string tenantId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(TeamEntity.TenantId)} eq '{tenantId}'";
            return await this.GetWithFilterAsync<TeamEntity>(query);
        }

        /// <summary>
        /// Gets all user teams details.
        /// </summary>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<IEnumerable<TeamEntity>> GetAllTeamsAsync()
        {
            await this.EnsureInitializedAsync();
            return await this.GetAllAsync<TeamEntity>();
        }

        /// <summary>
        /// Insert or update a new team configuration details when user installs a Bot.
        /// </summary>
        /// <param name="teamConfigurationDetails">The team configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<bool> CreateTeamsAsync(TeamEntity teamConfigurationDetails)
        {
            if (teamConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(teamConfigurationDetails), "The teams configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            var user = await this.GetAsync<TeamEntity>(teamConfigurationDetails.TeamId);
            if (user == null)
            {
                teamConfigurationDetails.RowKey = Guid.NewGuid().ToString();
                return await this.CreateAsync(teamConfigurationDetails);
            }

            return true;
        }
    }
}