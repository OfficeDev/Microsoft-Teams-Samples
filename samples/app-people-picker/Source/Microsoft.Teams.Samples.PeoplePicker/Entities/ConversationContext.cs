// <copyright file="ConversationContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Entities
{
    /// <summary>
    /// Conversation entity definition.
    /// </summary>
    public class ConversationContext
    {
        /// <summary>
        /// Gets or sets title of Conversation.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Proactive welcome message in the conversation.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets Users (AAD-ID) in the conversation.
        /// </summary>
        public string[] Users { get; set; }

        /// <summary>
        /// Gets or sets Conversation Id.
        /// </summary>
        public string ConversationId { get; set; }
    }
}
