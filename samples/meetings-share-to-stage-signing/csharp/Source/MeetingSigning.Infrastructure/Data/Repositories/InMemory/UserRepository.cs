// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// UserRepository is an inmemory implementation of IUserRepository
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDictionary<string, User> _userDictionary;

        public UserRepository()
        {
            _userDictionary = new Dictionary<string, User>();
        }

        /// <summary>
        /// AddUser inserts a new user to inmemory store.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Task</returns>
        public async Task AddUser(User user)
        {
            if (await UserExists(user.UserId))
            {
                return;
            }

            _userDictionary.Add(user.UserId, user);
        }

        /// <summary>
        /// GetUser tries to find an existing user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User found or null</returns>
        public Task<User> GetUser(string userId)
        {
            if (_userDictionary.TryGetValue(userId, out User? existingUser))
            {
                return Task.FromResult(existingUser.DeepCopy());
            }
            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"User with id {userId} was not found.");
        }

        /// <inheritdoc/>
        public Task<bool> UserExists(string userId)
        {
            return Task.FromResult(_userDictionary.ContainsKey(userId));
        }
    }
}
