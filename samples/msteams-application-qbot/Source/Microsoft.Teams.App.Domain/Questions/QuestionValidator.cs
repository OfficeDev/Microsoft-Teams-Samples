namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using System;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Question validator.
    /// </summary>
    internal sealed class QuestionValidator : IQuestionValidator
    {
        /// <inheritdoc/>
        public bool IsValid(Question question)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (string.IsNullOrEmpty(question.CourseId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(question.ChannelId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(question.MessageId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(question.AuthorId))
            {
                return false;
            }

            return true;
        }
    }
}
