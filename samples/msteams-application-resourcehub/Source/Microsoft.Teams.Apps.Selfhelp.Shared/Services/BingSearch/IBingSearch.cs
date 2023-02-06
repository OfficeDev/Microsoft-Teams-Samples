namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.BingSearch
{
    /// <summary>
    /// This interface represent the bing search result details.
    /// </summary>
    public interface IBingSearch
    {
        /// <summary>
        /// Search bing service.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>Returns search result.</returns>
        Task<BingSearchResult> GetBingSearchResultsAsync(string query);
    }
}