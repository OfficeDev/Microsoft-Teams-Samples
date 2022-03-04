// <copyright file="ConversationType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Conversation type enum.
    /// </summary>
    public enum ConversationType
    {
        /// <summary>
        /// 1:1 chat with a user or a bot.
        /// </summary>
        Personal,

        /// <summary>
        /// Group chat.
        /// </summary>
        GroupChat,

        /// <summary>
        /// Team channel.
        /// </summary>
        Channel,
    }
}
