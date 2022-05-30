// <copyright file="IAppSettingsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Repositories
{
    using System.Threading.Tasks;

    /// <summary>
    /// App settings repository interface.
    /// </summary>
    public interface IAppSettingsRepository
    {
        /// <summary>
        /// Gets service url.
        /// </summary>
        /// <returns>Service url.</returns>
        Task<string> GetServiceUrlAsync();

        /// <summary>
        /// Adds or updates service url.
        /// </summary>
        /// <param name="serviceUrl">Service url.</param>
        /// <returns>Async task.</returns>
        Task AddOrUpdateServiceUrlAsync(string serviceUrl);
    }
}
