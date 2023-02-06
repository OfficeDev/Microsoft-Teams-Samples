// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;

using Microsoft.Bot.Connector;

public interface IConnectorClientFactory
{
    ConnectorClient CreateConnectorClient(Uri serviceUrl);
}
