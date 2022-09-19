namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.FeedbackRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;

    /// <summary>
    /// Controller for feedback details.
    /// </summary>
    [Route("api/feedback")]
    public class FeedbackContoller : BaseController
    {
        /// <summary>
        /// Instance of feedback repository details.
        /// </summary>
        private readonly IFeedbackRepository feedbackRepository;

        /// <summary>
        /// Instance of logger details.
        /// </summary>
        private readonly ILogger<FeedbackContoller> logger;

        /// <summary>
        /// Instance of article repository details.
        /// </summary>
        private readonly IArticleRepository articleRepository;

        /// <summary>
        /// Instance of user repository details.
        /// </summary>
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackContoller"/> class.
        /// </summary>
        /// <param name="feedbackRepository">Entity represent feedback repository details.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        /// <param name="articleRepository">Entity represent article repository details.</param>
        /// <param name="userRepository">Entity represent user repository details.</param>
        public FeedbackContoller(
            IFeedbackRepository feedbackRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient,
            IArticleRepository articleRepository,
            IUserRepository userRepository)
            : base(telemetryClient)
        {
            this.feedbackRepository = feedbackRepository ?? throw new ArgumentNullException(nameof(feedbackRepository));
            this.logger = loggerFactory?.CreateLogger<FeedbackContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Create or update feedback details.
        /// </summary>
        /// <param name="entity">Details of the feedback to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [HttpPost]
        public async Task<IActionResult> AddFeedbackAsync([FromBody] FeedbackEntity entity)
        {
            this.RecordEvent("Create feedback- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (entity == null)
            {
                this.logger.LogError("Feedback Entitydetail is null.");
                this.RecordEvent("Create feedback - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                entity.CreatedOn = DateTime.UtcNow;
                entity.FeedbackId = Guid.NewGuid().ToString();

                var createdContent = await this.feedbackRepository.CreateFeedbackAsync(entity);

                if (createdContent == null)
                {
                    this.RecordEvent("Create feedback- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }
                else
                {
                    this.RecordEvent("Create feedback- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.Created, createdContent);
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
        /// Get all user fedback details.
        /// </summary>
        /// <returns>Returns all user fedback details.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUserFeedbacksAsync()
        {
            this.RecordEvent("GetAllUserFeedbacksAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var feedbackDetails = await this.feedbackRepository.GetAllFeedbacksAsync();

                if (feedbackDetails == null)
                {
                    this.RecordEvent("GetAllUserFeedbacksAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    var feedbackEntity = new List<FeedbackEntity>();
                    foreach (var feedback in feedbackDetails)
                    {
                        if (!string.IsNullOrEmpty(feedback.LearningContentId))
                        {
                            var entity = await this.articleRepository.GetLearningContentAsync(feedback.LearningContentId);

                            if (entity != null)
                            {
                                feedback.LearningContentId = entity.Title;
                            }
                        }

                        feedbackEntity.Add(feedback);
                    }

                    this.RecordEvent("GetAllUserFeedbacksAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(feedbackEntity);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetAllUserFeedbacksAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }
    }
}