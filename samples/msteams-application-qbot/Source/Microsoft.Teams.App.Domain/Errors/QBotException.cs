namespace Microsoft.Teams.Apps.QBot.Domain.Errors
{
    using System;
    using System.Net;

    /// <summary>
    /// QBotException.
    /// </summary>
    public class QBotException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QBotException"/> class.
        /// </summary>
        public QBotException()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QBotException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public QBotException(string message)
            : this(message, null/*innerException*/)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QBotException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public QBotException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QBotException"/> class.
        /// </summary>
        /// <param name="statusCode">Http Status code.</param>
        /// <param name="code">Error Code.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner Exception (Optional).</param>
        public QBotException(HttpStatusCode statusCode, ErrorCode code, string message, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.Code = code;
        }

        /// <summary>
        /// Gets Http Status Code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets Error code.
        /// </summary>
        public ErrorCode Code { get; }
    }
}
