// Copyright (c) Microsoft Corporation. All rights reserved.

namespace CallingMediaBot.Web.Services.BotFramework;

using Microsoft.Bot.Connector;

public interface IConnectorClientFactory
{
    ConnectorClient CreateConnectorClient();
}
