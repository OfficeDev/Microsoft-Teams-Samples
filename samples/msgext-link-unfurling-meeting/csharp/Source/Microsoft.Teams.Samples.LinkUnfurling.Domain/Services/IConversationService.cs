// <copyright file="IConversationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Services
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Conversation service contract.
    ///
    /// Conversation service exposes methods to manage apps and tabs for a conversation.
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Reads join meeting url to the chat.
        /// </summary>
        /// <param name="conversationContext">Conversation context.</param>
        /// <param name="userId">User's AAD Id.</param>
        /// <returns>Join Online meeting url.</returns>
        Task<string> GetJoinMeetingUrlForChatAsync(ConversationContext conversationContext, string userId);

        /// <summary>
        /// Adds/Installs application to conversation if its not already installed.
        /// </summary>
        /// <param name="conversationContext">Conversation context.</param>
        /// <returns>Installation id.</returns>
        Task<string> AddApplicationToConversationAsync(ConversationContext conversationContext);

        /// <summary>
        /// Adds tab to conversation if its not already added.
        /// </summary>
        /// <param name="conversationContext">Conversation context.</param>
        /// <param name="tabInfo">Tab info.</param>
        /// <returns>Tab id.</returns>
        Task<string> AddTabToConversationAsync(ConversationContext conversationContext, TabInfo tabInfo);
    }
}
