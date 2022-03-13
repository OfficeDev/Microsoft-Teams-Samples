// <copyright file="IConversationService.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.PeoplePicker.Entities;

    /// <summary>
    /// Conversation service contract.
    /// Conversation service exposes methods to manage apps and send message for a conversation.
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Creates a group conversation.
        /// </summary>
        /// <param name="conversationContext">Provides context for the group conversation. The conversationContext object persists until new conversation is created.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> CreateConversationAsync(ConversationContext conversationContext);

        /// <summary>
        /// Add App to group conversation.
        /// </summary>
        /// <param name="conversationContext">Provides context for the group conversation. The conversationContext object persists until new conversation is created.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> AddAppToConversationAsync(ConversationContext conversationContext);

        /// <summary>
        /// Sends Proactive Message in the conversation.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot. The turnContext object is created by a BotAdapter and persists for the length of the turn.</param>
        /// <param name="conversationContext">conversationContext.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task SendProactiveMessageAsync(ITurnContext<IInvokeActivity> turnContext, ConversationContext conversationContext);
    }
}
