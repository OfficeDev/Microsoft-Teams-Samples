namespace Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.FeedbackRepository
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing and deleting user feedback.
    /// </summary>
    public interface IFeedbackRepository
    {
        /// <summary>
        /// Inserts or updates a new user feedback details.
        /// </summary>
        /// <param name="feedbackEntity">The feedback details.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<bool> CreateFeedbackAsync(FeedbackEntity feedbackEntity);

        /// <summary>
        /// Get feedback details based on id.
        /// </summary>
        /// <param name="feedbackId">The unique id of each feedback content.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<FeedbackEntity> GetFeedbackByIdAsync(string feedbackId);

        /// <summary>
        /// Get feedback details based on learning content id.
        /// </summary>
        /// <param name="learningId">The unique id of each learning content content.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<FeedbackEntity>> GetFeedbackByLearningContentIdAsync(string learningId);

        /// <summary>
        /// Get all user feedbacks details.
        /// </summary>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        Task<IEnumerable<FeedbackEntity>> GetAllFeedbacksAsync();
    }
}