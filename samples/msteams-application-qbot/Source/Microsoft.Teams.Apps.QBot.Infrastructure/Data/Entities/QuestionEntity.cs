// <copyright file="QuestionEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;

    /// <summary>
    /// Question entity.
    /// </summary>
    internal class QuestionEntity
    {
        /// <summary>
        /// Gets or sets question's Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets course's id where the question is posted.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets channel's id where the question is posted.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets Teams message id corresponding to the question.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets user's Teams Id who posted the question.
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Answer"/>'s Id.
        /// </summary>
        public string AnswerId { get; set; }

        /// <summary>
        /// Gets or sets initial response message id.
        /// </summary>
        public string InitialResponseMessageId { get; set; }

        /// <summary>
        /// Gets or sets timestamp when this question was posted.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets answer.
        /// </summary>
        public virtual AnswerEntity Answer { get; set; }
    }
}
