namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;

    /// <summary>
    /// Answer entity.
    /// </summary>
    internal class AnswerEntity
    {
        /// <summary>
        /// Gets or sets answer's Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets question Id.
        /// </summary>
        public string QuestionId { get; set; }

        /// <summary>
        /// Gets or sets course's id where the answer is posted.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets channel's id where the answer is posted.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets Teams message id corresponding to the answer.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets user's Teams Id who posted the answer.
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets user's Team Id who accepted the answer.
        /// </summary>
        public string AcceptedById { get; set; }

        /// <summary>
        /// Gets or sets timestamp when this answer was posted.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets question entity.
        /// </summary>
        public virtual QuestionEntity Question { get; set; }
    }
}
