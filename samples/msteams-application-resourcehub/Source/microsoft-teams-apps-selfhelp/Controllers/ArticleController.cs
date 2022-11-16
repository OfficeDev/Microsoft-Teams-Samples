namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using System.Net;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;

    /// <summary>
    /// Controller for the article content.
    /// </summary>
    [Route("api/learning")]
    public class ArticleController : BaseController
    {
        /// <summary>
        /// Instance of article repository details.
        /// </summary>
        private readonly IArticleRepository learningRepository;

        /// <summary>
        /// Instance of user repository details.
        /// </summary>
        private readonly IUserRepository userDataRepository;

        /// <summary>
        /// Instance of article coontroller.
        /// </summary>
        private readonly ILogger<ArticleController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleController"/> class.
        /// </summary>
        /// <param name="learningRepository">Entity represent learning repository.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="userDataRepository">Entity represent user repository.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        public ArticleController(
            IArticleRepository learningRepository,
            ILoggerFactory loggerFactory,
            IUserRepository userDataRepository,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (telemetryClient is null)
            {
                throw new ArgumentNullException(nameof(telemetryClient));
            }

            this.learningRepository = learningRepository ?? throw new ArgumentNullException(nameof(learningRepository));
            this.logger = loggerFactory?.CreateLogger<ArticleController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.userDataRepository = userDataRepository ?? throw new ArgumentNullException(nameof(userDataRepository));
        }

        /// <summary>
        /// Get learning content by Id.
        /// </summary>
        /// <param name="learningId">learning id of learning content to be fetched.</param>
        /// <returns>Returns learning details.</returns>
        [HttpGet("{learningId}")]
        public async Task<IActionResult> GetLearningContentAsync(string learningId)
        {
            this.RecordEvent("Get learning content- The HTTP GET call has been initiated.", RequestType.Initiated);
            if (string.IsNullOrEmpty(learningId))
            {
                this.RecordEvent("Get Learning content - The HTTP GET call has failed.", RequestType.Failed);
                return this.BadRequest("Invalid learning Id.");
            }

            try
            {
                var learningDetails = await this.learningRepository.GetLearningContentAsync(learningId);

                if (learningDetails == null)
                {
                    this.RecordEvent("GetLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetLearningContentAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(learningDetails);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }

        /// <summary>
        /// Get learning content by type 0 = GettingStarted, 1= Scenarios, 2 = Trending Now.
        /// </summary>
        /// <param name="selectiontype">learning content type to be fetched.</param>
        /// <returns>Returns learning details.</returns>
        [Authorize]
        [HttpGet("selectiontype/{selectiontype}")]
        public async Task<IActionResult> GetLearningContentByTypeAsync(string selectiontype)
        {
            this.RecordEvent("Get learning content- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                SelectionType type = (SelectionType)Enum.Parse(typeof(SelectionType), selectiontype);
                var learningDetails = await this.learningRepository.GetLearningContentesByTypeAsync(type);

                if (learningDetails == null)
                {
                    this.RecordEvent("GetLearningContentByTypeAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetLearningContentByTypeAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(learningDetails);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetLearningContentByTypeAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }

        /// <summary>
        /// Get all learning contentes.
        /// </summary>
        /// <returns>Returns learning details.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLearningContentesAsync()
        {
            this.RecordEvent("Get learning content- The HTTP GET call has been initiated.", RequestType.Initiated);
            try
            {
                var learningDetails = await this.learningRepository.GetAllLearningContentesAsync();

                if (learningDetails == null)
                {
                    this.RecordEvent("GetAllLearningContentesAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    this.RecordEvent("GetAllLearningContentesAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(learningDetails.OrderByDescending(a => a.CreatedOn));
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetAllLearningContentesAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }

        /// <summary>
        /// Create a new learning content.
        /// </summary>
        /// <param name="learningEntity">Details of the learning content to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ArticleEntity learningEntity)
        {
            this.RecordEvent("Create learning- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (learningEntity == null)
            {
                this.logger.LogError("learning detail is null.");
                this.RecordEvent("Create learning - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                learningEntity.LearningId = Guid.NewGuid().ToString();
                learningEntity.CreatedOn = DateTime.UtcNow;

                var createdContent = await this.learningRepository.CreateLearningContentAsync(learningEntity);

                if (!createdContent)
                {
                    this.RecordEvent("Create learning- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }
                else
                {
                    this.RecordEvent("Create learning- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.Created, createdContent);
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
        /// Update existing learning content.
        /// </summary>
        /// <param name="learningId">Unique id of learning id.</param>
        /// <param name="learningEntity">Details of the learning content to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [Authorize]
        [HttpPatch("{learningId}")]
        public async Task<IActionResult> UpdateAsync(string learningId, [FromBody] ArticleEntity learningEntity)
        {
            this.RecordEvent("Create learning- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (learningEntity == null || string.IsNullOrEmpty(learningId))
            {
                this.logger.LogError("learning detail is null.");
                this.RecordEvent("Create learning - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                var entity = await this.learningRepository.GetLearningContentAsync(learningId);

                if (entity == null)
                {
                    this.logger.LogError($"Not found learning entity for id {learningEntity.LearningId}");
                    this.RecordEvent("Update learning - The HTTP PATCH call to update learning has failed.", RequestType.Failed);
                    return this.NotFound();
                }

                entity.Title = learningEntity.Title;
                entity.Source = learningEntity.Source;
                entity.Description = learningEntity.Description;
                entity.Length = learningEntity.Length;
                entity.Itemlink = learningEntity.Itemlink;
                entity.ItemType = learningEntity.ItemType;
                entity.KnowmoreLink = learningEntity.KnowmoreLink;
                entity.SecondaryTag = learningEntity.SecondaryTag;
                entity.PrimaryTag = learningEntity.PrimaryTag;
                entity.SectionType = learningEntity.SectionType;
                entity.TileImageLink = learningEntity.TileImageLink;
                entity.CreatedBy = learningEntity.CreatedBy;

                var createdContent = await this.learningRepository.UpdateLearningContentAsync(entity);

                if (!createdContent)
                {
                    this.RecordEvent("Create learning- The HTTP POST call has failed.", RequestType.Failed);
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }
                else
                {
                    this.RecordEvent("Create learning- The HTTP POST call has succeeded.", RequestType.Succeeded);
                    return this.StatusCode((int)HttpStatusCode.Created, createdContent);
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
        /// Delete learning content by Id.
        /// </summary>
        /// <param name="learningId">learningId id of learning content to be fetched.</param>
        /// <returns>Returns learning details.</returns>
        [Authorize]
        [HttpDelete("{learningId}")]
        public async Task<IActionResult> DeleteLearningContentAsync(string learningId)
        {
            this.RecordEvent("Get learning content- The HTTP GET call has been initiated.", RequestType.Initiated);
            if (string.IsNullOrEmpty(learningId))
            {
                this.RecordEvent("Get Learning content - The HTTP GET call has failed.", RequestType.Failed);
                return this.BadRequest("Invalid learning Id.");
            }

            try
            {
                var learningDetails = await this.learningRepository.GetLearningContentAsync(learningId);

                if (learningDetails == null)
                {
                    this.RecordEvent("GetLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Learning content not found.");
                }
                else
                {
                    var item = await this.learningRepository.DeleteLearningContentAsync(learningDetails);
                    this.RecordEvent("GetLearningContentAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(item);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetLearningContentAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching learning content.");
                throw;
            }
        }
    }
}