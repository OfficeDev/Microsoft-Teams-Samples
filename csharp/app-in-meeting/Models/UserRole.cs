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
        [JsonProperty("MeetingRole")]
        public string MeetingRole { get; set; }

        /// <summary>
        /// Gets or sets the conversation Id.
        /// </summary>
        public Conversation Conversation { get; set; }
    }
}