// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;

using System.Collections.Concurrent;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Settings;

public class ConnectorClientFactory : IConnectorClientFactory
{
    private readonly IOptions<BotSettings> _botSettings;
    private readonly ConcurrentDictionary<string, ConnectorClient> _connectorClients = new ConcurrentDictionary<string, ConnectorClient>();
    private readonly ILogger<ConnectorClientFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorClientFactory "/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public ConnectorClientFactory(IOptions<BotSettings> botSettings, ILogger<ConnectorClientFactory> logger)
    {
        _botSettings = botSettings;
        _logger = logger;
    }

    /// <summary>
    /// Creates a connector client based on the service url.
    /// Ensures only one client per serviceUrl/appId/scope is created
    /// </summary>
    /// <param name="serviceUrl"></param>
    /// <returns>A Connector Client</returns>
    public ConnectorClient CreateConnectorClient(Uri serviceUrl)
    {
        return CreateConnectorClient(
            serviceUrl,
            new MicrosoftAppCredentials(_botSettings.Value.MicrosoftAppId, _botSettings.Value.MicrosoftAppPassword));
    }

    private ConnectorClient CreateConnectorClient(Uri serviceUrl, AppCredentials appCredentials)
    {
        // As multiple bots can listen on a single serviceUrl, the clientKey also includes the OAuthScope.
        var clientKey = $"{serviceUrl}:{appCredentials?.MicrosoftAppId}:{appCredentials?.OAuthScope}";

        return _connectorClients.GetOrAdd(clientKey, (key) =>
        {
            return new ConnectorClient(serviceUrl, appCredentials, new HttpClient());
        });
    }
}
