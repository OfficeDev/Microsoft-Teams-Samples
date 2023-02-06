namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// KnowledgeBase write interface.
    /// </summary>
    public interface IKnowledgeBaseWriter
    {
        /// <summary>
        /// Adds KB to the db.
        /// </summary>
        /// <param name="knowledgeBase">Knowledge base.</param>
        /// <returns>Async task.</returns>
        Task<KnowledgeBase> AddKnowledgeBaseAsync(KnowledgeBase knowledgeBase);

        /// <summary>
        /// Updates KB in the db.
        /// </summary>
        /// <param name="knowledgeBase">Updated knowledge base.</param>
        /// <returns>Async task.</returns>
        Task UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase);

        /// <summary>
        /// Deletes KB from the db.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledge base id.</param>
        /// <returns>Async task.</returns>
        Task DeleteKnowledgeBaseAsync(string knowledgeBaseId);
    }
}
