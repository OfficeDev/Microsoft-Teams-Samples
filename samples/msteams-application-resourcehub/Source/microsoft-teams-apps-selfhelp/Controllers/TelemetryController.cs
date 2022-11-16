namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LogEventRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserReactionRepository;
    using Microsoft.Teams.Selfhelp.Authentication.Model;

    /// <summary>
    /// Controller for telemetry details.
    /// </summary>
    [Route("api/telemetry")]
    public class TelemetryController : BaseController
    {
        private const string share_to_user = "Shared to user";
        private const string share_to_channel = "Shared to Channel";

        /// <summary>
        /// Instance of the logger details.
        /// </summary>
        private readonly ILogger<TelemetryController> logger;

        /// <summary>
        /// Instance of article repository details.
        /// </summary>
        private readonly IArticleRepository articleRepository;

        /// <summary>
        /// Instance of user repository reaction repository details.
        /// </summary>
        private readonly IUserReactionRepository userReactionRepository;

        /// <summary>
        /// Instance of log event repository details.
        /// </summary>
        private readonly ILogEventRepository logEventRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryController"/> class.
        /// </summary>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="articleRepository">Entity represent article repository details.</param>
        /// <param name="userReactionRepository">Entity represent user reaction repository details.</param>
        /// <param name="logEventRepository">Entity represent log event repository details.</param>
        public TelemetryController(
            TelemetryClient telemetryClient,
            ILoggerFactory loggerFactory,
            IArticleRepository articleRepository,
            IUserReactionRepository userReactionRepository,
            ILogEventRepository logEventRepository)
            : base(telemetryClient)
        {
            this.logger = loggerFactory?.CreateLogger<TelemetryController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
            this.userReactionRepository = userReactionRepository ?? throw new ArgumentNullException(nameof(userReactionRepository));
            this.logEventRepository = logEventRepository ?? throw new ArgumentNullException(nameof(logEventRepository));
        }

        /// <summary>
        /// Get all user event log telemetry.
        /// </summary>
        /// <returns>Returns telemetry details.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTelemetryAsync()
        {
            this.RecordEvent("GetAllTelemetryAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var articlesData = new List<TelemetryData>();
                var articleDetails = await this.articleRepository.GetAllLearningContentesAsync();
                foreach (var article in articleDetails)
                {
                    var userRactionCount = await this.userReactionRepository.GetUserReactionByLearningIdAsync(article.LearningId);
                    var eventLogCount = await this.logEventRepository.GetLogEventLearningIdAsync(article.LearningId);
                    TelemetryData telemetryData = new TelemetryData();
                    telemetryData.ArticleTitle = article.Title;
                    telemetryData.ItemType = article.ItemType == 0 ? "Video" : "Articles";
                    telemetryData.TotalLikeCount = userRactionCount.Count(s => s.ReactionState == ReactionState.Like);
                    telemetryData.TotalDislikeCount = userRactionCount.Count(s => s.ReactionState == ReactionState.Dislike);
                    telemetryData.TotalViewCount = eventLogCount.Count(s => s.EventType == "Video") + eventLogCount.Count(s => s.EventType == "Article");
                    telemetryData.ShareArticleToUser = eventLogCount.Count(s => s.EventType == share_to_user);
                    telemetryData.ShareArticleToChannel = eventLogCount.Count(s => s.EventType == share_to_channel);
                    articlesData.Add(telemetryData);
                }

                return this.Ok(articlesData);
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetAllTelemetryAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching telemetry content.");
                throw;
            }
        }
    }
}