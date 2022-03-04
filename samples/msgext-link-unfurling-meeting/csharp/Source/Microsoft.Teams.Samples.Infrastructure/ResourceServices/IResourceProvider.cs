// <copyright file="IResourceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Resource provider contract.
    ///
    /// Provides method to fetch a resource.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Fetches resource object.
        /// </summary>
        /// <param name = "resourceId">Resource Id.</param>
        /// <returns>Resource.</returns>
        Task<Resource> GetResourceAsync(string resourceId);
    }
}
