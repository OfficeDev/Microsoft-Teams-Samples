﻿// <copyright file="DriveProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
	using System.Threading.Tasks;
	using Microsoft.Graph;
	using TabActivityFeed.Helpers;

	/// <summary>
	/// Represents the drive provider.
	/// </summary>
	public class DriveProvider : IDriveProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriveProvider"/> class.
        /// </summary>
        public DriveProvider()
        {
        }

        /// <summary>
        /// Retrieves the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Drive> GetDriveAsync(string accessToken, string driveId)
        {
            GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

            Drive drive = await graphServiceClient.Drives[driveId].Request().GetAsync().ConfigureAwait(false);

            return drive;
        }

        /// <summary>
        /// Retrieves the root of the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <returns>A drive item.</returns>
        public async Task<DriveItem> GetDriveRootAsync(string accessToken, string driveId)
        {
            GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

            DriveItem driveItem = await graphServiceClient.Drives[driveId].Root.Request().GetAsync().ConfigureAwait(false);

            return driveItem;
        }
    }
}
