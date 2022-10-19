namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserReactionRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing user reaction.
    /// </summary>
    public interface IUserReactionRepository
    {
        /// <summary>
        /// Inserts new user reaction details.
        /// </summary>
        /// <param name="entity">The user reaction content details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> AddUserReactionAsync(UserReactionEntity entity);

        /// <summary>
        /// Update new user reaction details.
        /// </summary>
        /// <param name="entity">The user reaction content details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> UpdateUserReactionAsync(UserReactionEntity entity);

        /// <summary>
        /// Get user reaction  details based on assigned user id and learning content id.
        /// </summary>
        /// <param name="userAadId">The unique id of user.</param>
        /// <param name="learningId">The unique id of learning article.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<UserReactionEntity>> GetUserReactionByLearningContentIdAsync(string userAadId, string learningId);

        /// <summary>
        /// Get user reaction details by learning id.
        /// </summary>
        /// <param name="learningId">Id of learning content.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<UserReactionEntity>> GetUserReactionByLearningIdAsync(string learningId);
    }
}