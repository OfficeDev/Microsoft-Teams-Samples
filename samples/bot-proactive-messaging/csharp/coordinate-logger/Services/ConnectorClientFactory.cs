// <copyright file="ConnectorClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.CoordinateLogger.Services
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Authentication;

    /// <summary>
    /// The ConnectorClientFactory is an implementation of the <see cref="IConnectorClientFactory" />
    /// that is responsible for creating at most one instance of a connector client per service url.
    /// </summary>
    public sealed class ConnectorClientFactory : IConnectorClientFactory
    {
        private readonly MicrosoftAppCredentials appCredentials;

        private readonly ConcurrentDictionary<string, ConnectorClient> connectorClients;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClientFactory"/> class.
        /// </summary>
        /// <param name="appCredentials">The Bot's Credentials.</param>
        public ConnectorClientFactory(
            MicrosoftAppCredentials appCredentials)
        {
            this.connectorClients = new ConcurrentDictionary<string, ConnectorClient>();
            this.appCredentials = appCredentials;
        }

        /// <inheritdoc />
        public ConnectorClient GetConnectorClient(string serviceUrl)
        {
            return this.connectorClients.GetOrAdd(serviceUrl, (url) =>
                new ConnectorClient(new Uri(url), this.appCredentials));
        }
    }
}
