// <copyright file="UserRole.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// User role model.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the meeting role of a user.
        /// </summary>
        [JsonProperty("Meeting")]
        public MeetingRole Meeting { get; set; }

        /// <summary>
        /// Gets or sets the conversation Id.
        /// </summary>
        public Conversation Conversation { get; set; }
    }

    /// <summary>
    /// Meeting represents a activity conversation for bot and the chat.
    /// </summary>
    public class MeetingRole
    {
        /// <summary>
        /// Gets or sets the user role for a particular meeting.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is in a particular meeting.
        /// </summary>
        public bool InMeeting { get; set; }
    }
}