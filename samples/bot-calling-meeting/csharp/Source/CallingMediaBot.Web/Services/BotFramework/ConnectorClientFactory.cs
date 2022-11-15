// Copyright (c) Microsoft Corporation. All rights reserved.

namespace CallingMediaBot.Web.Services.BotFramework;

using System.Collections.Concurrent;
using CallingMediaBot.Web.Options;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ConnectorClientFactory : IConnectorClientFactory
{
    private readonly BotOptions botOptions;
    private readonly ConcurrentDictionary<string, ConnectorClient> connectorClients = new ConcurrentDictionary<string, ConnectorClient>();
    private readonly ILogger<ConnectorClientFactory> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorClientFactory "/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public ConnectorClientFactory(IOptions<BotOptions> botOptions, ILogger<ConnectorClientFactory> logger)
    {
        this.botOptions = botOptions.Value;
        this.logger = logger;
    }

    /// <summary>
    /// Creates a connector client based on the service url.
    /// Ensures only one client per serviceUrl/appId/scope is created
    /// </summary>
    /// <returns>A Connector Client</returns>
    public ConnectorClient CreateConnectorClient()
    {
        return CreateConnectorClient(
            new Uri("https://smba.trafficmanager.net/teams/"),
            new MicrosoftAppCredentials(botOptions.AppId, botOptions.AppSecret));
    }

    private ConnectorClient CreateConnectorClient(Uri serviceUrl, AppCredentials appCredentials)
    {
        // As multiple bots can listen on a single serviceUrl, the clientKey also includes the OAuthScope.
        var clientKey = $"{serviceUrl}:{appCredentials?.MicrosoftAppId}:{appCredentials?.OAuthScope}";

        return connectorClients.GetOrAdd(clientKey, (key) =>
        {
            return new ConnectorClient(serviceUrl, appCredentials, new HttpClient());
        });
    }
}
