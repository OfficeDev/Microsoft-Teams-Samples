namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// QBot Service interface.
    /// </summary>
    public interface IQBotService : IQuestionReader
    {
        /// <summary>
        /// Persists new question.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <returns><see cref="Question"/>.</returns>
        Task<Question> AddQuestionAsync(Question question);

        /// <summary>
        /// Posts an answer to a question.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <returns><see cref="Answer"/>.</returns>
        Task<Answer> PostAnswerAsync(Answer answer);

        /// <summary>
        /// Posts Bot suggested answer to a question.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <param name="suggestedAnswer">Suggested answer.</param>
        /// <returns><see cref="Answer"/></returns>
        Task<Answer> PostSuggestedAnswerAsync(Answer answer, SuggestedAnswer suggestedAnswer);

        /// <summary>
        /// Removes Bot suggested answer and updates the message.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <param name="userId">User who marked it as not helpful.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task MarkSuggestedAnswerNotHelpfulAsync(Answer answer, string userId);
    }
}
