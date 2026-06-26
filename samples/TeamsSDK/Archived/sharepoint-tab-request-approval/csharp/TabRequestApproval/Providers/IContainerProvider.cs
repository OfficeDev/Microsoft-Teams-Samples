// <copyright file="IContainerProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Newtonsoft.Json.Schema;
    using TabActivityFeed.Models;

    /// <summary>
    /// Interface for the container provider.
    /// </summary>
    public interface IContainerProvider
    {
        /// <summary>
        /// Reads all containers.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<Container>> GetAllContainersAsync(string accessToken);

        /// <summary>
        /// Reads the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Container> GetContainerAsync(string accessToken, string containerId);

        /// <summary>
        /// Reads the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Container> CreateOrGetContainerAsync(string accessToken, string containerId, string teamsAppInstallationScopeId);

        /// <summary>
        /// Deletes the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteContainerAsync(string accessToken, string containerId, string teamsAppInstallationScopeId);

        /// <summary>
        /// Activates the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task ActivateContainerAsync(string accessToken, string containerId);

        /// <summary>
        /// Retrieves the container id associated with the teams app installation scope.
        /// </summary>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A container id associated with the teams app installation scope.</returns>
        string GetContainerIdFromTeamsAppInstallationScopeId(string teamsAppInstallationScopeId);
    }
}
