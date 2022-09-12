namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Domain.Users;

    /// <summary>
    /// QBot service.
    /// </summary>
    internal class QBotService : IQBotService
    {
        private readonly IQuestionRespository questionRespository;
        private readonly ITeamsMessageService teamsMessageService;
        private readonly IQuestionValidator questionValidator;
        private readonly IAnswerValidator answerValidator;
        private readonly ICourseReader courseReader;
        private readonly IQnAService qnAService;
        private readonly IUserReaderService userReaderService;
        private readonly ILogger<QBotService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QBotService"/> class.
        /// </summary>
        /// <param name="questionValidator">Question validator.</param>
        /// <param name="answerValidator">Answer validator.</param>
        /// <param name="questionRespository">Question repository.</param>
        /// <param name="teamsMessageService">Teams message service.</param>
        /// <param name="courseReader">Course reader.</param>
        /// <param name="qnAService">QnA Service.</param>
        /// <param name="userReaderService">User profile reader  service.</param>
        /// <param name="logger">Logger.</param>
        public QBotService(
            IQuestionValidator questionValidator,
            IAnswerValidator answerValidator,
            IQuestionRespository questionRespository,
            ITeamsMessageService teamsMessageService,
            ICourseReader courseReader,
            IQnAService qnAService,
            IUserReaderService userReaderService,
            ILogger<QBotService> logger)
        {
            this.questionValidator = questionValidator ?? throw new ArgumentNullException(nameof(questionValidator));
            this.answerValidator = answerValidator ?? throw new ArgumentNullException(nameof(answerValidator));
            this.questionRespository = questionRespository ?? throw new ArgumentNullException(nameof(questionRespository));
            this.teamsMessageService = teamsMessageService ?? throw new ArgumentNullException(nameof(teamsMessageService));
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.qnAService = qnAService ?? throw new ArgumentNullException(nameof(qnAService));
            this.userReaderService = userReaderService ?? throw new ArgumentNullException(nameof(userReaderService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Question> AddQuestionAsync(Question question)
        {
            if (!this.questionValidator.IsValid(question))
            {
                throw new Exception("Invalid question");
            }

            question.Id = question.MessageId;

            // Add question.
            await this.questionRespository.AddQuestionAsync(question);

            // Fetch question message if not set.
            var course = await this.courseReader.GetCourseAsync(question.CourseId);
            if (string.IsNullOrEmpty(question.Message))
            {
                // Get question message if its not cached.
                question.Message = await this.teamsMessageService.GetQuestionMessageAsync(course.TeamAadObjectId, question.ChannelId, question.MessageId);
            }

            try
            {
                // Get answer from QnA service if KB is configured.
                if (!string.IsNullOrEmpty(course.KnowledgeBaseId))
                {
                    var suggestedAnswer = await this.qnAService.GetAnswerAsync(question, course.KnowledgeBaseId);
                    if (suggestedAnswer != null && suggestedAnswer.Score > 0)
                    {
                        question.InitialResponseMessageId = await this.teamsMessageService.PostSuggestAnswerAsync(question, suggestedAnswer);
                        await this.questionRespository.UpdateQuestionAsync(question);
                        return question;
                    }
                }
            }
            catch (QBotException exception)
            {
                // If the application fails to generate answer, it would fallback to tag educators and tutors.
                this.logger.LogWarning(exception, $"Failed to generate answer from QnA Service. Question Id: {question.Id}");
            }

            // Didn't get a relevant answer from QnA service / KB not configured, Post "select answer" message.
            var users = new List<User>();
            var members = await this.courseReader.GetAllMembersAsync(question.CourseId, new List<MemberRole>() { MemberRole.Educator, MemberRole.Tutor });
            foreach (var member in members)
            {
                var user = await this.userReaderService.GetUserAsync(member.AadId, false/*fetchProfilePic*/);
                user.TeamId = member.TeamId;
                users.Add(user);
            }

            question.InitialResponseMessageId = await this.teamsMessageService.PostSelectAnswerAsync(question, users);
            await this.questionRespository.UpdateQuestionAsync(question);
            return question;
        }

        /// <inheritdoc/>
        public async Task<Answer> PostAnswerAsync(Answer answer)
        {
            if (!this.answerValidator.IsValid(answer))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidAnswer, "Invalid answer object");
            }

            // Ensure question isn't answered.
            var question = await this.questionRespository.GetQuestionAsync(answer.QuestionId);
            if (!string.IsNullOrEmpty(question.AnswerId))
            {
                throw new QBotException(HttpStatusCode.Conflict, ErrorCode.QuestionMarkedAsAnswered, "Question is marked as answered, cannot update it.");
            }

            var userIds = new HashSet<string>
            {
                answer.AuthorId,
                answer.AcceptedById,
                question.AuthorId,
            };

            var users = await this.userReaderService.GetUsersAsync(userIds, true/*fetchProfilePic*/);

            // Post answer.
            answer.MessageId = await this.teamsMessageService.PostCorrectAnswerAsync(question, answer, users);

            // Store answer.
            answer.Id = answer.MessageId;
            await this.questionRespository.AddAnswerAsync(answer);

            // Update answer id and update question in Db.
            question.AnswerId = answer.Id;
            await this.questionRespository.UpdateQuestionAsync(question);

            // Read question message.
            var course = await this.courseReader.GetCourseAsync(question.CourseId);
            var channel = await this.courseReader.GetChannelAsync(question.CourseId, question.ChannelId);
            if (string.IsNullOrEmpty(question.Message))
            {
                // Get question message if its not cached.
                question.Message = await this.teamsMessageService.GetQuestionMessageAsync(course.TeamAadObjectId, question.ChannelId, question.MessageId);
            }

            // Update select answer message.
            await this.teamsMessageService.UpdateSelectAnswerMessageAsync(course, channel, question, answer);

            // Notify question author if the question is not accepted by the author.
            if (answer.AcceptedById != question.AuthorId)
            {
                await this.NotifyQuestionAuthorAsync(course, channel, question, answer);
            }

            // Notify answer author if the answer is not accepted by the author.
            if (answer.AuthorId != answer.AcceptedById)
            {
                await this.NotifyAnswerAuthorAsync(course, channel, question, answer);
            }

            // Update QnA Service
            try
            {
                // If knowledge base if configured, post QnA Pair.
                if (!string.IsNullOrEmpty(course.KnowledgeBaseId))
                {
                    await this.qnAService.PostQnAPairAsync(question, answer, course.KnowledgeBaseId);
                }
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to post QnA Pair. Question Id: {question.Id}.");
            }

            return answer;
        }

        /// <inheritdoc/>
        public async Task MarkSuggestedAnswerNotHelpfulAsync(Answer answer, string userId)
        {
            if (answer is null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
            }

            // Fetch question.
            var question = await this.GetQuestionAsync(answer.CourseId, answer.ChannelId, answer.QuestionId);

            // Fetch user profile
            var user = await this.userReaderService.GetUserAsync(userId, false/*fetchProfilePic*/);

            // Update answer.
            await this.teamsMessageService.UpdateSuggestedAnswerAsync(question, answer, user);
        }

        /// <inheritdoc/>
        public async Task<Answer> PostSuggestedAnswerAsync(Answer answer, SuggestedAnswer suggestedAnswer)
        {
            if (!this.answerValidator.IsValid(answer))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidAnswer, "Invalid answer object");
            }

            if (suggestedAnswer is null)
            {
                throw new ArgumentNullException(nameof(suggestedAnswer));
            }

            // Fetch question.
            var question = await this.GetQuestionAsync(answer.CourseId, answer.ChannelId, answer.QuestionId);
            question.AnswerId = answer.Id;

            // Store answer and updated question.
            await this.questionRespository.AddAnswerAsync(answer);
            await this.questionRespository.UpdateQuestionAsync(question);

            var userIds = new HashSet<string>
            {
                answer.AcceptedById,
                question.AuthorId,
            };

            var users = await this.userReaderService.GetUsersAsync(userIds, false/*fetchProfilePic*/);

            // Update existing answer.
            await this.teamsMessageService.PostCorrectAnswerAsync(question, answer, users);

            // Notify question author if the question is not accepted by the author.
            if (answer.AcceptedById != question.AuthorId)
            {
                var course = await this.courseReader.GetCourseAsync(answer.CourseId);
                var channel = await this.courseReader.GetChannelAsync(answer.CourseId, answer.ChannelId);
                await this.NotifyQuestionAuthorAsync(course, channel, question, answer);
            }

            try
            {
                // Update QnA Service.
                await this.qnAService.UpdateQnAPairAsync(question, suggestedAnswer);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, "Failed to update QnA Pair.");
            }

            return answer;
        }

        /// <inheritdoc/>
        public async Task<Question> GetQuestionAsync(string courseId, string channelId, string questionId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(questionId))
            {
                throw new ArgumentException($"'{nameof(questionId)}' cannot be null or empty.", nameof(questionId));
            }

            var question = await this.questionRespository.GetQuestionAsync(questionId);
            var course = await this.courseReader.GetCourseAsync(courseId);
            question.Message = await this.teamsMessageService.GetQuestionMessageAsync(course.TeamAadObjectId, question.ChannelId, question.MessageId);
            return question;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuestionResponse>> GetQuestionResponsesAsync(string courseId, string channelId, string questionId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(questionId))
            {
                throw new ArgumentException($"'{nameof(questionId)}' cannot be null or empty.", nameof(questionId));
            }

            var course = await this.courseReader.GetCourseAsync(courseId);

            return await this.teamsMessageService.GetQuestionResponsesAsync(course.TeamAadObjectId, channelId, questionId);
        }

        /// <inheritdoc/>
        public async Task<QuestionResponse> GetQuestionResponseAsync(string courseId, string channelId, string questionId, string responseMessageId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or empty.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(questionId))
            {
                throw new ArgumentException($"'{nameof(questionId)}' cannot be null or empty.", nameof(questionId));
            }

            if (string.IsNullOrEmpty(responseMessageId))
            {
                throw new ArgumentException($"'{nameof(responseMessageId)}' cannot be null or empty.", nameof(responseMessageId));
            }

            var course = await this.courseReader.GetCourseAsync(courseId);
            return await this.teamsMessageService.GetQuestionResponseAsync(course.TeamAadObjectId, channelId, questionId, responseMessageId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Question>> GetQuestionsAsync(string courseId, string channelId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or whitespace", nameof(courseId));
            }

            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or whitespace", nameof(channelId));
            }

            var course = await this.courseReader.GetCourseAsync(courseId);
            var questions = await this.questionRespository.GetAllQuestionsAsync(courseId: courseId, channelId: channelId);

            var messageIds = questions.Select(question => question.MessageId);
            var messageDictionary = await this.teamsMessageService.GetQuestionsMessageAsync(course.TeamAadObjectId, channelId, messageIds);
            foreach (var question in questions)
            {
                if (messageDictionary.TryGetValue(question.MessageId, out var message))
                {
                    question.Message = message;
                }
            }

            return questions;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Question>> GetQuestionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"'{nameof(userId)}' cannot be null or whitespace", nameof(userId));
            }

            // Note(guptaa): We currently do not return question message.
            return this.questionRespository.GetAllQuestionsAsync(userId);
        }

        private async Task NotifyQuestionAuthorAsync(Course course, Channel channel, Question question, Answer answer)
        {
            try
            {
                await this.teamsMessageService.NotifyQuestionAnsweredAsync(course, channel, question, answer);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, "Failed to notify question author.");
            }
        }

        private async Task NotifyAnswerAuthorAsync(Course course, Channel channel, Question question, Answer answer)
        {
            try
            {
                await this.teamsMessageService.NotifyAnsweredAcceptedAsync(course, channel, question, answer);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, "Failed to notify answer author.");
            }
        }
    }
}
