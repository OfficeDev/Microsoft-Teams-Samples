// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace SharedChannelEvents.Models
{
    /// <summary>
    /// A minimal extension shape that mirrors the new PR fields on channelData.
    /// </summary>
    public class SharedChannelChannelData
    {
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("channel")]
        public ChannelInfo Channel { get; set; }

        // Host team for the channel
        [JsonProperty("team")]
        public TeamInfoEx Team { get; set; }

        // New arrays for shared/unshared teams
        [JsonProperty("sharedWithTeams")]
        public List<TeamInfoEx> SharedWithTeams { get; set; } = new List<TeamInfoEx>();

        [JsonProperty("unsharedFromTeams")]
        public List<TeamInfoEx> UnsharedFromTeams { get; set; } = new List<TeamInfoEx>();

        // New: membership source for add/remove in shared channel
        [JsonProperty("membershipSource")]
        public MembershipSourceEx MembershipSource { get; set; }
    }

    /// <summary>
    /// Extended team information for shared channels.
    /// </summary>
    public class TeamInfoEx
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("aadGroupId")]
        public string AadGroupId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Membership source information for shared channel members.
    /// </summary>
    public class MembershipSourceEx
    {
        [JsonProperty("sourceType")]
        public string SourceType { get; set; }

        // Unique identifier of the membership source (team or channel)
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("membershipType")]
        public string MembershipType { get; set; }

        // AAD group id for the team associated with the membership
        [JsonProperty("teamGroupId")]
        public string TeamGroupId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
    }
}