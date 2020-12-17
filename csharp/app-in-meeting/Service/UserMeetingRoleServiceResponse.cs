// <copyright file="UserMeetingRoleServiceResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using TokenApp.Models;

    /// <summary>
    /// User role response model.
    /// </summary>
    public class UserMeetingRoleServiceResponse
    {
        /// <summary>
        /// Gets or sets the user's role in the meeting.
        /// </summary>
        public UserRole UserRole { get; set; }
    }
}