// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.IServices
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// Get information about a user from Graph
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get <see cref="User"/> for a User based on their details in Microsoft Graph
        /// </summary>
        /// <param name="userId">The AADId/User Id of the user</param>
        /// <returns>A <see cref="User"/></returns>
        Task<User> GetUser(string userId);
    }
}
