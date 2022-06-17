// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="Course"/> repository.
    /// </summary>
    public interface IKnowledgeBaseRepository
    {
        /// <summary>
        /// Adds KB to the db.
        /// </summary>
        /// <param name="knowledgeBase">Knowledge base object.</param>
        /// <returns>Async task.</returns>
        Task AddKnowledgeBaseAsync(KnowledgeBase knowledgeBase);

        /// <summary>
        /// Updates KB in the db.
        /// </summary>
        /// <param name="knowledgeBase">Updated knowledge base object.</param>
        /// <returns>Async task.</returns>
        Task UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase);

        /// <summary>
        /// Deletes KB from the db.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledge base id.</param>
        /// <returns>Async task.</returns>
        Task DeleteKnowledgeBaseAsync(string knowledgeBaseId);

        /// <summary>
        /// Gets KB.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledge base id.</param>
        /// <returns>Knowledge base.</returns>
        Task<KnowledgeBase> GetKnowledgeBaseAsync(string knowledgeBaseId);

        /// <summary>
        /// Gets all the knowledge bases defined.
        /// </summary>
        /// <returns>List of KBs.</returns>
        Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync();

        /// <summary>
        /// Gets all the knowledge bases where the user is owner.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>List of KBs.</returns>
        Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesForUserAsync(string userId);
    }
}
