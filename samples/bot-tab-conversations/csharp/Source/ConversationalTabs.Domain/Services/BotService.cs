// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;

public class BotService : IBotService
{
    private readonly IConnectorClientFactory _connectorClientFactory;
    private readonly ILogger<BotService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public BotService(IConnectorClientFactory connectorClientFactory, ILogger<BotService> logger)
    {
        _connectorClientFactory = connectorClientFactory;
        _logger = logger;
    }

    public Task<ConversationResourceResponse> CreateConversation(string message, Uri serviceUri, string teamChannelId, string tenantId, string botId)
    {
        return CreateConversation(MessageFactory.Text(message), serviceUri, teamChannelId, tenantId, botId);
    }

    public Task<ConversationResourceResponse> CreateConversation(Attachment attachment, Uri serviceUri, string teamChannelId, string tenantId, string botId)
    {
        return CreateConversation(MessageFactory.Attachment(attachment), serviceUri, teamChannelId, tenantId, botId);
    }

    private async Task<ConversationResourceResponse> CreateConversation(IActivity activity, Uri serviceUri, string teamChannelId, string tenantId, string botId)
    {
        ConnectorClient client = _connectorClientFactory.CreateConnectorClient(serviceUri);

        var conversationParameter = new ConversationParameters
        {
            Bot = new ChannelAccount() { Id = botId },
            IsGroup = true,
            ChannelData = CreateTeamsChannelData(teamChannelId),
            TenantId = tenantId,
            Activity = (Activity)activity
        };

        var response = await client.Conversations.CreateConversationAsync(conversationParameter);
        return response;
    }

    private TeamsChannelData CreateTeamsChannelData(string teamChannelId)
    {
        TeamsChannelData channelData = new TeamsChannelData();

        if (channelData.Channel == null)
        {
            channelData.Channel = new ChannelInfo();
        }
        channelData.Channel.Id = teamChannelId;

        return channelData;
    }
}
