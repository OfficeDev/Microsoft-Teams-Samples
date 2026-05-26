// <copyright file="IAuthProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Interface defining a provider for auth related operations.
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>
        /// Retrieves an access token for microsoft graph API.
        /// </summary>
        /// <returns>A task representing the access token.</returns>
        Task<string> GetGraphAccessTokenAsync();
    }
}
