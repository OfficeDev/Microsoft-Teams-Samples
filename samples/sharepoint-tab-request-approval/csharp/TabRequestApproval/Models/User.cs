// <copyright file="User.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    /// <summary>
    /// User model.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Gets or sets the user principal name.
        /// </summary>
        public string userPrincipalName { get; set; }
    }
}
