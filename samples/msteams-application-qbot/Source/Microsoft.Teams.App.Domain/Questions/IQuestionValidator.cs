namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Question validator interface.
    /// </summary>
    internal interface IQuestionValidator
    {
        /// <summary>
        /// Checks if the question object passed in valid or not.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <returns>if the question object passed in valid or not.</returns>
        bool IsValid(Question question);
    }
}
