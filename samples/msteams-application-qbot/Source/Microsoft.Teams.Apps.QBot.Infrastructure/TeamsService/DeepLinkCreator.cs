// <copyright file="DeepLinkCreator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.WebUtilities;
    using Newtonsoft.Json;

    /// <summary>
    /// Creates deeplinks to teams entities.
    /// </summary>
    internal class DeepLinkCreator : IDeepLinkCreator
    {
        /// <inheritdoc/>
        public string GetPersonalTabDeepLink(string manifestAppId, string personalTabId)
        {
            return $"https://teams.microsoft.com/l/entity/{manifestAppId}/{personalTabId}";
        }

        /// <inheritdoc/>
        public string GetTeamsMessageDeepLink(string teamId, string channelId, string parentMessageId, string messageId)
        {
            return $"https://teams.microsoft.com/l/message/{channelId}/{messageId}?parentMessageId={parentMessageId}&groupId={teamId}";
        }

        /// <inheritdoc />
        public string GetPersonalTabConfigureCourseDeepLink(string manifestAppId, string personalTabId, string courseAadId)
        {
            var queryParameters = new Dictionary<string, string>
            {
                {
                    "context",
                    JsonConvert.SerializeObject(
                    new
                    {
                        subEntityId = $"courseConfig:/{courseAadId}",
                    })
                },
            };
            return QueryHelpers.AddQueryString(
                $"{this.GetPersonalTabDeepLink(manifestAppId: manifestAppId, personalTabId: personalTabId)}",
                queryParameters);
        }
    }
}
