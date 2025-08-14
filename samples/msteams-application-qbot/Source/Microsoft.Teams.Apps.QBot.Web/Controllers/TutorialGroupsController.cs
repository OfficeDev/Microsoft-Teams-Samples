// <copyright file="TutorialGroupsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Web.Authorization;

    /// <summary>
    /// <see cref="TutorialGroup"/> APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ErrorResponseFilterAttribute))]
    public class TutorialGroupsController : ControllerBase
    {
        private readonly ITutorialGroupSetup tutorialGroupSetup;
        private readonly ILogger<TutorialGroupsController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly ICourseReader courseReader;
        private readonly ICourseSetup courseSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="TutorialGroupsController"/> class.
        /// </summary>
        /// <param name="courseReader">CourseReader.</param>
        /// <param name="courseSetup">Course setup.</param>
        /// <param name="tutorialGroupSetup">Tutorial group setup.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="authorizationService">AuthZ service.</param>
        public TutorialGroupsController(
            ICourseReader courseReader,
            ICourseSetup courseSetup,
            ITutorialGroupSetup tutorialGroupSetup,
            ILogger<TutorialGroupsController> logger,
            IAuthorizationService authorizationService)
        {
            this.courseReader = courseReader ?? throw new System.ArgumentNullException(nameof(courseReader));
            this.courseSetup = courseSetup ?? throw new System.ArgumentNullException(nameof(courseSetup));
            this.tutorialGroupSetup = tutorialGroupSetup ?? throw new System.ArgumentNullException(nameof(tutorialGroupSetup));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.authorizationService = authorizationService ?? throw new System.ArgumentNullException(nameof(authorizationService));
        }

        #region TutorialGroup Members

        /// <summary>
        /// GET api tutorialgroups/{tutorialgroupId}/members
        ///
        /// Gets all the members in the course's tutorial group.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group's Id.</param>
        /// <returns>List of <see cref="Member"/>.</returns>
        [HttpGet("{tutorialGroupId}/members")]
        public async Task<ActionResult<IEnumerable<Member>>> GetAllTutorialGroupMembersAsync(string tutorialGroupId)
        {
            if (string.IsNullOrEmpty(tutorialGroupId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var tutorialGroup = await this.tutorialGroupSetup.GetTutorialGroupAsync(tutorialGroupId);
            var course = await this.courseReader.GetCourseAsync(tutorialGroup.CourseId).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseMemberPolicy).ConfigureAwait(false);

            var members = await this.tutorialGroupSetup.GetAllTutorialGroupMembersAsync(tutorialGroupId);
            return new ActionResult<IEnumerable<Member>>(members);
        }

        /// <summary>
        /// PUT api tutorialgroups/{id}
        ///
        /// Updates tutorial group.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <param name="tutorialGroup">Tutorial group.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPut("{tutorialGroupId}")]
        public async Task<ActionResult> UpdateTutorialGroupAsync(string tutorialGroupId, [FromBody] TutorialGroup tutorialGroup)
        {
            if (string.IsNullOrEmpty(tutorialGroupId) || tutorialGroup == null || tutorialGroupId != tutorialGroup.Id)
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(tutorialGroup.CourseId).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            await this.tutorialGroupSetup.UpdateTutorialGroupAsync(tutorialGroup);
            return await Task.FromResult(new StatusCodeResult(204));
        }

        /// <summary>
        /// POST api tutorialgroups
        ///
        /// Create a tutorial group.
        /// </summary>
        /// <param name="tutorialGroup">Tutorial group.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<TutorialGroup>> CreateTutorialGroupAsync([FromBody] TutorialGroup tutorialGroup)
        {
            if (tutorialGroup == null)
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var course = await this.courseReader.GetCourseAsync(tutorialGroup.CourseId).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            await this.courseSetup.AddTutorialGroupsAsync(tutorialGroup.CourseId, new List<TutorialGroup>() { tutorialGroup });
            return new OkObjectResult(tutorialGroup);
        }

        /// <summary>
        /// Delete api tutorialgroups/{id}
        ///
        /// Deletes a tutorial group.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpDelete("{tutorialGroupId}")]
        public async Task<ActionResult> DeleteTutorialGroupAsync(string tutorialGroupId)
        {
            if (string.IsNullOrEmpty(tutorialGroupId))
            {
                this.logger.LogWarning("Invalid request data.");
                return new BadRequestResult();
            }

            // Authorize
            var tutorialGroup = await this.tutorialGroupSetup.GetTutorialGroupAsync(tutorialGroupId);
            var course = await this.courseReader.GetCourseAsync(tutorialGroup.CourseId).ConfigureAwait(false);
            await this.authorizationService.AuthorizeAsync(this.User, course, AuthZPolicy.CourseManagerPolicy).ConfigureAwait(false);

            await this.courseSetup.DeleteTutorialGroupsAsync(new List<string>() { tutorialGroupId });
            return await Task.FromResult(this.StatusCode(204));
        }
        #endregion
    }
}
