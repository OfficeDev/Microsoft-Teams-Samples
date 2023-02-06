// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services
{
    using Microsoft.Graph;

    /// <summary>
    /// Service to provide infomation about a user's Teams
    /// </summary>
    public interface IUserTeamsService
    {
        /// <summary>
        /// Returns all of the authenticated user's Teams
        /// </summary>
        /// <returns>A HashSet of the user's Teams</returns>
        Task<IEnumerable<Team>> GetUsersTeamsAsync();

        /// <summary>
        /// Checks if the authenticated user is a member of a Team <paramref name="aadGroupId"/>
        /// </summary>
        /// <param name="aadGroupId"></param>
        /// <returns>True if a member, false otherwise</returns>
        Task<bool> IsUserAMemberOfTeamAsync(string aadGroupId);
    }
}
