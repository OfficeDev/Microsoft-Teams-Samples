// <copyright file="UsersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Domain.Questions;
    using Microsoft.Teams.Apps.QBot.Domain.Users;
    using Microsoft.Teams.Apps.QBot.Web.Authorization;

    /// <summary>
    /// <see cref="User"/> APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ErrorResponseFilterAttribute))]
    public sealed class UsersController : ControllerBase
    {
        private readonly ICourseReader courseReader;
        private readonly IQuestionReader questionReader;
        private readonly IUserReaderService userReaderService;
        private readonly ILogger<UsersController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly AuthorizationSettings authorizationSettings;
        private readonly IKnowledgeBaseReader kbReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="courseReader">Course Reader.</param>
        /// <param name="questionReader">Question reader.</param>
        /// <param name="userProfileService">User profile service.</param>
        /// <param name="authorizationService">AuthZ service.</param>
        /// <param name="authorizationSettings">Auth settings.</param>
        /// <param name="kbReader">The knowledge base reader.</param>
        /// <param name="logger">Logger.</param>
        public UsersController(
            ICourseReader courseReader,
            IQuestionReader questionReader,
            IUserReaderService userProfileService,
            IAuthorizationService authorizationService,
            AuthorizationSettings authorizationSettings,
            IKnowledgeBaseReader kbReader,
            ILogger<UsersController> logger)
        {
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.questionReader = questionReader ?? throw new ArgumentNullException(nameof(questionReader));
            this.userReaderService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.authorizationSettings = authorizationSettings ?? throw new ArgumentNullException(nameof(authorizationSettings));
            this.kbReader = kbReader ?? throw new ArgumentNullException(nameof(kbReader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET api/users/{userid}/configuration.
        ///
        /// Gets the configuration for the specified user.
        /// </summary>
        /// <param name="userId">The user's AAD Object Id.</param>
        /// <returns><see cref="UserConfiguration"/>.</returns>
        /// <response code="200"> Returns the configuration for the user.</response>
        /// <response code="400"> If the user Id is null or empty.</response>
        [HttpGet("{userId}/configuration")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<ActionResult<UserConfiguration>> GetConfigurationAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            var user = await this.userReaderService.GetUserAsync(userId, false);
            var userUpn = user.Upn;
            return await Task.FromResult(new UserConfiguration
            {
                Id = userId,
                IsAdministrator = this.authorizationSettings.AdminUpnList.Contains(userUpn),
            });
        }

        /// <summary>
        /// GET api/users/{userid}
        ///
        /// Gets user profile.
        /// </summary>
        /// <param name="userId">user's AAD id.</param>
        /// <returns>User profile.</returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            var userProfile = await this.userReaderService.GetUserAsync(userId, true/*fetchProfilePic*/);
            return userProfile;
        }

        /// <summary>
        /// GET api/users/{userid}/courses.
        ///
        /// Gets all the courses the user is part of.
        /// </summary>
        /// <param name="userId">User's Team Id.</param>
        /// <returns>List of <see cref="Course"/>.</returns>
        [HttpGet("{userId}/courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCoursesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            // Authorize
            await this.authorizationService.AuthorizeAsync(this.User, userId, AuthZPolicy.UserResourcePolicy);

            IEnumerable<Course> courses = new List<Course>();
            try
            {
                courses = await this.courseReader.GetAllCoursesForUserAsync(userId).ConfigureAwait(false);
                return new OkObjectResult(courses);
            }
            catch (QBotException exception)
            {
                // For personal app scenario when user's profile doesn't exist in the database.
                if (exception.Code == ErrorCode.UserNotFound && this.User.GetUserId() == userId)
                {
                    return new OkObjectResult(courses);
                }

                throw;
            }
        }

        /// <summary>
        /// GET api/users/{userid}/questions.
        ///
        /// Gets all the questions asked by the user.
        /// </summary>
        /// <param name="userId">User's team Id.</param>
        /// <returns>List of <see cref="Question"/>.</returns>
        [HttpGet("{userId}/questions")]
        public async Task<ActionResult<IEnumerable<Question>>> GetAllQuestionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            // Authorize
            await this.authorizationService.AuthorizeAsync(this.User, userId, AuthZPolicy.UserResourcePolicy);

            var questions = await this.questionReader.GetQuestionsAsync(userId);
            return new ActionResult<IEnumerable<Question>>(questions);
        }

        /// <summary>
        /// GET api/{userId}/knowledgebases
        /// </summary>
        /// <param name="userId">User's id.</param>
        /// <returns>List of <see cref="KnowledgeBase"/>.</returns>
        [HttpGet("{userId}/knowledgebases")]
        public async Task<ActionResult<IEnumerable<KnowledgeBase>>> GetAllKnowledgeBasesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("Invalid data.");
                return new BadRequestResult();
            }

            // Authorize
            await this.authorizationService.AuthorizeAsync(this.User, userId, AuthZPolicy.UserResourcePolicy);

            // return KBs owned by the user.
            var kbs = await this.kbReader.GetAllKnowledgeBasesAsync();
            kbs = kbs.Where(kb => kb.OwnerUserId == userId);
            return new OkObjectResult(kbs);
        }
    }
}
