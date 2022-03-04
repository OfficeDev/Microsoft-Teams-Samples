// <copyright file="IGraphClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using Microsoft.Graph;

    /// <summary>
    /// Graph client factory.
    /// </summary>
    public interface IGraphClientFactory
    {
        /// <summary>
        /// Gets authenticated instance of <see cref="GraphServiceClient"/>.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <returns><see cref="GraphServiceClient"/>.</returns>
        GraphServiceClient GetAuthenticatedGraphClient(string accessToken);
    }
}
