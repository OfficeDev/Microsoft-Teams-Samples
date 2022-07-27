// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Users
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// User reader service implementation.
    /// </summary>
    internal class UserReaderService : IUserReaderService
    {
        private readonly IUserRepository userRepository;
        private readonly IUserProfileService userProfileService;
        private readonly ILogger<UserReaderService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReaderService"/> class.
        /// </summary>
        /// <param name="userRepository">User repository.</param>
        /// <param name="userProfileService">User profile service.</param>
        /// <param name="logger">Logger.</param>
        public UserReaderService(
            IUserRepository userRepository,
            IUserProfileService userProfileService,
            ILogger<UserReaderService> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userAadId, bool fetchProfilePic)
        {
            try
            {
                var user = await this.userRepository.GetUserAsync(userAadId);
                if (fetchProfilePic)
                {
                    user.ProfilePicUrl = await this.userProfileService.GetUserProfilePicAsync(userAadId);
                }

                return user;
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to read user {userAadId} profile.");
                return await this.FetchAndStoreUserProfileAsync(userAadId, fetchProfilePic);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userAadIds, bool fetchProfilePic)
        {
            var result = new List<User>();
            foreach (var userAadId in userAadIds)
            {
                try
                {
                    // Note: Its possible optimize with batch graph calls.
                    var user = await this.GetUserAsync(userAadId, fetchProfilePic);
                    result.Add(user);
                }
                catch (QBotException exception)
                {
                    this.logger.LogWarning(exception, $"Failed to fetch user's ${userAadId} profile.");
                }
            }

            return result;
        }

        private async Task<User> FetchAndStoreUserProfileAsync(string userAadId, bool fetchProfilePic)
        {
            try
            {
                // Fetch user profile.
                var user = await this.userProfileService.GetUserProfileAsync(userAadId, fetchProfilePic);

                // Store in database.
                await this.userRepository.AddOrUpdateUserAsync(user);
                return user;
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, "Failed to fetch user profile.");
                throw;
            }
        }
    }
}
