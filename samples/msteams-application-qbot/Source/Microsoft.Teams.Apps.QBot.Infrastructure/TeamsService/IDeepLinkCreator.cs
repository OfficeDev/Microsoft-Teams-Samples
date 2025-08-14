// <copyright file="IDeepLinkCreator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    /// <summary>
    /// Creates deep links to entities in Teams.
    /// </summary>
    public interface IDeepLinkCreator
    {
        /// <summary>
        /// Creates a deeplink to a message in a Teams channel.
        /// </summary>
        /// <param name="teamId">Teams id.</param>
        /// <param name="channelId">Channel's id.</param>
        /// <param name="parentMessageId">Parent message id (root message).</param>
        /// <param name="messageId">Child message id</param>
        /// <returns>Deeplink to message.</returns>
        string GetTeamsMessageDeepLink(string teamId, string channelId, string parentMessageId, string messageId);

        /// <summary>
        /// Creates a deeplink to personal tab.
        /// </summary>
        /// <param name="manifestAppId">App id in manifest.json.</param>
        /// <param name="personalTabId">Personal tab id.</param>
        /// <returns>Deeplink to personal tab.</returns>
        string GetPersonalTabDeepLink(string manifestAppId, string personalTabId);

        /// <summary>
        /// Creates a deeplink to personal tab.
        /// </summary>
        /// <param name="manifestAppId">App id in manifest.json.</param>
        /// <param name="personalTabId">Personal tab id.</param>
        /// <param name="courseAadId">The course id to configure.</param>
        /// <returns>Deeplink to personal tab.</returns>
        string GetPersonalTabConfigureCourseDeepLink(string manifestAppId, string personalTabId, string courseAadId);
    }
}
