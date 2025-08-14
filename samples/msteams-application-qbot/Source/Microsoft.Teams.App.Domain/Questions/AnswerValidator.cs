namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using System;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Answer validator.
    /// </summary>
    internal sealed class AnswerValidator : IAnswerValidator
    {
        /// <inheritdoc/>
        public bool IsValid(Answer answer)
        {
            if (answer is null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            if (string.IsNullOrEmpty(answer.CourseId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(answer.ChannelId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(answer.AuthorId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(answer.QuestionId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(answer.AcceptedById))
            {
                return false;
            }

            if (string.IsNullOrEmpty(answer.Message))
            {
                return false;
            }

            return true;
        }
    }
}
