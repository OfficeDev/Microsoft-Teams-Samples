// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Bot.Connector;

namespace CallingBotSample.Services.BotFramework
{
    /// <summary>
    /// Factory to for handling a Bot Framework connector client
    /// </summary>
    public interface IConnectorClientFactory
    {
        /// <summary>
        /// Creates a connector client based on the service url.
        /// Ensures only one client per serviceUrl/appId/scope is created
        /// </summary>
        /// <returns>The connector client</returns>
        ConnectorClient CreateConnectorClient();
    }
}
