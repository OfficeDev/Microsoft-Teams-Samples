namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserReactionRepository;

    /// <summary>
    /// Controller for user reaction details.
    /// </summary>
    [Route("api/userreaction")]
    public class UserReactionContoller : BaseController
    {
        /// <summary>
        /// Instance of user reaction repository details.
        /// </summary>
        private readonly IUserReactionRepository userReactionRepository;

        /// <summary>
        /// Instance of logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<UserReactionContoller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReactionContoller"/> class.
        /// </summary>
        /// <param name="userReactionRepository">Entity repreesent user reaction repository details.</param>
        /// <param name="loggerFactory">Entity repreesent logger factory details.</param>
        /// <param name="telemetryClient">Entity repreesent application insights telemetry client.</param>
        public UserReactionContoller(
            IUserReactionRepository userReactionRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.userReactionRepository = userReactionRepository ?? throw new ArgumentNullException(nameof(userReactionRepository));
            this.logger = loggerFactory?.CreateLogger<UserReactionContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Add or update user reaction.
        /// </summary>
        /// <param name="entity">Details of the feedback to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateUserReactionAsync([FromBody] UserReactionEntity entity)
        {
            this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (entity == null)
            {
                this.logger.LogError("AddOrUpdateUserReactionAsync Entitydetail is null.");
                this.RecordEvent("AddOrUpdateUserReactionAsync - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                var learningDetails = await this.userReactionRepository.GetUserReactionByLearningContentIdAsync(entity.UserAadId, entity.LearningContentId);
                if (learningDetails == null || learningDetails.FirstOrDefault() == null)
                {
                    // add user reaction
                    entity.ReactionId = Guid.NewGuid().ToString();
                    entity.LastModifiedOn = DateTime.UtcNow;
                    var createdContent = await this.userReactionRepository.AddUserReactionAsync(entity);

                    if (createdContent == null)
                    {
                        this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call has failed.", RequestType.Failed);
                        return this.StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call has succeeded.", RequestType.Succeeded);
                        return this.StatusCode((int)HttpStatusCode.Created, createdContent);
                    }
                }
                else
                {
                    //update user reaction
                    if (learningDetails != null)
                    {
                        var entityToUpdate = learningDetails.First();
                        entityToUpdate.ReactionState = entity.ReactionState;
                        entityToUpdate.LastModifiedOn = DateTime.UtcNow;

                        var updatedContent = await this.userReactionRepository.UpdateUserReactionAsync(entityToUpdate);

                        if (updatedContent == null)
                        {
                            this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call has failed.", RequestType.Failed);
                            return this.StatusCode((int)HttpStatusCode.InternalServerError);
                        }
                        else
                        {
                            this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call has succeeded.", RequestType.Succeeded);
                            return this.StatusCode((int)HttpStatusCode.Created, updatedContent);
                        }
                    }
                    else
                    {
                        this.RecordEvent("AddOrUpdateUserReactionAsync- The HTTP POST call has failed.", RequestType.Failed);
                        return this.StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create feedback- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating learning.");
                throw;
            }
        }

        /// <summary>
        /// Get user reaction by user and learning content details.
        /// </summary>
        /// <param name="aadid">Aad id of user.</param>
        /// <param name="learningId">Id of learning content to be fetched.</param>
        /// <returns>Returns the user reaction by user and learning content details.</returns>
        [HttpGet("{learningId}")]
        public async Task<IActionResult> GetUserReactionByUsersAndLearningContentAsync([FromQuery] string aadid, string learningId)
        {
            this.RecordEvent("GetUserReactionByUsersAndLearningContentAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var learningDetails = await this.userReactionRepository.GetUserReactionByLearningContentIdAsync(aadid, learningId);

                if (learningDetails == null)
                {
                    this.RecordEvent("GetUserReactionByUsersAndLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetUserReactionByUsersAndLearningContentAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(learningDetails.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetUserReactionByUsersAndLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }
    }
}