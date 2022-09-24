namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System.Threading.Tasks;

    /// <summary>
    /// Url Provider contract.
    /// </summary>
    public interface IUrlProvider
    {
        /// <summary>
        /// Returns select answer page url.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <returns>Select answer page url.</returns>
        Task<string> GetSelectAnswerPageUrlAsync(string courseId, string channelId, string questionId);

        /// <summary>
        /// Returns select a specific message as answer page url.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionId">Question Id.</param>
        /// <param name="selectedMessageId">Selected message Id.</param>
        /// <returns>Select a specific message as answer page url.</returns>
        Task<string> GetSelectThisAnswerPageUrlAsync(string courseId, string channelId, string questionId, string selectedMessageId);

        /// <summary>
        /// Returns error page url for the given error code.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <returns>Error page url.</returns>
        Task<string> GetErrorPageUrlAsync(string errorCode);
    }
}
