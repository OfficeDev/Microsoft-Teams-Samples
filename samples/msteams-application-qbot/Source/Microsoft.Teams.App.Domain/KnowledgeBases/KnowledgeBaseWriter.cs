namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Knowldge base writer implementation.
    /// </summary>
    internal sealed class KnowledgeBaseWriter : IKnowledgeBaseWriter
    {
        private readonly IKnowledgeBaseValidator validator;
        private readonly IKnowledgeBaseRepository kbRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseWriter"/> class.
        /// </summary>
        /// <param name="validator">KB validator.</param>
        /// <param name="kbRepository">KB repository.</param>
        public KnowledgeBaseWriter(
            IKnowledgeBaseValidator validator,
            IKnowledgeBaseRepository kbRepository)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.kbRepository = kbRepository ?? throw new ArgumentNullException(nameof(kbRepository));
        }

        /// <inheritdoc/>
        public async Task<KnowledgeBase> AddKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
        {
            if (!this.validator.IsValid(knowledgeBase))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidKnowledgeBase, "Invalid knowledge base");
            }

            // Generate unique id.
            knowledgeBase.Id = Guid.NewGuid().ToString();
            await this.kbRepository.AddKnowledgeBaseAsync(knowledgeBase);
            return knowledgeBase;
        }

        /// <inheritdoc/>
        public async Task UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
        {
            if (!this.validator.IsValid(knowledgeBase) || string.IsNullOrEmpty(knowledgeBase.Id))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidKnowledgeBase, "Invalid knowledge base");
            }

            await this.kbRepository.UpdateKnowledgeBaseAsync(knowledgeBase);
        }

        /// <inheritdoc/>
        public async Task DeleteKnowledgeBaseAsync(string knowledgeBaseId)
        {
            if (string.IsNullOrEmpty(knowledgeBaseId))
            {
                throw new ArgumentException($"'{nameof(knowledgeBaseId)}' cannot be null or empty.", nameof(knowledgeBaseId));
            }

            await this.kbRepository.DeleteKnowledgeBaseAsync(knowledgeBaseId);
        }
    }
}
