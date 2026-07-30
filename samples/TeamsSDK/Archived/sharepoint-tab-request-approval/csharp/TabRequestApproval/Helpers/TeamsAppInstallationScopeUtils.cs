// <copyright file="TeamsAppInstallationScopeUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Helpers
{
    using System;
    using TabActivityFeed.Models;

    /// <summary>
    /// Teams app installation scope utility class.
    /// </summary>
    public static class TeamsAppInstallationScopeUtils
    {
        /// <summary>
        /// Creates a unique id that must be used to uniquely identify containers.
        /// </summary>
        /// <param name="chatId">Represents the chat id.</param>
        /// <param name="channelId">Represents the channel id.</param>
        /// <param name="groupId">Represents the group id of the channel.</param>
        /// <param name="tenantId">Represents the tenant id.</param>
        /// <param name="userId">Represents the user id.</param>
        /// <returns>A string unique id.</returns>
        public static string GetTeamsAppInstallationScopeId(string chatId, string channelId, string groupId, string tenantId, string userId)
        {
            try
            {
                TeamsAppInstallationScope teamsAppInstallationScope = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScope(chatId, channelId, groupId, userId);

                if (teamsAppInstallationScope == TeamsAppInstallationScope.TeamScope)
                {
                    return $"{tenantId}${groupId}${channelId}";
                }
                else if (teamsAppInstallationScope == TeamsAppInstallationScope.ChatScope)
                {
                    return $"{tenantId}${chatId}";
                }
                else
                {
                    // Logic for User Scope
                    return $"{tenantId}${userId}";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create container id. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines the installation scope of the application.
        /// </summary>
        /// <param name="chatId">Represents the chat id.</param>
        /// <param name="channelId">Represents the channel id.</param>
        /// <param name="groupId">Represents the group id of the channel.</param>
        /// <param name="userId">Represents the user id.</param>
        /// <returns>An enum representing the installation scope of the app.</returns>
        public static TeamsAppInstallationScope GetTeamsAppInstallationScope(string chatId, string channelId, string groupId, string userId)
        {
            if ((string.IsNullOrEmpty(chatId) || chatId == "undefined") && (!string.IsNullOrEmpty(channelId) && channelId != "undefined" && !string.IsNullOrEmpty(groupId) && groupId != "undefined"))
            {
                return TeamsAppInstallationScope.TeamScope;
            }
            else if ((!string.IsNullOrEmpty(chatId) && chatId != "undefined") && (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(groupId) || channelId == "undefined" || groupId == "undefined"))
            {
                return TeamsAppInstallationScope.ChatScope;
            }
            else if ((string.IsNullOrEmpty(chatId) || chatId == "undefined") && (string.IsNullOrEmpty(channelId) || channelId == "undefined") && (string.IsNullOrEmpty(groupId) || groupId == "undefined") && (!string.IsNullOrEmpty(userId) && userId != "undefined"))
            {
                return TeamsAppInstallationScope.UserScope;
            }
            else
            {
                throw new Exception("Unable to determine teams app installation scope.");
            }
        }
    }
}
