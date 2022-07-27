// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="TutorialGroup"/> repository.
    /// </summary>
    public interface ITutorialGroupRepository
    {
        /// <summary>
        /// Affs tutorial group to the DB.
        /// </summary>
        /// <param name="tutorialGroup">Tutorial group.</param>
        /// <returns>Async task.</returns>
        Task AddTutorialGroupAsync(TutorialGroup tutorialGroup);

        /// <summary>
        /// Adds multiple tutorial groups to the DB.
        /// </summary>
        /// <param name="tutorialGroups">List of tutorial groups.</param>
        /// <returns>Async task.</returns>
        Task AddTutorialGroupsAsync(IEnumerable<TutorialGroup> tutorialGroups);

        /// <summary>
        /// Updates tutorial group.
        /// </summary>
        /// <param name="tutorialGroup">updated tutorial group.</param>
        /// <returns>Async taks.</returns>
        Task UpdateTutorialGroupAsync(TutorialGroup tutorialGroup);

        /// <summary>
        /// Deletes tutorial group.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>Async task.</returns>
        Task DeleteTutorialGroupAsync(string tutorialGroupId);

        /// <summary>
        /// Gets tutorial group object.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>Tutorial group.</returns>
        Task<TutorialGroup> GetTutorialGroupAsync(string tutorialGroupId);

        /// <summary>
        /// Gets all tutorial groups in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>List of tutorial groups.</returns>
        Task<IEnumerable<TutorialGroup>> GetAllTutorialGroupsAsync(string courseId);
    }
}
