namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;

    /// <summary>
    /// Url Provider implementation.
    /// </summary>
    public class UrlProvider : IUrlProvider
    {
        /// <summary>
        /// Select an answer page url format.
        /// {0} - base url.
        /// {1} - course Id.
        /// {2} - channel Id.
        /// {3} - question Id.
        /// </summary>
        private const string SelectAnswerUrlFormat = "{0}/taskmodule/course/{1}/channel/{2}/question/{3}?userObjectId={{userObjectId}}&theme={{theme}}&locale={{locale}}&subEntityId={{subEntityId}}";

        /// <summary>
        /// Select an answer page url format.
        /// {0} - base url.
        /// {1} - course Id.
        /// {2} - channel Id.
        /// {3} - question Id.
        /// {4} - selectedMessageId.
        /// </summary>
        private const string SelectThisAnswerUrlFormat = "{0}/taskmodule/course/{1}/channel/{2}/question/{3}/selectedResponse/{4}?userObjectId={{userObjectId}}&theme={{theme}}&locale={{locale}}&subEntityId={{subEntityId}}";

        /// <summary>
        /// Error page url format.
        /// {0} - base url.
        /// {1} - error code.
        /// </summary>
        private const string ErrorPageUrlFormat = "{0}/error/{1}";

        private readonly IAppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class.
        /// </summary>
        /// <param name="appSettings">App Settings.</param>
        public UrlProvider(IAppSettings appSettings)
        {
            this.appSettings = appSettings ?? throw new System.ArgumentNullException(nameof(appSettings));
        }

        /// <inheritdoc/>
        public Task<string> GetErrorPageUrlAsync(string errorCode)
        {
            return Task.FromResult(string.Format(CultureInfo.InvariantCulture, ErrorPageUrlFormat, this.appSettings.BaseUrl, errorCode));
        }

        /// <inheritdoc/>
        public Task<string> GetSelectAnswerPageUrlAsync(string courseId, string channelId, string questionId)
        {
            return Task.FromResult(string.Format(
                CultureInfo.InvariantCulture,
                SelectAnswerUrlFormat,
                this.appSettings.BaseUrl,
                courseId,
                channelId,
                questionId));
        }

        /// <inheritdoc/>
        public Task<string> GetSelectThisAnswerPageUrlAsync(string courseId, string channelId, string questionId, string selectedMessageId)
        {
            return Task.FromResult(string.Format(
                CultureInfo.InvariantCulture,
                SelectThisAnswerUrlFormat,
                this.appSettings.BaseUrl,
                courseId,
                channelId,
                questionId,
                selectedMessageId));
        }
    }
}
