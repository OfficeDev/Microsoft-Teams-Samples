namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// KnowledgeBase reader.
    /// </summary>
    public interface IKnowledgeBaseReader
    {
        /// <summary>
        /// Gets Knowledge base.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledge base id.</param>
        /// <returns>Knowledge base.</returns>
        Task<KnowledgeBase> GetKnowledgeBaseAsync(string knowledgeBaseId);

        /// <summary>
        /// Gets all the knowledge bases.
        /// </summary>
        /// <returns>List of KBs.</returns>
        Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync();
    }
}
