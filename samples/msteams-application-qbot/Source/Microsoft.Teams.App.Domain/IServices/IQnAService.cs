namespace Microsoft.Teams.Apps.QBot.Domain.IServices
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// QnA service contract.
    /// </summary>
    public interface IQnAService
    {
        /// <summary>
        /// Fetched answer from QnA service.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="logicalKnowledgeBaseId">Logical knowledge base id.</param>
        /// <returns>Suggested Answer.</returns>
        Task<SuggestedAnswer> GetAnswerAsync(Question question, string logicalKnowledgeBaseId);

        /// <summary>
        /// Posts QnA pair to QnA service.
        /// </summary>
        /// <param name="question">Quesiton.</param>
        /// <param name="answer">Answer.</param>
        /// <param name="logicalKnowledgeBaseId">Logical knowledge base id.</param>
        /// <returns>Operation Id.</returns>
        Task<string> PostQnAPairAsync(Question question, Answer answer, string logicalKnowledgeBaseId);

        /// <summary>
        /// Updates an existing QnA Pair with alternate question.
        /// </summary>
        /// <param name="question">Quesiton.</param>
        /// <param name="suggestedAnswer">Suggested answer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateQnAPairAsync(Question question, SuggestedAnswer suggestedAnswer);

        /// <summary>
        /// Publishes all changes in test index of a knowledgebase to its prod index.
        /// </summary>
        /// <returns>Async task.</returns>
        Task PublishKbAsync();
    }
}
