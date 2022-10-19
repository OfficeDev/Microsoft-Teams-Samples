namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide bing search settings for application.
    /// </summary>
    public class BingSearchSettings
    {
        /// <summary>
        /// Gets or sets application base Uri.
        /// </summary>
        public string BingSearchEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the bing serarch service subscription key.
        /// </summary>
        public string BingSearchSubscriptionKey { get; set; }
    }
}