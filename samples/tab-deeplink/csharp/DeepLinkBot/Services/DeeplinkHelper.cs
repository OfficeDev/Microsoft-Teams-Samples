// <copyright file="DeeplinkHelper.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using System.Text;
using System.Text.Json;
using System.Web;

namespace Microsoft.AgentSamples.Services;

/// <summary>
/// Helper class for generating Teams deep links.
/// </summary>
public class DeeplinkHelper
{
    /// <summary>
    /// Generates a deep link to a tab task.
    /// </summary>
    /// <param name="teamsUrl">Teams deep link URL base.</param>
    /// <param name="appId">Application ID of the app.</param>
    /// <param name="entityId">Entity ID of the tab.</param>
    /// <param name="subEntityId">Sub-entity ID of the tab.</param>
    /// <returns>The deep link URL.</returns>
    public string GetDeepLinkToTabTask(string teamsUrl, string appId, string entityId, string subEntityId)
    {
        var taskValues = new Dictionary<string, string>
        {
            { "subEntityId", subEntityId }
        };
        var jsonContext = JsonSerializer.Serialize(taskValues);
        var taskContext = HttpUtility.UrlEncode(jsonContext);
        var deepLinkUrl = $"{teamsUrl}{appId}/{entityId}?context=";
        return deepLinkUrl + taskContext;
    }

    /// <summary>
    /// Generates a deep link to a meeting side panel.
    /// </summary>
    /// <param name="teamsUrl">Teams deep link URL base.</param>
    /// <param name="appId">Application ID of the app.</param>
    /// <param name="baseUrl">Base URL of the application.</param>
    /// <param name="entityId">Entity ID of the tab.</param>
    /// <param name="chatId">Chat ID of the meeting group chat.</param>
    /// <param name="contextType">Chat context where the app is installed.</param>
    /// <returns>The deep link URL.</returns>
    public string GetDeepLinkToMeetingSidePanel(string teamsUrl, string appId, string baseUrl, string entityId, string chatId, string contextType)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append($"\"chatId\":\"{chatId}\",");
        sb.Append($"\"contextType\":\"{contextType}\"");
        sb.Append('}');

        var jsonContext = sb.ToString();
        var taskContext = HttpUtility.UrlEncode(jsonContext);
        var encodedUrl = HttpUtility.UrlEncode($"{baseUrl}/appInMeeting");
        var deepLinkUrl = $"{teamsUrl}{appId}/{entityId}?webUrl={encodedUrl}&context=";
        return deepLinkUrl + taskContext;
    }

    /// <summary>
    /// Generates a deep link to a channel tab task.
    /// </summary>
    /// <param name="teamsUrl">Teams deep link URL base.</param>
    /// <param name="appId">Application ID of the app.</param>
    /// <param name="baseUrl">Base URL of the application.</param>
    /// <param name="channelId">Channel ID of the Teams channel where the app is installed.</param>
    /// <param name="entityId">Entity ID of the tab.</param>
    /// <param name="subEntityId">Sub-entity ID of the tab.</param>
    /// <returns>The deep link URL.</returns>
    public string GetDeepLinkToChannelTask(string teamsUrl, string appId, string baseUrl, string channelId, string entityId, string subEntityId)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append($"\"subEntityId\":\"{subEntityId}\",");
        sb.Append($"\"channelId\":\"{channelId}\"");
        sb.Append('}');

        var channelContext = sb.ToString();
        var encodedUrl = HttpUtility.UrlEncode($"{baseUrl}/DeepLinkChannel");
        var taskContext = HttpUtility.UrlEncode(channelContext);
        var deepLinkUrl = $"{teamsUrl}{appId}/{entityId}?webUrl={encodedUrl}&label=Topic&context=";
        return deepLinkUrl + taskContext;
    }
}