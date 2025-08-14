namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LearningPath
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing user learning path.
    /// </summary>
    public interface ILearningPathRepository
    {
        /// <summary>
        /// Inserts new learning details.
        /// </summary>
        /// <param name="learningPathEntity">The learning path content details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> CreateLearningPathContentAsync(LearningPathEntity learningPathEntity);

        /// <summary>
        /// Updates a new learning details.
        /// </summary>
        /// <param name="learningPathEntity">The learning path content details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> UpdateLearningPathContentAsync(LearningPathEntity learningPathEntity);

        /// <summary>
        /// Get learning details based on assigned user id.
        /// </summary>
        /// <param name="userAadId">The unique id of user.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<LearningPathEntity>> GetLearningPathByUserIdAsync(string userAadId);

        /// <summary>
        /// Get learning details based on assigned user id.
        /// </summary>
        /// <param name="LearningId">The learning id of user.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<LearningPathEntity>> GetLearningPathByLearningIdAsync(string LearningId);

        /// <summary>
        /// Get learning content based on user id and learning id.
        /// </summary>
        /// <param name="userAadId">Uesr Aad id.</param>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns the learning path by user id and learning id.</returns>
        Task<IEnumerable<LearningPathEntity>> GetLearningPathByUserIdAndLearningIdAsync(string userAadId, string learningId);
    }
}