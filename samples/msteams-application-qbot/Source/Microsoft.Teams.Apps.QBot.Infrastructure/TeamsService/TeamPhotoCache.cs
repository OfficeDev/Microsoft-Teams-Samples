namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// Teams Photo cache.
    /// </summary>
    internal class TeamPhotoCache : ITeamPhotoCache
    {
        /// <summary>
        /// Memory cache key format.
        ///
        /// {0} course id.
        /// </summary>
        private const string MemoryCacheKeyFormat = "team-{0}";

        private readonly MemoryCache cache;
        private readonly MemoryCacheEntryOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamPhotoCache"/> class.
        /// </summary>
        public TeamPhotoCache()
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
        public string ReadFromCache(string teamAadId)
        {
            var key = this.GetCacheKey(teamAadId);
            this.cache.TryGetValue(key, out string dataUri);
            return dataUri;
        }

        /// <inheritdoc/>
        public void WriteToCache(string teamAadId, string dataUri)
        {
            var key = this.GetCacheKey(teamAadId);
            this.cache.Set(key, dataUri, this.options);
        }

        private string GetCacheKey(string courseId)
        {
            return string.Format(MemoryCacheKeyFormat, courseId);
        }
    }
}
