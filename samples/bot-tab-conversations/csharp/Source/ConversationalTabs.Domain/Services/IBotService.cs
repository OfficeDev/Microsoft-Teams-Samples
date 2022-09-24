// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using Microsoft.Bot.Schema;

public interface IBotService
{
    /// <summary>
    /// Creates a new conversation in Bot Framework
    /// </summary>
    /// <param name="message">Message that will be sent as an activity</param>
    /// <param name="serviceUri">Service URI of the Bot</param>
    /// <param name="teamChannelId">Team ChannelId might begin with '19:'</param>
    /// <param name="tenantId">AAD Tenant Id of the Team's instance</param>
    /// <param name="botId">The App Id of the Bot</param>
    /// <returns>The created conversation resource</returns>
    Task<ConversationResourceResponse> CreateConversation(string message, Uri serviceUri, string teamChannelId, string tenantId, string botId);

    /// <summary>
    /// Creates a new conversation in Bot Framework
    /// </summary>
    /// <param name="attachment">Attachment that will be sent as an activity, this could contain an Adaptive Card</param>
    /// <param name="serviceUri">Service URI of the Bot</param>
    /// <param name="teamChannelId">Team ChannelId might begin with '19:'</param>
    /// <param name="tenantId">AAD Tenant Id of the Team's instance</param>
    /// <param name="botId">The App Id of the Bot</param>
    /// <returns>The created conversation resource</returns>
    Task<ConversationResourceResponse> CreateConversation(Attachment attachment, Uri serviceUri, string teamChannelId, string tenantId, string botId);
}
