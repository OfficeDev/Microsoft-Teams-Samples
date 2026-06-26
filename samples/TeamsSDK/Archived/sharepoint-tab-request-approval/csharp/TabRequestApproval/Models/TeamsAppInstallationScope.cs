// <copyright file="TeamsAppInstallationScope.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    /// <summary>
    /// An enum representing the scope of the request.
    /// </summary>
    public enum TeamsAppInstallationScope
    {
        /// <summary>
        /// Represents the team scope. The app is installed in a team.
        /// </summary>
        TeamScope,

        /// <summary>
        /// Represents the chat scope. The app is installed in a chat.
        /// </summary>
        ChatScope,

        /// <summary>
        /// Represents the user scope. The app is installed to the user's apps page.
        /// </summary>
        UserScope,
    }
}
