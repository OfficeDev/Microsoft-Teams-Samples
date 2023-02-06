namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="Question"/> repository.
    /// </summary>
    public interface IQuestionRespository
    {
        /// <summary>
        /// Add question to DB.
        /// </summary>
        /// <param name="question">Question object.</param>
        /// <returns>Async task.</returns>
        Task AddQuestionAsync(Question question);

        /// <summary>
        /// Gets question object from DB.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <returns>Question.</returns>
        Task<Question> GetQuestionAsync(string questionId);

        /// <summary>
        /// Gets all the questions posted by the user from the DB.
        /// </summary>
        /// <param name="userId">User's AAD Id.</param>
        /// <returns>List of questions asked by the user.</returns>
        Task<IEnumerable<Question>> GetAllQuestionsAsync(string userId);

        /// <summary>
        /// Gets all the questions posted in a channel of a course from the DB.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <returns>List of questions.</returns>
        Task<IEnumerable<Question>> GetAllQuestionsAsync(string courseId, string channelId);

        /// <summary>
        /// Updates question in the DB.
        /// </summary>
        /// <param name="question">Updated question object.</param>
        /// <returns>Async task.</returns>
        Task UpdateQuestionAsync(Question question);

        /// <summary>
        /// Deletes question from the DB.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <returns>Async task.</returns>
        Task DeleteQuestionAsync(string questionId);

        /// <summary>
        /// Adds answer to the DB.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <returns>Async task.</returns>
        Task AddAnswerAsync(Answer answer);
    }
}
