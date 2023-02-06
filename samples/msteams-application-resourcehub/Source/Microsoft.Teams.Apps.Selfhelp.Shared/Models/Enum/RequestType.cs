namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum
{
    /// <summary>
    /// Enum represents API request status.
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// This represents the request is initiated.
        /// </summary>
        Initiated,

        /// <summary>
        /// TThis represents the request is completed.
        /// </summary>
        Succeeded,

        /// <summary>
        /// This represents the request is failed
        /// </summary>
        Failed,
    }
}