namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    using System;

    /// <summary>
    /// Captures a response to a question.
    /// </summary>
    public sealed class QuestionResponse
    {
        /// <summary>
        /// Gets or sets Response Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets question's Id.
        /// </summary>
        public string QuestionId { get; set; }

        /// <summary>
        /// Gets or sets Teams message id corresponding to the response.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets response text.
        /// TODO(guptaa): define max length.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets user's Teams Id who posted the response.
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets timestamp when this response was posted.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }
    }
}
