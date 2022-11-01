namespace Microsoft.Teams.Apps.Selfhelp.Helper
{
    /// <summary>
    /// Send message response object.
    /// </summary>
    public class SendMessageResponse
    {
        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the result type.
        /// </summary>
        public SendMessageResult ResultType { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list representing all of the status code responses received when trying
        /// to send the notification to the recipient. These results can include success, failure, and throttle
        /// status codes.
        /// </summary>
        public string AllSendStatusCodes { get; set; }

        /// <summary>
        /// Gets or sets the number of throttle responses.
        /// </summary>
        public int TotalNumberOfSendThrottles { get; set; }

        /// <summary>
        /// Gets or sets the error message when trying to send the notification.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message when trying to send the notification.
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the error message when trying to send the notification.
        /// </summary>
        public string ChannelConversationId { get; set; }
    }
}