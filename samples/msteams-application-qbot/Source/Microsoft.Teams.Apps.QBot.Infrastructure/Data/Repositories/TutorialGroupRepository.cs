namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories
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
    /// TutorialGroup Repository Impl.
    /// </summary>
    internal sealed class TutorialGroupRepository : ITutorialGroupRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<TutorialGroupRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TutorialGroupRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public TutorialGroupRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<TutorialGroupRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddTutorialGroupAsync(TutorialGroup tutorialGroup)
        {
            var course = await this.dbContext.Courses.FindAsync(tutorialGroup.CourseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.AddTutorialGroupAsync)}: Course not found!");
            }

            var tutorialGroupEntity = this.mapper.Map<TutorialGroupEntity>(tutorialGroup);
            course.TutorialGroups.Add(tutorialGroupEntity);

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add tutorial group. Tutorial group: {tutorialGroup.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task AddTutorialGroupsAsync(IEnumerable<TutorialGroup> tutorialGroups)
        {
            var course = await this.dbContext.Courses.FindAsync(tutorialGroups.First().CourseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.AddTutorialGroupsAsync)}: Course not found!");
            }

            var tutorialGroupEntity = this.mapper.Map<TutorialGroupEntity>(tutorialGroups.First());
            course.TutorialGroups.Add(tutorialGroupEntity);

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add tutorial groups. Tutorial group count: {tutorialGroups.Count()}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteTutorialGroupAsync(string tutorialGroupId)
        {
            var tutorialGroup = await this.dbContext.TutorialGroups.FindAsync(tutorialGroupId);
            if (tutorialGroup == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.TutorialGroupNotFound, $"{nameof(this.DeleteTutorialGroupAsync)}: Tutorial group not found!");
            }

            this.dbContext.Remove(tutorialGroup);

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to delete tutorial group. Tutorial group: {tutorialGroupId}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TutorialGroup>> GetAllTutorialGroupsAsync(string courseId)
        {
            var course = this.dbContext.Courses
                .Include(c => c.TutorialGroups)
                .FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetAllTutorialGroupsAsync)}: Course not found!");
            }

            var groups = this.mapper.Map<IEnumerable<TutorialGroup>>(course.TutorialGroups);
            return Task.FromResult(groups);
        }

        /// <inheritdoc/>
        public async Task<TutorialGroup> GetTutorialGroupAsync(string tutorialGroupId)
        {
            var tutorialGroupEntity = await this.dbContext.TutorialGroups.FindAsync(tutorialGroupId);
            if (tutorialGroupEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.TutorialGroupNotFound, $"{nameof(this.GetTutorialGroupAsync)}: Tutorial group not found!");
            }

            return this.mapper.Map<TutorialGroup>(tutorialGroupEntity);
        }

        /// <inheritdoc/>
        public async Task UpdateTutorialGroupAsync(TutorialGroup tutorialGroup)
        {
            var cachedEntity = await this.dbContext.TutorialGroups.FindAsync(tutorialGroup.Id);
            if (cachedEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.TutorialGroupNotFound, $"{nameof(this.UpdateTutorialGroupAsync)}: Tutorial group not found!");
            }

            var newEntity = this.mapper.Map<TutorialGroupEntity>(tutorialGroup);
            cachedEntity.DisplayName = newEntity.DisplayName;
            this.dbContext.Update(cachedEntity);

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update tutorial groups. Tutorial group: {tutorialGroup.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}
