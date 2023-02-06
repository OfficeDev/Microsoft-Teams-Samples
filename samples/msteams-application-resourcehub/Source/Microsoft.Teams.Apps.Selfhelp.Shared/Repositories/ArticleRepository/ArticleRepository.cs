namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;

    /// <summary>
    /// This class manages storage operations related to learning content.
    /// </summary>
    public class ArticleRepository : BaseRepository<ArticleEntity>, IArticleRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store Learning content.
        /// </summary>
        private const string LearningContentTable = "LearningEntity";

        /// <summary>
        /// Represents the partitionKey used to store Learning content.
        /// </summary>
        private const string TablePartitionKey = "Learning";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleRepository"/> class.
        /// Article repository entity.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public ArticleRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<ArticleRepository> logger)
            : base(options?.Value.ConnectionString, LearningContentTable, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Create a learning content entity in the table storage.
        /// </summary>
        /// <param name="learningEntity">Entity to be created.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> CreateLearningContentAsync(ArticleEntity learningEntity)
        {
            if (learningEntity == null)
            {
                throw new ArgumentNullException(nameof(learningEntity), "The learningEntity details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.CreateAsync(learningEntity);
        }

        /// <summary>
        /// Delete a learning content entity in a table storage.
        /// </summary>
        /// <param name="learningEntity">Entity to be deleted.</param>
        /// <returns>Returns true if details deleted successfully,Else returns false.</returns>
        public async Task<bool> DeleteLearningContentAsync(ArticleEntity learningEntity)
        {
            if (learningEntity == null)
            {
                throw new ArgumentNullException(nameof(learningEntity), "The learningEntity configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.DeleteAsync(learningEntity);
        }

        /// <summary>
        /// Get all learning content from storage.
        /// </summary>
        /// <returns>Returns list of article entity.</returns>
        public async Task<IEnumerable<ArticleEntity>> GetAllLearningContentesAsync()
        {
            await this.EnsureInitializedAsync();
            return await this.GetAllAsync<ArticleEntity>();
        }

        /// <summary>
        /// Get learning content from the storage based on learning id.
        /// </summary>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns list of learning content entity.</returns>
        public async Task<ArticleEntity> GetLearningContentAsync(string learningId)
        {
            await this.EnsureInitializedAsync();
            return await this.GetAsync<ArticleEntity>(learningId);
        }

        /// <summary>
        /// Update the learning content.
        /// </summary>
        /// <param name="learningEntity">Data body of learning content.</param>
        /// <returns>Returns updated learning content entity.</returns>
        public async Task<bool> UpdateLearningContentAsync(ArticleEntity learningEntity)
        {
            if (learningEntity == null)
            {
                throw new ArgumentNullException(nameof(learningEntity), "The learningEntity configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.UpdateAsync(learningEntity);
        }

        /// <summary>
        /// Get all learning content by its type.
        /// </summary>
        /// <param name="selectionType">Selection type of learning content.</param>
        /// <returns>Return all learning content by type.</returns>
        public async Task<IEnumerable<ArticleEntity>> GetLearningContentesByTypeAsync(SelectionType selectionType)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(ArticleEntity.SectionType)} eq '{selectionType.ToString()}'";
            return await this.GetRecordsByPartitionkeyAndFilterQueryAsync<ArticleEntity>(query);
        }
    }
}