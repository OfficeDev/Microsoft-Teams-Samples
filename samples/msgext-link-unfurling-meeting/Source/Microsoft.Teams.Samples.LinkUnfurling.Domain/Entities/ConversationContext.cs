// <copyright file="ConversationContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Conversation context.
    /// </summary>
    public class ConversationContext
    {
        /// <summary>
        /// Gets or sets conversation id.
        ///
        /// This is chatId incase of group chat, a channel id incase of a team.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets conversation type.
        /// </summary>
        public ConversationType ConversationType { get; set; }

        /// <summary>
        /// Gets or sets Team id. This should be set if the <see cref="ConversationType"/> is <see cref="ConversationType.Channel"/>.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the current conversation is linked to a meeting.
        /// </summary>
        public bool IsMeetingConversation { get; set; }

        /// <summary>
        /// Gets or sets meeting Id.
        /// </summary>
        public string MeetingId { get; set; }
    }
}
