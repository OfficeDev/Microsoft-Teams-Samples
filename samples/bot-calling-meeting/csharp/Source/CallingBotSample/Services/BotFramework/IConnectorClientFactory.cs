// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Bot.Connector;

namespace CallingBotSample.Services.BotFramework
{
    public interface IConnectorClientFactory
    {
        ConnectorClient CreateConnectorClient();
    }
}
