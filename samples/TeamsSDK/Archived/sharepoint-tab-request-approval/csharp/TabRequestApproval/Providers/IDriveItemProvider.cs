// <copyright file="IDriveItemProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Interface for the drive item provider.
    /// </summary>
    public interface IDriveItemProvider
    {
        /// <summary>
        /// Creates a file (drive items can be any sort of file).
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="parentId">Represents the parent id.</param>
        /// <param name="name">Represents the name.</param>
        /// <param name="stream">Represents the file stream.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateFileAsync(string accessToken, string driveId, string parentId, string name, System.IO.Stream stream);

        /// <summary>
        /// Retrieves all items within the specified drive with the specified item id.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A collection of drive items.</returns>
        Task<ICollection<DriveItem>> GetDriveItemsAsync(string accessToken, string driveId, string itemId = null);

        /// <summary>
        /// Retrieves the specified drive item from the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<DriveItem> GetDriveItemAsync(string accessToken, string driveId, string itemId);

        /// <summary>
        /// Updates the drive item.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <param name="driveItem">Represents the updated drive item.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<DriveItem> UpdateDriveItemAsync(string accessToken, string driveId, string itemId, Stream driveItem);

        /// <summary>
        /// Deletes the specified drive item.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteDriveItemAsync(string accessToken, string driveId, string itemId);

        /// <summary>
        /// Retrieves the item's download url.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A Uri.</returns>
        Task<HttpResponseMessage> GetFileDownloadUrlAsync(string accessToken, string driveId, string itemId);
    }
}
