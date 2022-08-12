namespace Microsoft.Teams.Apps.QBot.Domain.Questions
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Answer validator.
    /// </summary>
    internal interface IAnswerValidator
    {
        /// <summary>
        /// Checks if the answer object passed in valid or not.
        /// </summary>
        /// <param name="answer">Answer.</param>
        /// <returns>if the answer object passed in valid or not.</returns>
        bool IsValid(Answer answer);
    }
}
