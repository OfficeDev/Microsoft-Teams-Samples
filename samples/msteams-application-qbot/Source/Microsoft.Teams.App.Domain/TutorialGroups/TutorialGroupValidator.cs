namespace Microsoft.Teams.Apps.QBot.Domain
{
    using System;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="TutorialGroup"/> validator.
    /// </summary>
    internal sealed class TutorialGroupValidator : ITutorialGroupValidator
    {
        /// <inheritdoc/>
        public bool IsValid(TutorialGroup tutorialGroup)
        {
            if (tutorialGroup == null)
            {
                throw new ArgumentNullException(nameof(tutorialGroup));
            }

            if (string.IsNullOrEmpty(tutorialGroup.DisplayName))
            {
                return false;
            }

            if (string.IsNullOrEmpty(tutorialGroup.CourseId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(tutorialGroup.ShortCode))
            {
                return false;
            }

            return true;
        }
    }
}
