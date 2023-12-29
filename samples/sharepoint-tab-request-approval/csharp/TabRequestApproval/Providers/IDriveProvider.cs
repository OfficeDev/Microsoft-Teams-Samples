// <copyright file="IDriveProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Interface for the drive provider.
    /// </summary>
    public interface IDriveProvider
    {
        /// <summary>
        /// Retrieves the root of the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <returns>A drive item.</returns>
        Task<DriveItem> GetDriveRootAsync(string accessToken, string driveId);

        /// <summary>
        /// Retrieves the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Drive> GetDriveAsync(string accessToken, string driveId);
    }
}
