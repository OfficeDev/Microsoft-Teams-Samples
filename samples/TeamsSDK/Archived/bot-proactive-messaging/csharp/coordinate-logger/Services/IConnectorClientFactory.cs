// <copyright file="IConnectorClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.CoordinateLogger.Services
{
    using Microsoft.Bot.Connector;

    /// <summary>
    /// An IConnectorClientFactory is responsible for managing connector clients to BotFramework.
    /// </summary>
    public interface IConnectorClientFactory
    {
        /// <summary>
        /// Get the <see cref="ConnectorClient" /> for the serviceUrl.
        /// </summary>
        /// <param name="serviceUrl">The service url.</param>
        /// <returns>A connector client for the given service url.</returns>
        ConnectorClient GetConnectorClient(string serviceUrl);
    }
}