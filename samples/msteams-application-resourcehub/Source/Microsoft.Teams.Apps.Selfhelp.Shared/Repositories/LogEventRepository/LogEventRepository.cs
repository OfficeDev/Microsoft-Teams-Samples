namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LogEventRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// Log event repository details.
    /// </summary>
    public class LogEventRepository : BaseRepository<EventLogEntity>, ILogEventRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store Learning path.
        /// </summary>
        private const string TableName = "UserEventLogs";

        /// <summary>
        /// Represents the partitionKey used to store Learning configurations.
        /// </summary>
        private const string TablePartitionKey = "UserEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventRepository"/> class.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public LogEventRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<LogEventRepository> logger)
            : base(options?.Value.ConnectionString, TableName, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Add a event log.
        /// </summary>
        /// <param name="entity">Event log details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> AddEventLog(EventLogEntity entity)
        {
            await this.EnsureInitializedAsync();
            return await this.CreateAsync(entity);
        }

        /// <summary>
        /// Get all log event details by learning id.
        /// </summary>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Reruns the all log event details by learning id.</returns>
        public async Task<IEnumerable<EventLogEntity>> GetLogEventLearningIdAsync(string learningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(EventLogEntity.LearningContentId)} eq '{learningId}'";
            return await this.GetWithFilterAsync<EventLogEntity>(query);
        }
    }
}