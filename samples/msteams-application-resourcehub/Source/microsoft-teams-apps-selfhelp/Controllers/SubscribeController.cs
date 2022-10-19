namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;

    /// <summary>
    /// Controller for subscribe.
    /// </summary>
    [Route("api/subscribe")]
    [ApiController]
    [Authorize]
    public class SubscribeController : BaseController
    {
        /// <summary>
        /// Instance of user repository details.
        /// </summary>
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Instance to send logs to the application insights service.
        /// </summary>
        private readonly ILogger<SubscribeController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeController"/> class.
        /// </summary>
        /// <param name="userRepository">Entity represent user repository details.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        public SubscribeController(
            IUserRepository userRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = loggerFactory?.CreateLogger<SubscribeController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Create or update subscribe path.
        /// </summary>
        /// <param name="UserId">Unique id of user.</param>
        /// <param name="Status">A status.</param>
        /// <returns>Return create or update subscribe path.</returns>
        [HttpPost("CreateOrUpdateAsync")]
        public async Task<IActionResult> CreateOrUpdateAsync(string UserId, string Status)
        {
            this.RecordEvent("Create or update subscribe path- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            try
            {
                var result = await this.userRepository.GetUserByUserIdAsync(UserId);
                if (result.Count() > 0)
                {
                    // Get current subscription status
                    if (result == null)
                    {
                        this.RecordEvent("Unable to update learning path- The HTTP POST call has failed.", RequestType.Failed);
                        return this.StatusCode((int)HttpStatusCode.InternalServerError);
                    }

                    var entityToUpdate = result.First();
                    if (Status == "true")
                    {
                        entityToUpdate.Status = true;
                    }
                    else
                    {
                        entityToUpdate.Status = false;
                    }

                    entityToUpdate.CreatedOn = DateTime.UtcNow;
                    var updatedContent = await this.userRepository.UpdateUserConfigurationsAsync(entityToUpdate);

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

                return this.StatusCode((int)HttpStatusCode.Created, true);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create Subscribe- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while creating learning.");
                throw;
            }
        }

        /// <summary>
        /// Get logged in user's subscription status by user id.
        /// </summary>
        /// <returns>Returns the user by user id.</returns>
        [HttpGet("GetSubscribeByUserIdAsync")]
        public async Task<IActionResult> GetUserByUserIdAsync()
        {
            this.RecordEvent("GetUserByUserIdAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var subscribeDetails = await this.userRepository.GetUserByUserIdAsync(this.UserAadId);

                if (subscribeDetails == null)
                {
                    this.RecordEvent("GetUserByUserIdAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetUserByUserIdAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(subscribeDetails);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetUserByUserIdAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }
    }
}