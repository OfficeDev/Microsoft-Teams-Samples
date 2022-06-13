// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories
{
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public interface IUserRepository
    {
        /// <summary>
        /// AddUser inserts a given user if not already present in the database. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Task</returns>
        Task AddUser(User user);

        /// <summary>
        /// GetUser finds a given user with the userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User thus found</returns>
        Task<User> GetUser(string userId);

        /// <summary>
        /// Checks if a User exists in the repository
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>If the user exists</returns>
        Task<bool> UserExists(string userId);
    }
}
