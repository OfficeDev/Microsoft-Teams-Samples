// <copyright file="ITeamPhotoCache.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    /// <summary>
    /// Team photo cache interface.
    /// </summary>
    internal interface ITeamPhotoCache
    {
        /// <summary>
        /// Reads Base64 encoded photo of the Team from cache.
        /// </summary>
        /// <param name="teamAadId">teamAadId.</param>
        /// <returns>Team photo.</returns>
        string ReadFromCache(string teamAadId);

        /// <summary>
        /// Writes Base64 encoded photo of the course to the cache.
        /// </summary>
        /// <param name="teamAadId">Team AAD id.</param>
        /// <param name="dataUri">Base 64 encoded photo.</param>
        void WriteToCache(string teamAadId, string dataUri);
    }
}
