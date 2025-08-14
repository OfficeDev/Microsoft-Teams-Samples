// <copyright file="IUserProfilePictureCache.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    /// <summary>
    /// User profile picture cache interface.
    /// </summary>
    internal interface IUserProfilePictureCache
    {
        /// <summary>
        /// Reads user profile pic from cache.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>User's profile pic.</returns>
        string ReadFromCache(string userId);

        /// <summary>
        /// Writes user's profile pic to cache.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="profilePic">User's profile pic.</param>
        void WriteToCache(string userId, string profilePic);
    }
}
