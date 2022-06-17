// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.IServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Team information service interface.
    /// </summary>
    public interface ITeamInfoService
    {
        /// <summary>
        /// Gets a list of team owners.
        /// </summary>
        /// <param name="teamAadId">Team's add id.</param>
        /// <returns>List of team owners AAD Id.</returns>
        Task<IEnumerable<string>> GetTeamOwnersIdsAsync(string teamAadId);

        /// <summary>
        /// Gets Base64 encoded course photo.
        /// </summary>
        /// <param name="teamAadId">CourseId</param>
        /// <returns>Base64 encoded course photo.</returns>
        Task<string> GetTeamPhotoAsync(string teamAadId);

        /// <summary>
        /// Gets Base64 encoded team photo list.
        /// </summary>
        /// <param name="teamAadIds">teamAadId</param>
        /// <returns>Base64 encoded course photo dictionary.</returns>
        Task<IDictionary<string, string>> GetTeamsPhotoAsync(IEnumerable<string> teamAadIds);
    }
}
