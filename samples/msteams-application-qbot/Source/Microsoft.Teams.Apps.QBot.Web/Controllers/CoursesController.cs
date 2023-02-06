// <copyright file="CoursesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Domain.Questions;
    using Microsoft.Teams.Apps.QBot.Web.Authorization;

    /// <summary>
    /// <see cref="Course"/> APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ErrorResponseFilterAttribute))]
    public class CoursesController : ControllerBase
    {
        private readonly ILogger<CoursesController> logger;
        private readonly ICourseReader courseReader;
        private readonly ICourseSetup courseSetup;
        private readonly IQuestionReader questionReader;
        private readonly IQBotService qBotService;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesController"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="courseReader">CourseReader.</param>
        /// <param name="courseSetup">Course setup.</param>
        /// <param name="questionReader">Question reader.</param>
        /// <param name="qBotService">QBot Service.</param>
        /// <param name="authorizationService">AuthZ service.</param>
        public CoursesController(
            ILogger<CoursesController> logger,
            ICourseReader courseReader,
            ICourseSetup courseSetup,
            IQuestionReader questionReader,
            IQBotService qBotService,
            IAuthorizationService authorizationService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.courseSetup = courseSetup ?? throw new ArgumentNullException(nameof(courseSetup));
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.questionReader = questionReader ?? throw new ArgumentNullException(nameof(questionReader));
            this.qBotService = qBotService ?? throw new ArgumentNullException(nameof(qBotService));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        #region Courses

        /// <summary>
        /// GET api/courses/id
        ///
        /// Gets the course for the id passed.
        /// </summary>
        /// <param name="id">Course id.</param>
        /// <returns><see cref="Course"/>.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourseAsync(string id)
        {
            this.logger.LogInformation($"Getting course: {id}");

            // Authorize
            var course = await this.courseReader.GetCourseAsync(id).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            return course;
        }

        /// <summary>
        /// GET api/courses
        ///
        /// Gets all the courses.
        /// </summary>
        /// <returns>List of all the courses.</returns>
        [HttpGet]
        [Authorize(Policy = AuthZPolicy.AdminPolicy)]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCoursesAsync()
        {
            this.logger.LogInformation("Getting all courses.");
            var courses = await this.courseReader.GetAllCoursesAsync();
            return new OkObjectResult(courses);
        }

        /// <summary>
        /// PUT api/courses/id.
        ///
        /// Update a course.
        /// </summary>
        /// <param name="id">courseId</param>
        /// <param name="updatedCourse">updated object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> PutCourseAsync(string id, [FromBody] Course updatedCourse)
        {
            this.logger.LogInformation($"Updating course: {id}");

            if (string.IsNullOrEmpty(id) || updatedCourse == null)
            {
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(id);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            await this.courseSetup.UpdateCourseAsync(updatedCourse);
            return await Task.FromResult(new StatusCodeResult(204));
        }

        #endregion

        #region Course Members

        /// <summary>
        /// GET api/courses/{courseId}/members
        ///
        /// Gets all the members in the course.
        /// </summary>
        /// <param name="courseId">Course's id.</param>
        /// <returns><see cref="Member"/>.</returns>
        [HttpGet("{courseId}/members")]
        public async Task<ActionResult<IEnumerable<Member>>> GetAllMembersAsync(string courseId)
        {
            this.logger.LogInformation($"Getting all members of a course : {courseId}");

            if (string.IsNullOrEmpty(courseId))
            {
                this.logger.LogWarning($"Invalid course id : {courseId}");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var members = await this.courseReader.GetAllMembersAsync(courseId).ConfigureAwait(false);
            return new ActionResult<IEnumerable<Member>>(members);
        }

        /// <summary>
        /// GET api/courses/{courseId}/members/memberId
        ///
        /// Gets a member in the course.
        /// </summary>
        /// <param name="courseId">Course's id.</param>
        /// <param name="memberId">Member's id.</param>
        /// <returns><see cref="Member"/>.</returns>
        [HttpGet("{courseId}/members/{memberId}")]
        public async Task<ActionResult<Member>> GetMemberAsync(string courseId, string memberId)
        {
            this.logger.LogInformation($"Getting all members of a course : {courseId}");

            if (string.IsNullOrEmpty(courseId))
            {
                this.logger.LogWarning($"Invalid course id : {courseId}");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var member = await this.courseReader.GetMemberAsync(courseId, memberId).ConfigureAwait(false);
            return new ActionResult<Member>(member);
        }

        /// <summary>
        /// PUT api/courses/{courseId}/members/memberId
        ///
        /// Updates course member's:
        /// - Tutorial group membership.
        /// - Role
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="memberId">Member's id.</param>
        /// <param name="member">Updated member object.</param>
        /// <returns>Async task.</returns>
        [HttpPut("{courseId}/members/{memberId}")]
        public async Task<ActionResult> UpdateCourseMemberAsync(string courseId, string memberId, [FromBody] Member member)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (string.IsNullOrEmpty(memberId))
            {
                throw new ArgumentException($"'{nameof(memberId)}' cannot be null or empty.", nameof(memberId));
            }

            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            await this.courseSetup.UpdateMemberAsync(courseId, member);

            return await Task.FromResult(new StatusCodeResult(204));
        }

        #endregion

        #region Channels

        /// <summary>
        /// GET api/courses/{courseId}/channels
        ///
        /// Gets all the channels in the course.
        /// </summary>
        /// <param name="courseId">Course's id.</param>
        /// <returns><see cref="Channel"/>.</returns>
        [HttpGet("{courseId}/channels")]
        public async Task<ActionResult<IEnumerable<Channel>>> GetAllChannelsAsync(string courseId)
        {
            this.logger.LogInformation($"Getting all channels in a course : {courseId}");

            if (string.IsNullOrEmpty(courseId))
            {
                this.logger.LogWarning($"Invalid course id : {courseId}");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var channels = await this.courseReader.GetAllChannelsAsync(courseId).ConfigureAwait(false);
            return new OkObjectResult(channels);
        }

        #endregion

        #region Channel Questions

        /// <summary>
        /// GET api/courses/{courseid}/channels/{channelid}/questions/{questionId}
        ///
        /// Gets the question.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <returns><see cref="Question"/>.</returns>
        [HttpGet("{courseId}/channels/{channelId}/questions/{questionId}")]
        public async Task<ActionResult<Question>> GetChannelQuestionAsync(string courseId, string channelId, string questionId)
        {
            this.logger.LogInformation($"Getting question : {questionId} posted in channel : {channelId}, course {courseId}.");

            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(questionId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var question = await this.questionReader.GetQuestionAsync(courseId, channelId, questionId);
            return new ActionResult<Question>(question);
        }

        /// <summary>
        /// GET api/courses/{courseid}/channels/{channelid}/questions
        ///
        /// Gets all the questions asked in the course's channel.
        /// </summary>
        /// <param name="courseId">Course's id.</param>
        /// <param name="channelId">Channel's id.</param>
        /// <returns><see cref="Question"/>.</returns>
        [HttpGet("{courseId}/channels/{channelId}/questions")]
        public async Task<ActionResult<IEnumerable<Question>>> GetAllChannelQuestionsAsync(string courseId, string channelId)
        {
            this.logger.LogInformation($"Getting all questions posted in channel : {channelId}, course {courseId}.");
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(channelId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var questions = await this.questionReader.GetQuestionsAsync(courseId, channelId);
            return new ActionResult<IEnumerable<Question>>(questions);
        }

        /// <summary>
        /// GET api/courses/{courseid}/channels/{channelid}/questions/{questionId}/responses
        ///
        /// Gets all the responses to the question.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <returns><see cref="QuestionResponse"/>.</returns>
        [HttpGet("{courseId}/channels/{channelId}/questions/{questionId}/responses")]
        public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestionResponsesAsync(string courseId, string channelId, string questionId)
        {
            this.logger.LogInformation($"Getting all respones to question : {questionId} posted in channel : {channelId}, course {courseId}.");
            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(questionId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var responses = await this.questionReader.GetQuestionResponsesAsync(courseId, channelId, questionId);
            return new ActionResult<IEnumerable<QuestionResponse>>(responses);
        }

        /// <summary>
        /// GET api/courses/{courseid}/channels/{channelid}/questions/{questionId}/responses/{responseId}
        ///
        /// Gets the response to the question.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <param name="responseId">Response Id.</param>
        /// <returns><see cref="QuestionResponse"/>.</returns>
        [HttpGet("{courseId}/channels/{channelId}/questions/{questionId}/responses/{responseId}")]
        public async Task<ActionResult<QuestionResponse>> GetQuestionResponseAsync(string courseId, string channelId, string questionId, string responseId)
        {
            this.logger.LogInformation($"Getting a response: {responseId} to a question : {questionId}");

            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(questionId) || string.IsNullOrEmpty(responseId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy);

            var response = await this.questionReader.GetQuestionResponseAsync(courseId, channelId, questionId, responseId);
            if (response == null)
            {
                this.logger.LogWarning($"Response: {responseId} not found!");
                return this.NotFound();
            }

            return new ActionResult<QuestionResponse>(response);
        }

        /// <summary>
        /// POST api/courses/{courseid}/channels/{channelid}/questions/{questionId}/answer
        ///
        /// Post answer to a question.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <param name="answer">Answer object.</param>
        /// <returns><see cref="Answer"/>.</returns>
        [HttpPost("{courseId}/channels/{channelId}/questions/{questionId}/answer")]
        public async Task<ActionResult<Answer>> PostAnswerAsync(string courseId, string channelId, string questionId, [FromBody] Answer answer)
        {
            this.logger.LogInformation($"Posting an answer to question: {questionId} in channel : {channelId}, course: {courseId}.");

            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(questionId) || answer == null)
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // AnswerId / Message Id will be set by the application.
            if (!string.IsNullOrEmpty(answer.Id) || (!string.IsNullOrEmpty(answer.MessageId)))
            {
                this.logger.LogWarning("Invalid answer object.");
                return new BadRequestResult();
            }

            // Authorize
            var question = await this.qBotService.GetQuestionAsync(courseId, channelId, questionId);
            await this.authorizationService.AuthorizeAsync(this.User, question, AuthZPolicy.PostAnswerPolicy);

            var response = await this.qBotService.PostAnswerAsync(answer);
            return new ActionResult<Answer>(response);
        }

        #endregion

        #region TutorialGroups

        /// <summary>
        /// GET api/courses/{courseid}/tutorialgroups.
        ///
        /// Gets all the tutorial groups in the course.
        /// </summary>
        /// <param name="courseId">Course's Id.</param>
        /// <returns>List of <see cref="TutorialGroup"/>.</returns>
        [HttpGet("{courseId}/tutorialgroups")]
        public async Task<ActionResult<IEnumerable<TutorialGroup>>> GetAllTutorialGroupsAsync(string courseId)
        {
            this.logger.LogInformation($"Getting all tutorial groups for course: {courseId}");
            if (string.IsNullOrEmpty(courseId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(courseId);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var tutorialGroups = await this.courseReader.GetAllTutorialGroupsAsync(courseId);
            return new OkObjectResult(tutorialGroups);
        }

        #endregion
    }
}
