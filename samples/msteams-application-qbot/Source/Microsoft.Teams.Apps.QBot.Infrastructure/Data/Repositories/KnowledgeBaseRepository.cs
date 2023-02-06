namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// KnowledgeBase Repository.
    /// </summary>
    internal class KnowledgeBaseRepository : IKnowledgeBaseRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<KnowledgeBaseRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public KnowledgeBaseRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<KnowledgeBaseRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
        {
            try
            {
                var entity = this.mapper.Map<KnowledgeBaseEntity>(knowledgeBase);
                this.dbContext.KnowledgeBases.Add(entity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add knowledge base: {knowledgeBase.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteKnowledgeBaseAsync(string knowledgeBaseId)
        {
            var knowledgeBase = await this.dbContext.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.KnowledgeBaseNotFound, $"KnowledgeBase {knowledgeBaseId} not found!");
            }

            try
            {
                this.dbContext.KnowledgeBases.Remove(knowledgeBase);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to delete knowledge base: {knowledgeBase.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync()
        {
            IEnumerable<KnowledgeBaseEntity> knowledgeBaseEntities = this.dbContext.KnowledgeBases;
            IEnumerable<KnowledgeBase> knowledgeBases = this.mapper.Map<IEnumerable<KnowledgeBase>>(knowledgeBaseEntities);
            return Task.FromResult(knowledgeBases);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesForUserAsync(string userId)
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            IEnumerable<KnowledgeBaseEntity> knowledgeBaseEntities = this.dbContext.KnowledgeBases;
            IEnumerable<KnowledgeBase> knowledgeBases = this.mapper.Map<IEnumerable<KnowledgeBase>>(knowledgeBaseEntities);
            return Task.FromResult(knowledgeBases.Where(kb => userId.Equals(kb.OwnerUserId)));
        }

        /// <inheritdoc/>
        public async Task<KnowledgeBase> GetKnowledgeBaseAsync(string knowledgeBaseId)
        {
            var knowledgeBaseEntity = await this.dbContext.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBaseEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.KnowledgeBaseNotFound, $"GetKnowledgeBaseAsync - KnowledgeBase {knowledgeBaseId} not found!");
            }

            return this.mapper.Map<KnowledgeBase>(knowledgeBaseEntity);
        }

        /// <inheritdoc/>
        public Task UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
        {
            var cachedKnowledgeBase = this.dbContext.KnowledgeBases.Find(knowledgeBase.Id);

            // Throw if entity doesn't exist
            if (cachedKnowledgeBase == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.KnowledgeBaseNotFound, $"UpdateKnowledgeBaseAsync - KnowledgeBase {knowledgeBase.Id} not found!");
            }

            // Update if it exists.
            var knowledgeBaseEntity = this.mapper.Map<KnowledgeBaseEntity>(knowledgeBase);
            cachedKnowledgeBase.Name = knowledgeBaseEntity.Name;
            cachedKnowledgeBase.OwnerUserId = knowledgeBaseEntity.OwnerUserId;

            try
            {
                this.dbContext.KnowledgeBases.Update(cachedKnowledgeBase);
                return this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update knowledge base: {knowledgeBase.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}
