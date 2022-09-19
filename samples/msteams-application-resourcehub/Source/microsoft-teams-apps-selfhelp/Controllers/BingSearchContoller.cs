namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.BingSearch;
    using Microsoft.Teams.Selfhelp.Models.Configuration;

    /// <summary>
    /// Controller for bing search result details.
    /// </summary>
    [Route("api/bingsearch")]
    public class BingSearchContoller : BaseController
    {
        /// <summary>
        /// Instance of bing search result details.
        /// </summary>
        private readonly IBingSearch bingSearch;

        /// <summary>
        /// Instance of logger details.
        /// </summary>
        private readonly ILogger<BingSearchContoller> logger;

        /// <summary>
        /// Instance of article repository details.
        /// </summary>
        private readonly IArticleRepository learningRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingSearchContoller"/> class.
        /// </summary>
        /// <param name="bingSearch">Entity represent bing search result.</param>
        /// <param name="learningRepository">Entity represent learning repository.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity repreesent application insights telemetry client.</param>
        public BingSearchContoller(
            IBingSearch bingSearch,
            IArticleRepository learningRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.learningRepository = learningRepository ?? throw new ArgumentNullException(nameof(learningRepository));
            this.bingSearch = bingSearch ?? throw new ArgumentNullException(nameof(bingSearch));
            this.logger = loggerFactory?.CreateLogger<BingSearchContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Get bing search result.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Returns learning details.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetAsync([FromBody] SearchQuery query)
        {
            this.RecordEvent("GetAsync- The HTTP GET call has been initiated.", RequestType.Initiated);

            try
            {
                var result = new BingSearchResult();
                try
                {
                    result = await this.bingSearch.GetBingSearchResultsAsync(query.Query);
                }
                catch (Exception bingException)
                {
                    this.RecordEvent("GetBingSearchResultsAsync- The HTTP GET call has failed.", RequestType.Failed);
                    this.logger.LogError(bingException, "Error occurred while bing search content.");
                }

                var internalSearch = await this.learningRepository.GetAllLearningContentesAsync();
                var filter = internalSearch.Where(a => a.PrimaryTag.Contains(query.Query, StringComparison.OrdinalIgnoreCase)
                || a.SecondaryTag.Contains(query.Query, StringComparison.OrdinalIgnoreCase)
                || a.Title.Contains(query.Query, StringComparison.OrdinalIgnoreCase)
                || a.Description.Contains(query.Query, StringComparison.OrdinalIgnoreCase));

                if (filter != null && filter.Count() > 0)
                {
                    result.articles = filter;
                }

                if (internalSearch == null)
                {
                    this.RecordEvent("GetBingSearchResultsAsync- The HTTP GET call has failed.", RequestType.Failed);
                    return this.NotFound("Search result not found.");
                }
                else
                {
                    this.RecordEvent("GetBingSearchResultsAsync- The HTTP GET call has succeeded.", RequestType.Succeeded);
                    return this.Ok(result);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetBingSearchResultsAsync- The HTTP GET call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while bing search content.");
                throw;
            }
        }
    }
}