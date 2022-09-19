namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LearningPath
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This class manages storage operations related to learning path.
    /// </summary>
    public class LearningPathRepository : BaseRepository<LearningPathEntity>, ILearningPathRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store Learning path.
        /// </summary>
        private const string LearningPathTable = "LearningPathEntity";

        /// <summary>
        /// Represents the partitionKey used to store Learning configurations.
        /// </summary>
        private const string TablePartitionKey = "LearningPath";

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningPathRepository"/> class.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public LearningPathRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<LearningPathRepository> logger)
            : base(options?.Value.ConnectionString, LearningPathTable, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Create the learning content path.
        /// </summary>
        /// <param name="learningPathEntity">learning path details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> CreateLearningPathContentAsync(LearningPathEntity learningPathEntity)
        {
            if (learningPathEntity == null)
            {
                throw new ArgumentNullException(nameof(learningPathEntity), "The learningPathEntity details should be provided");
            }

            var learningEntity = await GetLearningPathByUserIdAndLearningIdAsync(learningPathEntity.LearningContentId, learningPathEntity.UserAadId);
            if (learningEntity.Count() == 0)
            {
                learningPathEntity.PartitionKey = Guid.NewGuid().ToString();
                learningPathEntity.RowKey = Guid.NewGuid().ToString();
                await this.EnsureInitializedAsync();
                return await this.CreateAsync(learningPathEntity);
            }
            else
            {
                return await this.UpdateLearningPathContentAsync(learningEntity.FirstOrDefault());
            }
        }

        /// <summary>
        /// Get all learning path by user id.
        /// </summary>
        /// <param name="userAadId">user Aad id.</param>
        /// <returns>Return the learning path details by user.</returns>
        public async Task<IEnumerable<LearningPathEntity>> GetLearningPathByUserIdAsync(string userAadId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(LearningPathEntity.UserAadId)} eq '{userAadId}'";
            return await this.GetWithFilterAsync<LearningPathEntity>(query);
        }

        /// <summary>
        /// Get all learning path by learning id.
        /// </summary>
        /// <param name="LearningId">Id of learning content.</param>
        /// <returns>Return the learning path by learning id.</returns>
        public async Task<IEnumerable<LearningPathEntity>> GetLearningPathByLearningIdAsync(string LearningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(LearningPathEntity.LearningContentId)} eq '{LearningId}'";
            return await this.GetWithFilterAsync<LearningPathEntity>(query);
        }

        /// <summary>
        /// Get all learning path by user and learning content id.
        /// </summary>
        /// <param name="userAadId">User Aad id.</param>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns all learning path by user and by learning id.</returns>
        public async Task<IEnumerable<LearningPathEntity>> GetLearningPathByUserIdAndLearningIdAsync(string userAadId, string learningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(LearningPathEntity.UserAadId)} eq '{userAadId}' and {nameof(LearningPathEntity.LearningContentId)} eq '{learningId}'";
            return await this.GetWithFilterAsync<LearningPathEntity>(query);
        }

        /// <summary>
        /// Update the learning content path.
        /// </summary>
        /// <param name="learningPathEntity">learning path details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> UpdateLearningPathContentAsync(LearningPathEntity learningPathEntity)
        {
            if (learningPathEntity == null)
            {
                throw new ArgumentNullException(nameof(learningPathEntity), "The learningPathEntity configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.UpdateAsync(learningPathEntity);
        }
    }
}