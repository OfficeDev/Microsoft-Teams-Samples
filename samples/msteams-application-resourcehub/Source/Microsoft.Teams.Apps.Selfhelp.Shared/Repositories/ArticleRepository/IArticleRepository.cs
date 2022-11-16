namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing and deleting learning configurations.
    /// </summary>
    public interface IArticleRepository
    {
        /// <summary>
        /// Inserts or updates a new learning details.
        /// </summary>
        /// <param name="learningEntity">The learning content details.</param>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<bool> CreateLearningContentAsync(ArticleEntity learningEntity);

        /// <summary>
        /// Updates a new learning details.
        /// </summary>
        /// <param name="learningEntity">The learning content details.</param>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<bool> UpdateLearningContentAsync(ArticleEntity learningEntity);

        /// <summary>
        /// Delete learning details.
        /// </summary>
        /// <param name="learningEntity">The learning content details.</param>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<bool> DeleteLearningContentAsync(ArticleEntity learningEntity);

        /// <summary>
        /// Get learning details based on ID.
        /// </summary>
        /// <param name="learningId">The unique id of each learning content.</param>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<ArticleEntity> GetLearningContentAsync(string learningId);

        /// <summary>
        /// Get all learning details.
        /// </summary>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<IEnumerable<ArticleEntity>> GetAllLearningContentesAsync();

        /// <summary>
        /// Get all learning details by partitionkey and item selection type.
        /// </summary>
        /// <returns>Returns true if details inserted or updated successfully. Else returns false.</returns>
        Task<IEnumerable<ArticleEntity>> GetLearningContentesByTypeAsync(SelectionType selectionType);
    }
}