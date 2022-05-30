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
    /// Question Repository Impl.
    /// </summary>
    internal sealed class QuestionRepository : IQuestionRespository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<QuestionRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public QuestionRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<QuestionRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddAnswerAsync(Answer answer)
        {
            if (answer is null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            var questionEntity = this.dbContext.Questions.Find(answer.QuestionId);
            if (questionEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.QuestionNotFound, $"{nameof(this.AddAnswerAsync)}: Question {answer.QuestionId} not found!");
            }

            // Set answer
            var answerEntity = this.mapper.Map<AnswerEntity>(answer);
            questionEntity.Answer = answerEntity;

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add answer. Question Id: {answer.QuestionId}, Answer Id: {answer.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task AddQuestionAsync(Question question)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            // Add question.
            var questionEntity = this.mapper.Map<QuestionEntity>(question);
            this.dbContext.Add(questionEntity);

            try
            {
                // Save
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add question. Question Id: {question.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteQuestionAsync(string questionId)
        {
            if (string.IsNullOrWhiteSpace(questionId))
            {
                throw new ArgumentException($"'{nameof(questionId)}' cannot be null or whitespace", nameof(questionId));
            }

            var questionEntity = this.dbContext.Questions.Find(questionId);
            if (questionEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.QuestionNotFound, $"{nameof(this.DeleteQuestionAsync)}: Question {questionId} not found!");
            }

            try
            {
                // Delete and save.
                this.dbContext.Questions.Remove(questionEntity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to delete question. Question Id: {questionId}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Question>> GetAllQuestionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"'{nameof(userId)}' cannot be null or whitespace", nameof(userId));
            }

            var questionEntities = this.dbContext.Questions
                .Where(q => q.AuthorId == userId)
                .ToList();

            var questions = this.mapper.Map<IEnumerable<Question>>(questionEntities);
            return Task.FromResult(questions);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Question>> GetAllQuestionsAsync(string courseId, string channelId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty", nameof(courseId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty", nameof(channelId));
            }

            var questionEntities = this.dbContext.Questions
                .Where(q => q.ChannelId == channelId && q.CourseId == courseId)
                .ToList();

            var questions = this.mapper.Map<IEnumerable<Question>>(questionEntities);
            return Task.FromResult(questions);
        }

        /// <inheritdoc/>
        public async Task<Question> GetQuestionAsync(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
            {
                throw new ArgumentException($"'{nameof(questionId)}' cannot be null or empty.", nameof(questionId));
            }

            var questionEntity = this.dbContext.Questions.Find(questionId);
            if (questionEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.QuestionNotFound, $"{nameof(this.GetQuestionAsync)}: Question {questionId} not found!");
            }

            return await Task.FromResult(this.mapper.Map<Question>(questionEntity));
        }

        /// <inheritdoc/>
        public async Task UpdateQuestionAsync(Question question)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            var questionEntity = this.dbContext.Questions.Find(question.Id);
            if (questionEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.QuestionNotFound, $"{nameof(this.UpdateQuestionAsync)}: Question {question.Id} not found!");
            }

            var newEntity = this.mapper.Map<QuestionEntity>(question);

            // Note: We could add question text property and update that if fetching messages becomes a performance issue.
            questionEntity.InitialResponseMessageId = newEntity.InitialResponseMessageId;
            questionEntity.AnswerId = newEntity.AnswerId;

            try
            {
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update question. Question Id: {question.Id}.";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}
