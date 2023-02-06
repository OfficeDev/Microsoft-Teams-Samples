namespace Microsoft.Teams.Apps.QBot.Domain
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="TutorialGroup"/> validator.
    /// </summary>
    internal interface ITutorialGroupValidator
    {
        /// <summary>
        /// Checks if <see cref="TutorialGroup"/> is valid or not.
        /// </summary>
        /// <param name="tutorialGroup">Tutorial group.</param>
        /// <returns>if <see cref="TutorialGroup"/> is valid or not.</returns>
        bool IsValid(TutorialGroup tutorialGroup);
    }
}
