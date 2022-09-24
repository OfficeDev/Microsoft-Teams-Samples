// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Users
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// User reader service interface.
    /// </summary>
    public interface IUserReaderService
    {
        /// <summary>
        /// Gets user's profile.
        /// Looks-up in the database, if not found, fetches the profile from MS Graph.
        /// </summary>
        /// <param name="userAadId">User's Aad id.</param>
        /// <param name="fetchProfilePic">If user's profile pic should be fetched.</param>
        /// <returns>User's profile.</returns>
        Task<User> GetUserAsync(string userAadId, bool fetchProfilePic);

        /// <summary>
        /// Gets list of users.
        /// Looks-up in the database, if not found, fetches the profile from MS Graph.
        /// </summary>
        /// <param name="userAadIds">List of user Aad ids.</param>
        /// <param name="fetchProfilePic">If user's profile pic should be fetched.</param>
        /// <returns>List of users.</returns>
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userAadIds, bool fetchProfilePic);
    }
}
