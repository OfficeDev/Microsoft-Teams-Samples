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
            if (user.UserId == null && user.Email == null)
            {
                throw new ApiException(HttpStatusCode.BadRequest, ErrorCode.InvalidOperation, "Unable to add a user which has no UserId or Email.");
            }

            if (await UserExists(user.Id))
            {
                return;
            }

            _userDictionary.Add(user.Id, user);
        }

        /// <summary>
        /// GetUser tries to find an existing user
        /// </summary>
        /// <param name="id"></param>
        /// <returns>User found or null</returns>
        public Task<User> GetUser(string id)
        {
            if (_userDictionary.TryGetValue(id, out User? existingUser))
            {
                return Task.FromResult(existingUser.DeepCopy());
            }
            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"User with id {id} was not found.");
        }

        /// <inheritdoc/>
        public Task<bool> UserExists(string id)
        {
            return Task.FromResult(_userDictionary.ContainsKey(id));
        }
    }
}
