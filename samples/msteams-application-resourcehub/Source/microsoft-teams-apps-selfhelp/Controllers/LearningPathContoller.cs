namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LearningPath;

    /// <summary>
    /// Controller for learning path details.
    /// </summary>
    [Route("api/learningpath")]
    public class LearningPathContoller : BaseController
    {
        /// <summary>
        /// Instance of learning repository details.
        /// </summary>
        private readonly ILearningPathRepository learningPathRepository;

        /// <summary>
        /// Instance of logger details.
        /// </summary>
        private readonly ILogger<LearningPathContoller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningPathContoller"/> class.
        /// </summary>
        /// <param name="learningPathRepository">Entity represent learning path repository details.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        public LearningPathContoller(
            ILearningPathRepository learningPathRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.learningPathRepository = learningPathRepository ?? throw new ArgumentNullException(nameof(learningPathRepository));
            this.logger = loggerFactory?.CreateLogger<LearningPathContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Create or update learning path.
        /// </summary>
        /// <param name="entity">Details of the learning path content to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [Authorize]
        [HttpPost("CreateOrUpdateAsync")]
        public async Task<IActionResult> CreateOrUpdateAsync([FromBody] LearningPathEntity entity)
        {
            this.RecordEvent("Create or update learning path- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (entity == null)
            {
                this.logger.LogError("learning path detail is null.");
                this.RecordEvent("Create learning - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                var result = await this.learningPathRepository.GetLearningPathByUserIdAndLearningIdAsync(entity.UserAadId, entity.LearningContentId);
                if (result.Count() > 0)
                {
                    var entityToUpdate = result.First();
                    entityToUpdate.LastModifiedOn = DateTime.UtcNow;
                    entityToUpdate.CompleteState = entity.CompleteState;
                    var updatedContent = await this.learningPathRepository.UpdateLearningPathContentAsync(entityToUpdate);

                    if (updatedContent == null)
                    {
                        this.RecordEvent("Create learning path- The HTTP POST call has failed.", RequestType.Failed);
                        return this.StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        this.RecordEvent("Create learning path- The HTTP POST call has succeeded.", RequestType.Succeeded);
                        return this.StatusCode((int)HttpStatusCode.Created, updatedContent);
                    }
                }
                else
                {
                    entity.LastModifiedOn = DateTime.UtcNow;
                    var createdContent = await this.learningPathRepository.CreateLearningPathContentAsync(entity);

                    if (createdContent == null)
                    {
                        this.RecordEvent("Create learning path- The HTTP POST call has failed.", RequestType.Failed);
                        return this.StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        this.RecordEvent("Create learning path- The HTTP POST call has succeeded.", RequestType.Succeeded);
                        return this.StatusCode((int)HttpStatusCode.Created, createdContent);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create learning- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating learning.");
                throw;
            }
        }

        /// <summary>
        /// Get learning content path by user id.
        /// </summary>
        /// <param name="userid">Unique user id.</param>
        /// <returns>Returns learning details.</returns>
        [Authorize]
        [HttpGet("mylearningpath")]
        public async Task<IActionResult> GetLearningPathByUsersAsync([FromQuery] string userid)
        {
            this.RecordEvent("GetLearningPathByUsersAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var learningDetails = await this.learningPathRepository.GetLearningPathByUserIdAsync(userid);

                if (learningDetails == null)
                {
                    this.RecordEvent("GetLearningPathByUsersAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetLearningPathByUsersAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(learningDetails);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetLearningPathByUsersAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }
    }
}