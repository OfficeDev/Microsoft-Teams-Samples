// <copyright file="SetupRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Controllers
{
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Meeting setup request object.
    /// </summary>
    public class SetupRequest
    {
        /// <summary>
        /// <see cref="ConversationContext.ConversationId"/>.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// <see cref="ConversationContext.ConversationType"/>.
        /// </summary>
        public ConversationType ConversationType { get; set; }

        /// <summary>
        /// Gets or sets Team Id. Should be set for channel conversation.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// <see cref="Meeting.Id"/>. This should be set if the request is coming from a meeting chat/channel.
        /// </summary>
        public string MeetingId { get; set; }

        /// <summary>
        /// <see cref="Resource.Id"/>.
        /// </summary>
        public string ResourceId { get; set; }
    }
}
