namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.FeedbackRepository
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This class represent various methods the all feedback repository details.
    /// </summary>
    public class FeedbackRepository : BaseRepository<FeedbackEntity>, IFeedbackRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store Learning configurations.
        /// </summary>
        private const string FeedbackTable = "FeedbackEntity";

        /// <summary>
        /// Represents the partitionKey used to store Learning configurations.
        /// </summary>
        private const string TablePartitionKey = "Feedback";

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRepository"/> class.
        /// </summary>
        /// <param name="options">Entity represents bot options.</param>
        /// <param name="logger">Entity represents logger object.</param>
        public FeedbackRepository(
            IOptions<BlobStorageSetting> options,
            ILogger<FeedbackRepository> logger)
            : base(options?.Value.ConnectionString, FeedbackTable, TablePartitionKey, logger)
        {
        }

        /// <summary>
        /// Inserts or updates a new user feedback details.
        /// </summary>
        /// <param name="feedbackEntity">The feedback details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<bool> CreateFeedbackAsync(FeedbackEntity feedbackEntity) 
        {
            if (feedbackEntity == null)
            {
                throw new ArgumentNullException(nameof(feedbackEntity), "The feedbackEntity details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.CreateAsync(feedbackEntity);
        }

        /// <summary>
        /// Get feedback details based on ID.
        /// </summary>
        /// <param name="feedbackId">The unique id of each feedback content.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<FeedbackEntity> GetFeedbackByIdAsync(string feedbackId)
        {
            await this.EnsureInitializedAsync();
            return await this.GetAsync<FeedbackEntity>(feedbackId);
        }

        /// <summary>
        /// Get feedback details based on learning content ID.
        /// </summary>
        /// <param name="learningId">The unique id of each learning content content.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<IEnumerable<FeedbackEntity>> GetFeedbackByLearningContentIdAsync(string learningId)
        {
            await this.EnsureInitializedAsync();
            string query = $"{nameof(FeedbackEntity.LearningContentId)} eq '{learningId}'";
            return await this.GetRecordsByPartitionkeyAndFilterQueryAsync<FeedbackEntity>(query);
        }

        /// <summary>
        /// Get all user feedbacks details.
        /// </summary>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        public async Task<IEnumerable<FeedbackEntity>> GetAllFeedbacksAsync()
        {
            await this.EnsureInitializedAsync();
            return await this.GetAllAsync<FeedbackEntity>();
        }
    }
}