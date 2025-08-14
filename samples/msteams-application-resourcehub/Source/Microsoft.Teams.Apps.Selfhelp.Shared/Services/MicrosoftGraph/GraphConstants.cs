namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph
{
    /// <summary>
    /// Microsoft Graph Constants.
    /// </summary>
    public class GraphConstants
    {
        /// <summary>
        /// Microsoft Graph version 1.0 base Url.
        /// </summary>
        public const string V1BaseUrl = "https://graph.microsoft.com/v1.0";

        /// <summary>
        /// Microsoft Graph Beta base url.
        /// </summary>
        public const string BetaBaseUrl = "https://graph.microsoft.com/beta";

        /// <summary>
        /// Max page size.
        /// </summary>
        public const int MaxPageSize = 999;

        /// <summary>
        /// Max retry for Graph API calls.
        /// Note: Max value allowed is 10.
        /// </summary>
        public const int MaxRetry = 5;
    }
}