namespace Microsoft.Teams.Apps.Selfhelp.Helper
{
    /// <summary>
    /// Send message result enum.
    /// </summary>
    public enum SendMessageResult
    {
        /// <summary>
        /// Type indicating sending the notification succeeded.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Type indicating sending the notification was throttled.
        /// </summary>
        Throttled,

        /// <summary>
        /// Type indicating sending the notification failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Type indicating that the recipient can't be found.
        /// When sending a notification to a removed recipient, the send function gets 404 error.
        /// The recipient should be excluded from the list.
        /// </summary>
        RecipientNotFound,

        /// <summary>
        /// Type indicating sending the SMS notification is accepted for processing.
        /// </summary>
        Queued,
    }
}