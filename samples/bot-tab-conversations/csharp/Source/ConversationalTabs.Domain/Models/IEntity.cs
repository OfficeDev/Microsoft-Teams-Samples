// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

/// <summary>
/// When a Microsoft Teams tab is created an EntityId is assigned to it.
/// This Model defines an Entity around that Id.
/// </summary>
public interface IEntity<T> : IRepositoryObject where T: ISubEntity
{
    /// <summary>
    /// For Conversational Tabs, and Entity requires a Channel to create messages.
    /// This Channel will be where, when an incoming event occurs, the conversation will be created.
    /// </summary>
    MsTeamsBotData ProactiveBotData { get; }

    /// <summary>
    /// In conversational tabs, sub entities refer to conversations that might be started under the entity.
    /// </summary>
    ICollection<T> SubEntities { get; }
}
