// <copyright file="UserRespository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// User repository implementation.
    /// </summary>
    internal class UserRepository : IUserRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<UserRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db context.</param>
        /// <param name="mapper">Auto mapper.</param>
        /// <param name="logger">Logger.</param>
        public UserRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<UserRepository> logger)
        {
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddOrUpdateUserAsync(User user)
        {
            try
            {
                // Update if user exists.
                if (this.dbContext.Users.Find(user.AadId) != null)
                {
                    await this.UpdateUserAsync(user);
                    return;
                }

                // Add user otherwise.
                var userEntity = this.mapper.Map<UserEntity>(user);
                this.dbContext.Add(userEntity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to update user {user.AadId}.");
                throw exception;
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add user {user.AadId}.";
                this.logger.LogWarning(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteUserAsync(string userAadId)
        {
            var userEntity = this.dbContext.Users.Find(userAadId);
            if (userEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"{nameof(this.DeleteUserAsync)}: User {userAadId} not found!");
            }

            try
            {
                this.dbContext.Users.Remove(userEntity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to delete user: {userAadId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userAadId)
        {
            var userEntity = await this.dbContext.Users.FindAsync(userAadId);
            if (userEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetUserAsync)}: User {userAadId} not found!");
            }

            return this.mapper.Map<User>(userEntity);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userAadIds)
        {
            var result = new List<User>();
            foreach (var userId in userAadIds)
            {
                try
                {
                    var user = await this.GetUserAsync(userId);
                    result.Add(user);
                }
                catch (QBotException exception)
                {
                    this.logger.LogWarning(exception, $"{nameof(this.GetUsersAsync)} Failed to fetch user's profile. Skipping the user [{userId}].");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersWithNoCourseAsync()
        {
            var users = await this.dbContext.Users
                .Include(u => u.CourseMembership)
                .Where(u => u.CourseMembership.Count == 0)
                .ToListAsync();

            IEnumerable<User> result = new List<User>();
            if (users != null)
            {
                result = this.mapper.Map<IEnumerable<User>>(users);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task UpdateUserAsync(User updatedUser)
        {
            var userEntity = await this.dbContext.Users.FindAsync(updatedUser.AadId);
            if (userEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.UpdateUserAsync)}: User {updatedUser.AadId} not found!");
            }

            userEntity.Name = updatedUser.Name;
            if (string.IsNullOrEmpty(userEntity.TeamId))
            {
                userEntity.TeamId = updatedUser.TeamId;
            }

            try
            {
                this.dbContext.Users.Update(userEntity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update user: {updatedUser.AadId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}
