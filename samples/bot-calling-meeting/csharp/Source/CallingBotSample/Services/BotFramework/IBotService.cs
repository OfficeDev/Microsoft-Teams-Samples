// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace CallingBotSample.Services.BotFramework
{
    /// <summary>
    /// Service for calling Bot Framework
    /// </summary>
    public interface IBotService
    {
        /// <summary>
        /// Send a string message to a conversation
        /// </summary>
        /// <param name="message">Message to send to the conversation</param>
        /// <param name="conversationId">The Id of the conversation to send the message to</param>
        /// <returns>The created resource</returns>
        Task<ResourceResponse> SendToConversation(string message, string conversationId);

        /// <summary>
        /// Send an attachment to a conversation
        /// </summary>
        /// <param name="attachment">Attachment to send to the conversation</param>
        /// <param name="conversationId">The Id of the conversation to send the message to</param>
        /// <returns>The created resource</returns>
        Task<ResourceResponse> SendToConversation(Attachment attachment, string conversationId);
    }
}
