namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserReactionRepository
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// User reaction repository details class.
    /// </summary>
    public class UserReactionRepository : BaseRepository<UserReactionEntity>, IUserReactionRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store Learning path.
        /// </summary>
        private const string TableName = "UserReactionEntity";

        /// <summary>
        /// Represents the partitionKey used to store Learning configurations.
        /// </summary>
        private const string TablePartitionKey = "UserReaction";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReactionRepository"/> class.
        /// User reaction repository details.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public UserReactionRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<UserReactionRepository> logger)
            : base(options?.Value.ConnectionString, TableName, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Add a new user reaction details.
        /// </summary>
        /// <param name="entity">User reaction details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> AddUserReactionAsync(UserReactionEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.CreateAsync(entity);
        }

        /// <summary>
        /// Get all user reaction details by learning content id.
        /// </summary>
        /// <param name="userAadId">User Aad id.</param>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns the all user reaction by learning content id.</returns>
        public async Task<IEnumerable<UserReactionEntity>> GetUserReactionByLearningContentIdAsync(string userAadId, string learningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(LearningPathEntity.UserAadId)} eq '{userAadId}' and {nameof(LearningPathEntity.LearningContentId)} eq '{learningId}'";
            return await this.GetWithFilterAsync<UserReactionEntity>(query);
        }

        /// <summary>
        /// Get all user reaction details by learning id.
        /// </summary>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns the all user reaction by learning id.</returns>
        public async Task<IEnumerable<UserReactionEntity>> GetUserReactionByLearningIdAsync(string learningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(LearningPathEntity.LearningContentId)} eq '{learningId}'";
            return await this.GetWithFilterAsync<UserReactionEntity>(query);
        }

        /// <summary>
        /// Upadte user reaction details.
        /// </summary>
        /// <param name="entity">User reaction details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> UpdateUserReactionAsync(UserReactionEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.UpdateAsync(entity);
        }
    }
}
