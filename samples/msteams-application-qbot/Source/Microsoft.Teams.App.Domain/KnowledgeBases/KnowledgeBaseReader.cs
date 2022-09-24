namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Knowledge base reader implementation.
    /// </summary>
    internal sealed class KnowledgeBaseReader : IKnowledgeBaseReader
    {
        private readonly IKnowledgeBaseRepository kbRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseReader"/> class.
        /// </summary>
        /// <param name="kbRepository">Knowledge base repository.</param>
        public KnowledgeBaseReader(IKnowledgeBaseRepository kbRepository)
        {
            this.kbRepository = kbRepository ?? throw new System.ArgumentNullException(nameof(kbRepository));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync()
        {
            return this.kbRepository.GetAllKnowledgeBasesAsync();
        }

        /// <inheritdoc/>
        public Task<KnowledgeBase> GetKnowledgeBaseAsync(string knowledgeBaseId)
        {
            if (knowledgeBaseId is null)
            {
                throw new ArgumentNullException(nameof(knowledgeBaseId));
            }

            return this.kbRepository.GetKnowledgeBaseAsync(knowledgeBaseId);
        }
    }
}
