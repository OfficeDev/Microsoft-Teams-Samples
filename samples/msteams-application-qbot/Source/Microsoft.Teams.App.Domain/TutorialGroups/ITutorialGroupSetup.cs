namespace Microsoft.Teams.Apps.QBot.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Holds operations involed in setting-up a tutorial group.
    ///
    /// Includes following:
    /// 1. Updating tutorial group information (Name only).
    /// 2. Managing members in a tutorial group.
    /// </summary>
    public interface ITutorialGroupSetup
    {
        /// <summary>
        /// Gets tutorial group.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>Tutorial group.</returns>
        Task<TutorialGroup> GetTutorialGroupAsync(string tutorialGroupId);

        /// <summary>
        /// Get all the members in a tutorial group
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>List of members.</returns>
        Task<IEnumerable<Member>> GetAllTutorialGroupMembersAsync(string tutorialGroupId);

        /// <summary>
        /// Updates an existing tutorial group.
        ///
        /// Note: Only name changes are allowed.
        /// </summary>
        /// <param name="tutorialGroup">Updated tutorial group.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateTutorialGroupAsync(TutorialGroup tutorialGroup);
    }
}
