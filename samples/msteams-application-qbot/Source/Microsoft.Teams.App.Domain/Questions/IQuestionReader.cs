namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Question reader interface.
    /// </summary>
    public interface IQuestionReader
    {
        /// <summary>
        /// Gets question from the db.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <param name="questionId">Question id.</param>
        /// <returns>Question.</returns>
        Task<Question> GetQuestionAsync(string courseId, string channelId, string questionId);

        /// <summary>
        /// Gets response to a question.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <param name="questionId">Question id.</param>
        /// <returns>List of question responses.</returns>
        Task<IEnumerable<QuestionResponse>> GetQuestionResponsesAsync(string courseId, string channelId, string questionId);

        /// <summary>
        /// Gets question response.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <param name="questionId">Question id.</param>
        /// <param name="responseMessageId">Response message id.</param>
        /// <returns>Question response.</returns>
        Task<QuestionResponse> GetQuestionResponseAsync(string courseId, string channelId, string questionId, string responseMessageId);

        /// <summary>
        /// Gets all the question asked in a channel of a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <returns>List of questions.</returns>
        Task<IEnumerable<Question>> GetQuestionsAsync(string courseId, string channelId);

        /// <summary>
        /// Gets all the questions asked by the user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>List of questions.</returns>
        Task<IEnumerable<Question>> GetQuestionsAsync(string userId);
    }
}