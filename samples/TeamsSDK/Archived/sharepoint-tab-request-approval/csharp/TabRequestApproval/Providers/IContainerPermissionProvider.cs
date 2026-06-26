// <copyright file="IContainerPermissionProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TabActivityFeed.Models;

    /// <summary>
    /// Interface for the container permissions provider.
    /// </summary>
    public interface IContainerPermissionProvider
    {
        /// <summary>
        /// Creates container permissions.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permission">Represents the permission to create.</param>
        /// <returns>A container permission model.</returns>
        Task<ContainerPermission> CreateContainerPermissionAsync(string accessToken, string containerId, ContainerPermission permission);

        /// <summary>
        /// Reads the specified container permissions.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>A list of container permission models.</returns>
        Task<IEnumerable<ContainerPermission>> ReadContainerPermissionAsync(string accessToken, string containerId);

        /// <summary>
        /// Updates the container permissions.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permissionId">Represents the permission id.</param>
        /// <param name="role">Represents the role.</param>
        /// <returns>A container permission model.</returns>
        Task<ContainerPermission> UpdateContainerPermissionAsync(string accessToken, string containerId, string permissionId, string role);

        /// <summary>
        /// Deletes the container permissions.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permissionId">Represents the permission id.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteContainerPermissionAsync(string accessToken, string containerId, string permissionId);
    }
}
