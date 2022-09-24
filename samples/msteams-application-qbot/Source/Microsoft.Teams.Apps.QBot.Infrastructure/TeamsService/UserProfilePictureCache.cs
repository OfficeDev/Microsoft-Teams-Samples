// <copyright file="UserProfilePictureCache.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// User profile cache.
    /// </summary>
    internal class UserProfilePictureCache : IUserProfilePictureCache
    {
        /// <summary>
        /// Memory cache key format.
        ///
        /// {0} user's AAD id.
        /// </summary>
        private const string MemoryCacheKeyFormat = "user-{0}";

        private readonly MemoryCache cache;
        private readonly MemoryCacheEntryOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePictureCache"/> class.
        /// </summary>
        public UserProfilePictureCache()
        {
            this.cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 512,
            });

            this.options = new MemoryCacheEntryOptions()
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120),
            };
        }

        /// <inheritdoc/>
        public string ReadFromCache(string userId)
        {
            var key = this.GetCacheKey(userId);
            this.cache.TryGetValue(key, out string profilePic);
            return profilePic;
        }

        /// <inheritdoc/>
        public void WriteToCache(string userId, string profilePic)
        {
            var key = this.GetCacheKey(userId);
            this.cache.Set(key, profilePic, this.options);
        }

        private string GetCacheKey(string userId)
        {
            return string.Format(MemoryCacheKeyFormat, userId);
        }
    }
}
