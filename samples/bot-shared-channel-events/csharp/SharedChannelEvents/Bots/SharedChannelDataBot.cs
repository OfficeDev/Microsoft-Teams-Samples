// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedChannelEvents.Models;
using AdaptiveCards;

namespace SharedChannelEvents.Bots
{
    public class SharedChannelDataBot : TeamsActivityHandler
    {
        private readonly ILogger<SharedChannelDataBot> _logger;

        public SharedChannelDataBot(ILogger<SharedChannelDataBot> logger)
        {
            _logger = logger;
        }

        // --- Handle channel share / unshare conversation updates ---
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Always present on Teams activities
            var tcd = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var eventType = tcd?.EventType?.ToLowerInvariant();

            // Read extended shared-channel shape (safe even if fields are absent)
            var extended = turnContext.Activity.GetChannelData<SharedChannelChannelData>();

            // Also keep a raw JObject for logging / future-proof access
            var raw = turnContext.Activity.ChannelData as JObject
                      ?? (turnContext.Activity.ChannelData != null
                          ? JObject.FromObject(turnContext.Activity.ChannelData)
                          : new JObject());

            // Helpful baseline log
            LoggerExtensions.LogInformation(_logger, "ConversationUpdate eventType={EventType}, channelId={ChannelId}, teamId={TeamId}",
                eventType, tcd?.Channel?.Id, tcd?.Team?.Id);

            switch (eventType)
            {
                case "channelshared":
                {
                    var hostTeam = extended?.Team; // The channel's host team
                    var sharedWith = extended?.SharedWithTeams ?? new List<TeamInfoEx>();

                    LoggerExtensions.LogInformation(_logger, "ChannelShared: hostTeam={HostTeamId}, sharedWithCount={Count}",
                        hostTeam?.Id, sharedWith.Count);

                    // Enhanced debugging for team information
                    LoggerExtensions.LogInformation(_logger, "Host team info: Id={Id}, Name={Name}, AadGroupId={AadGroupId}", 
                        hostTeam?.Id, hostTeam?.Name, hostTeam?.AadGroupId);

                    foreach (var team in sharedWith)
                    {
                        LoggerExtensions.LogInformation(_logger, "SharedWithTeam: id={Id}, name={Name}, aadGroupId={AadGroupId}, tenantId={TenantId}",
                            team.Id, team.Name, team.AadGroupId, team.TenantId);
                    }

                    // Try to extract team names from raw JSON if not available in structured data
                    var rawTeamNames = ExtractTeamNamesFromRaw(raw, "sharedWithTeams");
                    LoggerExtensions.LogInformation(_logger, "Raw team names extracted: {TeamNames}", string.Join(", ", rawTeamNames));

                    // Display formatted JSON in console
                    var channelSharedData = new
                    {
                        eventType = "channelShared",
                        channel = new { id = tcd?.Channel?.Id },
                        team = new 
                        { 
                            id = hostTeam?.Id, 
                            aadGroupId = hostTeam?.AadGroupId, 
                            tenantId = hostTeam?.TenantId, 
                            name = hostTeam?.Name 
                        },
                        sharedWithTeams = sharedWith.Select(t => new 
                        { 
                            id = t.Id, 
                            aadGroupId = t.AadGroupId, 
                            tenantId = t.TenantId, 
                            name = t.Name 
                        })
                    };

                    var formattedJson = JsonConvert.SerializeObject(channelSharedData, Formatting.Indented);
                    LoggerExtensions.LogInformation(_logger, "Channel shared\n{ChannelData}", formattedJson);

                    // Get channel information
                    var channelInfo = GetChannelInfo(turnContext);
                    
                    // Create adaptive card for each team the channel is shared with
                    foreach (var team in sharedWith)
                    {
                        string teamName = await ResolveTeamNameAsync(team, turnContext);
                        
                        var channelSharedCard = CreateChannelSharedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            teamName
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(channelSharedCard),
                            cancellationToken);
                    }
                    
                    // If no teams to share with, send a generic message
                    if (sharedWith.Count == 0)
                    {
                        var channelSharedCard = CreateChannelSharedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            "Unknown Team"
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(channelSharedCard),
                            cancellationToken);
                    }
                    break;
                }

                case "channelunshared":
                {
                    var unsharedFrom = extended?.UnsharedFromTeams ?? new List<TeamInfoEx>();

                    LoggerExtensions.LogInformation(_logger, "ChannelUnshared: unsharedFromCount={Count}", unsharedFrom.Count);

                    foreach (var team in unsharedFrom)
                    {
                        LoggerExtensions.LogInformation(_logger, "UnsharedFromTeam: id={Id}, name={Name}, aadGroupId={AadGroupId}, tenantId={TenantId}",
                            team.Id, team.Name, team.AadGroupId, team.TenantId);
                    }

                    // Try to extract team names from raw JSON if not available in structured data
                    var rawTeamNames = ExtractTeamNamesFromRaw(raw, "unsharedFromTeams");
                    LoggerExtensions.LogInformation(_logger, "Raw team names extracted: {TeamNames}", string.Join(", ", rawTeamNames));

                    // Display formatted JSON in console
                    var channelUnsharedData = new
                    {
                        eventType = "channelUnshared",
                        channel = new { id = tcd?.Channel?.Id },
                        team = new 
                        { 
                            id = extended?.Team?.Id, 
                            aadGroupId = extended?.Team?.AadGroupId, 
                            tenantId = extended?.Team?.TenantId, 
                            name = extended?.Team?.Name 
                        },
                        unsharedFromTeams = unsharedFrom.Select(t => new 
                        { 
                            id = t.Id, 
                            aadGroupId = t.AadGroupId, 
                            tenantId = t.TenantId, 
                            name = t.Name 
                        })
                    };

                    var formattedJson = JsonConvert.SerializeObject(channelUnsharedData, Formatting.Indented);
                    LoggerExtensions.LogInformation(_logger, "Channel unshared\n{ChannelData}", formattedJson);

                    // Get channel information
                    var channelInfo = GetChannelInfo(turnContext);
                    
                    // Create adaptive card for each team the channel is unshared from
                    foreach (var team in unsharedFrom)
                    {
                        string teamName = await ResolveTeamNameAsync(team, turnContext);
                        
                        var channelUnsharedCard = CreateChannelUnsharedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            teamName
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(channelUnsharedCard),
                            cancellationToken);
                    }
                    
                    // If no teams to unshare from, send a generic message
                    if (unsharedFrom.Count == 0)
                    {
                        var channelUnsharedCard = CreateChannelUnsharedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            "Unknown Team"
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(channelUnsharedCard),
                            cancellationToken);
                    }
                    break;
                }

                case "channelmemberadded":
                {
                    LoggerExtensions.LogInformation(_logger, "Channel member added event received");

                    // Get channel information
                    var channelInfo = GetChannelInfo(turnContext);
                    
                    // Try to extract member information from the activity
                    var memberNames = new List<string>();
                    
                    // Check if there's member information in MembersAdded
                    if (turnContext.Activity.MembersAdded != null && turnContext.Activity.MembersAdded.Count > 0)
                    {
                        foreach (var member in turnContext.Activity.MembersAdded)
                        {
                            if (member is TeamsChannelAccount teamsAccount)
                            {
                                string memberName = await GetMemberDisplayNameAsync(teamsAccount, turnContext);
                                memberNames.Add(memberName);
                            }
                            else
                            {
                                // Try to extract name from basic member info
                                string memberName = member.Name;
                                if (!string.IsNullOrEmpty(memberName))
                                {
                                    memberNames.Add(memberName);
                                }
                                else if (!string.IsNullOrEmpty(member.Id))
                                {
                                    // Create a user-friendly name from ID
                                    var idParts = member.Id.Split(':');
                                    if (idParts.Length > 1)
                                    {
                                        memberNames.Add($"User ({idParts[idParts.Length - 1].Substring(0, Math.Min(8, idParts[idParts.Length - 1].Length))})");
                                    }
                                    else
                                    {
                                        memberNames.Add($"User ({member.Id.Substring(0, Math.Min(8, member.Id.Length))})");
                                    }
                                }
                            }
                        }
                    }
                    
                    // Try to extract member names from raw channel data as fallback
                    if (memberNames.Count == 0)
                    {
                        try
                        {
                            var rawMembers = ExtractMemberNamesFromRaw(raw, "membersAdded");
                            memberNames.AddRange(rawMembers);
                        }
                        catch (Exception ex)
                        {
                            LoggerExtensions.LogWarning(_logger, ex, "Failed to extract member names from raw data");
                        }
                    }
                    
                    // Display formatted JSON in console
                    var memberAddedData = new
                    {
                        eventType = "channelMemberAdded",
                        channel = new { id = tcd?.Channel?.Id, name = extended?.Channel?.Name, type = extended?.Channel?.Type },
                        team = new { id = extended?.Team?.Id },
                        members = memberNames
                    };

                    var formattedJson = JsonConvert.SerializeObject(memberAddedData, Formatting.Indented);
                    LoggerExtensions.LogInformation(_logger, "Channel member added\n{ChannelData}", formattedJson);

                    // Create adaptive card for each member added or a summary card
                    if (memberNames.Count > 0)
                    {
                        foreach (string memberName in memberNames)
                        {
                            var memberAddedCard = CreateTeamMemberAddedAdaptiveCard(
                                channelInfo.ChannelName,
                                channelInfo.ChannelType,
                                "Channel Member Added",
                                memberName
                            );

                            await turnContext.SendActivityAsync(
                                MessageFactory.Attachment(memberAddedCard),
                                cancellationToken);
                        }
                    }
                    else
                    {
                        // Fallback when no specific member names are available
                        var memberAddedCard = CreateTeamMemberAddedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            "Channel Member Added",
                            "A member has been added"
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(memberAddedCard),
                            cancellationToken);
                    }
                    
                    // Return early to avoid calling base method which causes permission errors
                    return;
                }

                case "channelmemberremoved":
                {
                    LoggerExtensions.LogInformation(_logger, "Channel member removed event received");

                    // Get channel information
                    var channelInfo = GetChannelInfo(turnContext);
                    
                    // Try to extract member information from the activity
                    var memberNames = new List<string>();
                    
                    // Check if there's member information in MembersRemoved
                    if (turnContext.Activity.MembersRemoved != null && turnContext.Activity.MembersRemoved.Count > 0)
                    {
                        foreach (var member in turnContext.Activity.MembersRemoved)
                        {
                            if (member is TeamsChannelAccount teamsAccount)
                            {
                                string memberName = await GetMemberDisplayNameAsync(teamsAccount, turnContext);
                                memberNames.Add(memberName);
                            }
                            else
                            {
                                // Try to extract name from basic member info
                                string memberName = member.Name;
                                if (!string.IsNullOrEmpty(memberName))
                                {
                                    memberNames.Add(memberName);
                                }
                                else if (!string.IsNullOrEmpty(member.Id))
                                {
                                    // Create a user-friendly name from ID
                                    var idParts = member.Id.Split(':');
                                    if (idParts.Length > 1)
                                    {
                                        memberNames.Add($"User ({idParts[idParts.Length - 1].Substring(0, Math.Min(8, idParts[idParts.Length - 1].Length))})");
                                    }
                                    else
                                    {
                                        memberNames.Add($"User ({member.Id.Substring(0, Math.Min(8, member.Id.Length))})");
                                    }
                                }
                            }
                        }
                    }
                    
                    // Try to extract member names from raw channel data as fallback
                    if (memberNames.Count == 0)
                    {
                        try
                        {
                            var rawMembers = ExtractMemberNamesFromRaw(raw, "membersRemoved");
                            memberNames.AddRange(rawMembers);
                        }
                        catch (Exception ex)
                        {
                            LoggerExtensions.LogWarning(_logger, ex, "Failed to extract member names from raw data");
                        }
                    }
                    
                    // Display formatted JSON in console
                    var memberRemovedData = new
                    {
                        eventType = "channelMemberRemoved",
                        channel = new { id = tcd?.Channel?.Id, name = extended?.Channel?.Name, type = extended?.Channel?.Type },
                        team = new { id = extended?.Team?.Id },
                        members = memberNames
                    };

                    var formattedJson = JsonConvert.SerializeObject(memberRemovedData, Formatting.Indented);
                    LoggerExtensions.LogInformation(_logger, "Channel member removed\n{ChannelData}", formattedJson);

                    // Create adaptive card for each member removed or a summary card
                    if (memberNames.Count > 0)
                    {
                        foreach (string memberName in memberNames)
                        {
                            var memberRemovedCard = CreateTeamMemberRemovedAdaptiveCard(
                                channelInfo.ChannelName,
                                channelInfo.ChannelType,
                                "Channel Member Removed",
                                memberName
                            );

                            await turnContext.SendActivityAsync(
                                MessageFactory.Attachment(memberRemovedCard),
                                cancellationToken);
                        }
                    }
                    else
                    {
                        // Fallback when no specific member names are available
                        var memberRemovedCard = CreateTeamMemberRemovedAdaptiveCard(
                            channelInfo.ChannelName,
                            channelInfo.ChannelType,
                            "Channel Member Removed",
                            "A member has been removed"
                        );

                        await turnContext.SendActivityAsync(
                            MessageFactory.Attachment(memberRemovedCard),
                            cancellationToken);
                    }
                    
                    // Return early to avoid calling base method which causes permission errors
                    return;
                }

                default:
                    // No-op; continue normal routing
                    break;
            }

            // (Optional) dump raw channelData for inspection when developing
            var rawJson = raw.ToString(Formatting.Indented);
            LoggerExtensions.LogInformation(_logger, "channelData (raw): {Json}", rawJson);

            // Additional debugging for team information structure
            if (extended != null)
            {
                LoggerExtensions.LogInformation(_logger, "Extended channel data structure:");
                LoggerExtensions.LogInformation(_logger, "- EventType: {EventType}", extended.EventType);
                LoggerExtensions.LogInformation(_logger, "- Channel: {Channel}", extended.Channel != null ? JsonConvert.SerializeObject(extended.Channel) : "null");
                LoggerExtensions.LogInformation(_logger, "- Team: {Team}", extended.Team != null ? JsonConvert.SerializeObject(extended.Team) : "null");
                LoggerExtensions.LogInformation(_logger, "- SharedWithTeams count: {Count}", extended.SharedWithTeams?.Count ?? 0);
                LoggerExtensions.LogInformation(_logger, "- UnsharedFromTeams count: {Count}", extended.UnsharedFromTeams?.Count ?? 0);
                
                if (extended.SharedWithTeams != null)
                {
                    for (int i = 0; i < extended.SharedWithTeams.Count; i++)
                    {
                        var team = extended.SharedWithTeams[i];
                        LoggerExtensions.LogInformation(_logger, "- SharedWithTeams[{Index}]: Id={Id}, Name='{Name}', AadGroupId={AadGroupId}", 
                            i, team.Id, team.Name, team.AadGroupId);
                    }
                }
                
                if (extended.UnsharedFromTeams != null)
                {
                    for (int i = 0; i < extended.UnsharedFromTeams.Count; i++)
                    {
                        var team = extended.UnsharedFromTeams[i];
                        LoggerExtensions.LogInformation(_logger, "- UnsharedFromTeams[{Index}]: Id={Id}, Name='{Name}', AadGroupId={AadGroupId}", 
                            i, team.Id, team.Name, team.AadGroupId);
                    }
                }
            }

            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        // --- Membership add/remove: where did this member come from in a shared channel? ---
        protected override async Task OnTeamsMembersAddedAsync(
            IList<TeamsChannelAccount> membersAdded,
            TeamInfo teamInfo,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var extended = turnContext.Activity.GetChannelData<SharedChannelChannelData>();
            var source = extended?.MembershipSource
                        ?? (turnContext.Activity.ChannelData as JObject)?["membershipSource"]?.ToObject<MembershipSourceEx>();

            // Get channel information
            var channelInfo = GetChannelInfo(turnContext);

            foreach (var member in membersAdded)
            {
                // Enhanced member name resolution using async method
                string memberName = await GetMemberDisplayNameAsync(member, turnContext);

                // Create and send adaptive card for member added
                var memberAddedCard = CreateTeamMemberAddedAdaptiveCard(
                    channelInfo.ChannelName,
                    channelInfo.ChannelType,
                    "Member Added",
                    memberName
                );

                await turnContext.SendActivityAsync(
                    MessageFactory.Attachment(memberAddedCard),
                    cancellationToken);

                LoggerExtensions.LogInformation(_logger, "Team member added: {MemberName} (ID: {MemberId}) to channel {ChannelName} ({ChannelType})",
                    memberName, member.Id, channelInfo.ChannelName, channelInfo.ChannelType);
            }

            if (source != null)
            {
                LoggerExtensions.LogInformation(_logger, "MemberAdded via {SourceType} ({MembershipType}). SourceId={Id}, TeamGroupId={TeamGroupId}, TenantId={TenantId}",
                    source.SourceType, source.MembershipType, source.Id, source.TeamGroupId, source.TenantId);

                // Display formatted JSON in console for member added
                var memberAddedData = new
                {
                    eventType = "teamMemberAdded",
                    membershipSource = new
                    {
                        sourceType = source.SourceType,
                        id = source.Id,
                        membershipType = source.MembershipType,
                        teamGroupId = source.TeamGroupId,
                        tenantId = source.TenantId
                    }
                };

                var formattedJson = JsonConvert.SerializeObject(memberAddedData, Formatting.Indented);
                LoggerExtensions.LogInformation(_logger, "Member added in shared channel ({MembershipType} via {SourceType})\n{MemberData}", 
                    source.MembershipType, source.SourceType, formattedJson);
            }

            await base.OnTeamsMembersAddedAsync(membersAdded, teamInfo, turnContext, cancellationToken);
        }

        protected override async Task OnTeamsMembersRemovedAsync(
            IList<TeamsChannelAccount> membersRemoved,
            TeamInfo teamInfo,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var source = turnContext.Activity.GetChannelData<SharedChannelChannelData>()?.MembershipSource
                        ?? (turnContext.Activity.ChannelData as JObject)?["membershipSource"]?.ToObject<MembershipSourceEx>();

            // Get channel information
            var channelInfo = GetChannelInfo(turnContext);

            foreach (var member in membersRemoved)
            {
                // Enhanced member name resolution using async method
                string memberName = await GetMemberDisplayNameAsync(member, turnContext);
                
                // Create and send adaptive card for member removed
                var memberRemovedCard = CreateTeamMemberRemovedAdaptiveCard(
                    channelInfo.ChannelName,
                    channelInfo.ChannelType,
                    "Member Removed",
                    memberName
                );

                await turnContext.SendActivityAsync(
                    MessageFactory.Attachment(memberRemovedCard),
                    cancellationToken);

                LoggerExtensions.LogInformation(_logger, "Team member removed: {MemberName} (ID: {MemberId}) from channel {ChannelName} ({ChannelType})",
                    memberName, member.Id, channelInfo.ChannelName, channelInfo.ChannelType);
            }

            if (source != null)
            {
                LoggerExtensions.LogInformation(_logger, "MemberRemoved via {SourceType} ({MembershipType}). SourceId={Id}, TeamGroupId={TeamGroupId}, TenantId={TenantId}",
                    source.SourceType, source.MembershipType, source.Id, source.TeamGroupId, source.TenantId);

                // Display formatted JSON in console for member removed
                var memberRemovedData = new
                {
                    eventType = "teamMemberRemoved",
                    membershipSource = new
                    {
                        sourceType = source.SourceType,
                        id = source.Id,
                        membershipType = source.MembershipType,
                        teamGroupId = source.TeamGroupId,
                        tenantId = source.TenantId
                    }
                };

                var formattedJson = JsonConvert.SerializeObject(memberRemovedData, Formatting.Indented);
                LoggerExtensions.LogInformation(_logger, "Member removed from shared channel ({MembershipType} via {SourceType})\n{MemberData}", 
                    source.MembershipType, source.SourceType, formattedJson);
            }

            await base.OnTeamsMembersRemovedAsync(membersRemoved, teamInfo, turnContext, cancellationToken);
        }

        // --- Handle Teams channel creation and deletion events ---
        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            LoggerExtensions.LogInformation(_logger, "Channel created: {ChannelName} (ID: {ChannelId}) in team {TeamName} (ID: {TeamId})", 
                channelInfo.Name, channelInfo.Id, teamInfo?.Name, teamInfo?.Id);

            // Determine channel type based on channelInfo properties
            string channelType = GetChannelTypeFromChannelInfo(channelInfo);
            
            // Create and send adaptive card for channel created
            var channelCreatedCard = CreateChannelCreatedAdaptiveCard(
                channelInfo.Name ?? "Unknown Channel",
                channelType,
                teamInfo?.Name ?? "Unknown Team"
            );

            await turnContext.SendActivityAsync(
                MessageFactory.Attachment(channelCreatedCard),
                cancellationToken);

            await base.OnTeamsChannelCreatedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
        }

        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            LoggerExtensions.LogInformation(_logger, "Channel deleted: {ChannelName} (ID: {ChannelId}) from team {TeamName} (ID: {TeamId})", 
                channelInfo.Name, channelInfo.Id, teamInfo?.Name, teamInfo?.Id);

            // Determine channel type based on channelInfo properties
            string channelType = GetChannelTypeFromChannelInfo(channelInfo);

            // Create and send adaptive card for channel deleted
            var channelDeletedCard = CreateChannelDeletedAdaptiveCard(
                channelInfo.Name ?? "Unknown Channel",
                channelType,
                teamInfo?.Name ?? "Unknown Team"
            );

            await turnContext.SendActivityAsync(
                MessageFactory.Attachment(channelDeletedCard),
                cancellationToken);

            await base.OnTeamsChannelDeletedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
        }

        /// <summary>
        /// Determines the channel type from ChannelInfo properties
        /// </summary>
        private string GetChannelTypeFromChannelInfo(ChannelInfo channelInfo)
        {
            if (channelInfo == null)
                return "Unknown";

            try
            {
                // Try to determine channel type based on available information
                // This is a best-effort approach using available properties
                
                // Check if it's a private channel
                // Private channels in Teams typically have different naming patterns or properties
                if (!string.IsNullOrEmpty(channelInfo.Name))
                {
                    // Private channels often have specific indicators
                    // This is a simplified approach - you might need to enhance based on your Teams setup
                    
                    // For now, we'll use a heuristic approach
                    // You can enhance this by checking additional properties or making API calls
                    
                    // Default logic for demonstration
                    return "Standard"; // Most channels are standard by default
                }

                return "Standard";
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to determine channel type for channel: {ChannelName}", channelInfo?.Name);
                return "Unknown";
            }
        }

        /// <summary>
        /// Extracts team names from raw JSON when structured data doesn't contain names
        /// </summary>
        private List<string> ExtractTeamNamesFromRaw(JObject raw, string arrayPropertyName)
        {
            var teamNames = new List<string>();
            
            try
            {
                var teamsArray = raw[arrayPropertyName] as JArray;
                if (teamsArray != null)
                {
                    foreach (var teamToken in teamsArray)
                    {
                        if (teamToken is JObject teamObj)
                        {
                            // Try different possible property names for team name
                            string teamName = teamObj["name"]?.ToString() 
                                           ?? teamObj["displayName"]?.ToString()
                                           ?? teamObj["teamName"]?.ToString()
                                           ?? teamObj["Name"]?.ToString()
                                           ?? teamObj["DisplayName"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(teamName))
                            {
                                teamNames.Add(teamName);
                            }
                            else
                            {
                                // If no name, try to create a meaningful identifier
                                string id = teamObj["id"]?.ToString() ?? teamObj["Id"]?.ToString();
                                if (!string.IsNullOrEmpty(id))
                                {
                                    // Use consistent formatting with ResolveTeamNameAsync
                                    if (id.Contains(":"))
                                    {
                                        var parts = id.Split(':');
                                        if (parts.Length > 1)
                                        {
                                            var lastPart = parts.Last();
                                            if (parts[0] == "19" && lastPart.Length >= 6)
                                            {
                                                teamNames.Add($"Team-{lastPart.Substring(0, Math.Min(6, lastPart.Length))}");
                                            }
                                            else
                                            {
                                                teamNames.Add($"Team-{lastPart.Substring(0, Math.Min(8, lastPart.Length))}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        teamNames.Add($"Team-{id.Substring(0, Math.Min(8, id.Length))}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to extract team names from raw JSON");
            }
            
            return teamNames;
        }

        /// <summary>
        /// Extracts member names from raw JSON when structured data doesn't contain names
        /// </summary>
        private List<string> ExtractMemberNamesFromRaw(JObject raw, string arrayPropertyName)
        {
            var memberNames = new List<string>();
            
            try
            {
                var membersArray = raw[arrayPropertyName] as JArray;
                if (membersArray != null)
                {
                    foreach (var memberToken in membersArray)
                    {
                        if (memberToken is JObject memberObj)
                        {
                            // Try different possible property names for member name
                            string memberName = memberObj["name"]?.ToString() 
                                             ?? memberObj["displayName"]?.ToString()
                                             ?? memberObj["givenName"]?.ToString()
                                             ?? memberObj["Name"]?.ToString()
                                             ?? memberObj["DisplayName"]?.ToString()
                                             ?? memberObj["GivenName"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(memberName))
                            {
                                memberNames.Add(memberName);
                            }
                            else
                            {
                                // Try to construct name from firstName and lastName
                                string firstName = memberObj["givenName"]?.ToString() ?? memberObj["GivenName"]?.ToString();
                                string lastName = memberObj["surname"]?.ToString() ?? memberObj["Surname"]?.ToString();
                                
                                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                                {
                                    memberNames.Add($"{firstName} {lastName}");
                                }
                                else if (!string.IsNullOrEmpty(firstName))
                                {
                                    memberNames.Add(firstName);
                                }
                                else
                                {
                                    // Try to extract from email or userPrincipalName
                                    string email = memberObj["email"]?.ToString() 
                                                ?? memberObj["userPrincipalName"]?.ToString()
                                                ?? memberObj["Email"]?.ToString() 
                                                ?? memberObj["UserPrincipalName"]?.ToString();
                                    
                                    if (!string.IsNullOrEmpty(email))
                                    {
                                        var emailParts = email.Split('@');
                                        if (emailParts.Length > 0)
                                        {
                                            var nameFromEmail = emailParts[0].Replace(".", " ");
                                            // Make it more presentable
                                            var words = nameFromEmail.Split(' ');
                                            var capitalizedName = string.Join(" ", words.Select(word => 
                                                string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                                            memberNames.Add(capitalizedName);
                                        }
                                    }
                                    else
                                    {
                                        // Final fallback - use part of member ID if available
                                        string id = memberObj["id"]?.ToString() ?? memberObj["Id"]?.ToString();
                                        if (!string.IsNullOrEmpty(id))
                                        {
                                            var idParts = id.Split(':');
                                            if (idParts.Length > 1)
                                            {
                                                var lastPart = idParts[idParts.Length - 1];
                                                memberNames.Add($"User ({lastPart.Substring(0, Math.Min(8, lastPart.Length))})");
                                            }
                                            else
                                            {
                                                memberNames.Add($"User ({id.Substring(0, Math.Min(8, id.Length))})");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to extract member names from raw JSON");
            }
            
            return memberNames;
        }

        /// <summary>
        /// Creates a formatted message for team sharing/unsharing events
        /// </summary>
        private async Task<string> CreateTeamMessageAsync(List<TeamInfoEx> teams, List<string> rawTeamNames, string action, string indicator, ITurnContext turnContext)
        {
            var teamNames = new List<string>();
            
            // First, try to get names from structured data
            foreach (var team in teams)
            {
                string resolvedName = await ResolveTeamNameAsync(team, turnContext);
                teamNames.Add(resolvedName);
            }
            
            // If no meaningful names from structured data, use raw extracted names
            if (teamNames.All(name => name.StartsWith("Team-") || name.StartsWith("Team (") || name == "Unknown Team") && rawTeamNames.Count > 0)
            {
                // Only use raw names if they seem more meaningful than our generated ones
                var meaningfulRawNames = rawTeamNames.Where(name => 
                    !string.IsNullOrEmpty(name) && 
                    !name.StartsWith("Team-") && 
                    !name.StartsWith("Team (") && 
                    name != "Unknown Team").ToList();
                
                if (meaningfulRawNames.Count > 0)
                {
                    teamNames = meaningfulRawNames;
                }
            }

            // Create the message
            if (teamNames.Count == 0)
            {
                return $"{indicator} Channel {action} team(s).";
            }
            else if (teamNames.Count == 1)
            {
                return $"{indicator} Channel {action} {teamNames[0]}.";
            }
            else if (teamNames.Count <= 3)
            {
                return $"{indicator} Channel {action} {string.Join(", ", teamNames)}.";
            }
            else
            {
                var firstThree = string.Join(", ", teamNames.Take(3));
                return $"{indicator} Channel {action} {firstThree} and {teamNames.Count - 3} more team(s).";
            }
        }

        /// <summary>
        /// Resolves team name using multiple strategies including Teams API calls
        /// </summary>
        private async Task<string> ResolveTeamNameAsync(TeamInfoEx team, ITurnContext turnContext)
        {
            // Strategy 1: Use the name if already available
            if (!string.IsNullOrEmpty(team.Name))
            {
                return team.Name;
            }

            // Strategy 2: Try to get team details using Teams API
            try
            {
                if (!string.IsNullOrEmpty(team.Id))
                {
                    var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, team.Id);
                    if (teamDetails != null && !string.IsNullOrEmpty(teamDetails.Name))
                    {
                        LoggerExtensions.LogInformation(_logger, "Retrieved team name via Teams API: {TeamName} for ID {TeamId}", 
                            teamDetails.Name, team.Id);
                        return teamDetails.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to get team details via Teams API for team ID: {TeamId}", team.Id);
            }

            // Strategy 3: Try using AAD Group ID if available
            try
            {
                if (!string.IsNullOrEmpty(team.AadGroupId))
                {
                    // Note: This would require Microsoft Graph API access
                    // For now, we'll create a placeholder that could be extended
                    LoggerExtensions.LogInformation(_logger, "Could fetch team name using AAD Group ID: {AadGroupId}", team.AadGroupId);
                }
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to resolve team name via AAD Group ID: {AadGroupId}", team.AadGroupId);
            }

            // Strategy 4: Create a user-friendly identifier
            if (!string.IsNullOrEmpty(team.Id))
            {
                // Check if it's a Teams ID format and extract meaningful part
                if (team.Id.Contains(":"))
                {
                    var parts = team.Id.Split(':');
                    if (parts.Length > 1)
                    {
                        var lastPart = parts.Last();
                        
                        // For Teams conversation IDs, show a more meaningful format
                        if (parts[0] == "19" && lastPart.Length >= 6)
                        {
                            // This looks like a Teams conversation/channel ID
                            return $"Team-{lastPart.Substring(0, Math.Min(6, lastPart.Length))}";
                        }
                        else if (lastPart.Length > 8)
                        {
                            return $"Team-{lastPart.Substring(0, 8)}";
                        }
                        else
                        {
                            return $"Team-{lastPart}";
                        }
                    }
                }
                else
                {
                    // Regular ID - use first 8 characters
                    var shortId = team.Id.Substring(0, Math.Min(8, team.Id.Length));
                    return $"Team-{shortId}";
                }
            }

            // Final fallback
            return "Unknown Team";
        }

        /// <summary>
        /// Enhanced method to get member display name with multiple fallback options
        /// </summary>
        private async Task<string> GetMemberDisplayNameAsync(TeamsChannelAccount member, ITurnContext turnContext)
        {
            // Strategy 1: Try multiple name properties in order of preference
            if (!string.IsNullOrEmpty(member.Name))
                return member.Name;

            if (!string.IsNullOrEmpty(member.GivenName) && !string.IsNullOrEmpty(member.Surname))
                return $"{member.GivenName} {member.Surname}";

            if (!string.IsNullOrEmpty(member.GivenName))
                return member.GivenName;

            // Strategy 2: Try to get member details using Teams API
            try
            {
                if (!string.IsNullOrEmpty(member.Id))
                {
                    // Try to get team member details if we have team context
                    var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id;
                    if (!string.IsNullOrEmpty(teamId))
                    {
                        var teamMember = await TeamsInfo.GetTeamMemberAsync(turnContext, teamId, member.Id, cancellationToken: default);
                        if (teamMember != null)
                        {
                            if (!string.IsNullOrEmpty(teamMember.Name))
                            {
                                LoggerExtensions.LogInformation(_logger, "Retrieved member name via Teams API: {MemberName} for ID {MemberId}", 
                                    teamMember.Name, member.Id);
                                return teamMember.Name;
                            }
                            
                            if (!string.IsNullOrEmpty(teamMember.GivenName) && !string.IsNullOrEmpty(teamMember.Surname))
                            {
                                var fullName = $"{teamMember.GivenName} {teamMember.Surname}";
                                LoggerExtensions.LogInformation(_logger, "Retrieved member name via Teams API: {MemberName} for ID {MemberId}", 
                                    fullName, member.Id);
                                return fullName;
                            }
                            
                            if (!string.IsNullOrEmpty(teamMember.GivenName))
                            {
                                LoggerExtensions.LogInformation(_logger, "Retrieved member given name via Teams API: {MemberName} for ID {MemberId}", 
                                    teamMember.GivenName, member.Id);
                                return teamMember.GivenName;
                            }
                            
                            // Try UserPrincipalName from team member if available
                            if (!string.IsNullOrEmpty(teamMember.UserPrincipalName))
                            {
                                var emailParts = teamMember.UserPrincipalName.Split('@');
                                if (emailParts.Length > 0)
                                {
                                    var nameFromEmail = emailParts[0].Replace(".", " ");
                                    LoggerExtensions.LogInformation(_logger, "Extracted member name from UPN via Teams API: {MemberName} for ID {MemberId}", 
                                        nameFromEmail, member.Id);
                                    return nameFromEmail;
                                }
                            }
                        }
                    }

                    // Try to get general member info if team context is not available
                    var memberInfo = await TeamsInfo.GetMemberAsync(turnContext, member.Id, cancellationToken: default);
                    if (memberInfo != null)
                    {
                        if (!string.IsNullOrEmpty(memberInfo.Name))
                        {
                            LoggerExtensions.LogInformation(_logger, "Retrieved member name via Teams GetMember API: {MemberName} for ID {MemberId}", 
                                memberInfo.Name, member.Id);
                            return memberInfo.Name;
                        }
                        
                        if (!string.IsNullOrEmpty(memberInfo.GivenName) && !string.IsNullOrEmpty(memberInfo.Surname))
                        {
                            var fullName = $"{memberInfo.GivenName} {memberInfo.Surname}";
                            LoggerExtensions.LogInformation(_logger, "Retrieved member name via Teams GetMember API: {MemberName} for ID {MemberId}", 
                                fullName, member.Id);
                            return fullName;
                        }
                        
                        if (!string.IsNullOrEmpty(memberInfo.GivenName))
                        {
                            LoggerExtensions.LogInformation(_logger, "Retrieved member given name via Teams GetMember API: {MemberName} for ID {MemberId}", 
                                memberInfo.GivenName, member.Id);
                            return memberInfo.GivenName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to get member details via Teams API for member ID: {MemberId}", member.Id);
            }

            // Strategy 3: Try UserPrincipalName from original member
            if (!string.IsNullOrEmpty(member.UserPrincipalName))
            {
                // Extract name from email address
                var emailParts = member.UserPrincipalName.Split('@');
                if (emailParts.Length > 0)
                {
                    var nameFromEmail = emailParts[0].Replace(".", " ");
                    // Make it more presentable
                    var words = nameFromEmail.Split(' ');
                    var capitalizedName = string.Join(" ", words.Select(word => 
                        string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    return capitalizedName;
                }
            }

            // Strategy 4: Try Email from original member
            if (!string.IsNullOrEmpty(member.Email))
            {
                // Extract name from email address
                var emailParts = member.Email.Split('@');
                if (emailParts.Length > 0)
                {
                    var nameFromEmail = emailParts[0].Replace(".", " ");
                    // Make it more presentable
                    var words = nameFromEmail.Split(' ');
                    var capitalizedName = string.Join(" ", words.Select(word => 
                        string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    return capitalizedName;
                }
            }

            // Strategy 5: Try to extract from AAD Object ID if available
            if (!string.IsNullOrEmpty(member.AadObjectId))
            {
                // Note: This could be enhanced to use Microsoft Graph API to get user details
                LoggerExtensions.LogInformation(_logger, "Member has AAD Object ID: {AadObjectId}, could fetch user details via Graph API", member.AadObjectId);
                // For now, return a more user-friendly identifier
                return $"User (AAD: {member.AadObjectId.Substring(0, Math.Min(8, member.AadObjectId.Length))})";
            }

            // Final fallback - use part of member ID if available, but make it more user-friendly
            if (!string.IsNullOrEmpty(member.Id))
            {
                // Extract meaningful part from member ID
                var idParts = member.Id.Split(':');
                if (idParts.Length > 1)
                {
                    var lastPart = idParts[idParts.Length - 1];
                    return $"User ({lastPart.Substring(0, Math.Min(8, lastPart.Length))})";
                }
                else
                {
                    return $"User ({member.Id.Substring(0, Math.Min(8, member.Id.Length))})";
                }
            }

            return "Unknown Member";
        }

        /// <summary>
        /// Synchronous version for backwards compatibility
        /// </summary>
        private string GetMemberDisplayName(TeamsChannelAccount member)
        {
            // Try multiple name properties in order of preference
            if (!string.IsNullOrEmpty(member.Name))
                return member.Name;

            if (!string.IsNullOrEmpty(member.GivenName) && !string.IsNullOrEmpty(member.Surname))
                return $"{member.GivenName} {member.Surname}";

            if (!string.IsNullOrEmpty(member.GivenName))
                return member.GivenName;

            if (!string.IsNullOrEmpty(member.UserPrincipalName))
            {
                // Extract name from email address
                var emailParts = member.UserPrincipalName.Split('@');
                if (emailParts.Length > 0)
                {
                    var nameFromEmail = emailParts[0].Replace(".", " ");
                    // Make it more presentable
                    var words = nameFromEmail.Split(' ');
                    var capitalizedName = string.Join(" ", words.Select(word => 
                        string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    return capitalizedName;
                }
            }

            if (!string.IsNullOrEmpty(member.Email))
            {
                // Extract name from email address
                var emailParts = member.Email.Split('@');
                if (emailParts.Length > 0)
                {
                    var nameFromEmail = emailParts[0].Replace(".", " ");
                    // Make it more presentable
                    var words = nameFromEmail.Split(' ');
                    var capitalizedName = string.Join(" ", words.Select(word => 
                        string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    return capitalizedName;
                }
            }

            // Final fallback - use part of member ID if available
            if (!string.IsNullOrEmpty(member.Id))
            {
                // Extract meaningful part from member ID
                var idParts = member.Id.Split(':');
                if (idParts.Length > 1)
                {
                    var lastPart = idParts[idParts.Length - 1];
                    return $"User ({lastPart.Substring(0, Math.Min(8, lastPart.Length))})";
                }
            }

            return "Unknown Member";
        }

        /// <summary>
        /// Gets channel information including name and type
        /// </summary>
        private (string ChannelName, string ChannelType) GetChannelInfo(ITurnContext turnContext)
        {
            try
            {
                // Get Teams channel data
                var tcd = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var extended = turnContext.Activity.GetChannelData<SharedChannelChannelData>();
                
                // Determine channel name
                string channelName = "Unknown Channel";
                
                // Try to get channel name from various sources
                if (!string.IsNullOrEmpty(extended?.Channel?.Name))
                {
                    channelName = extended.Channel.Name;
                }
                else if (!string.IsNullOrEmpty(tcd?.Channel?.Name))
                {
                    channelName = tcd.Channel.Name;
                }
                else
                {
                    // Extract channel name from conversation ID if possible
                    var conversationId = turnContext.Activity.Conversation?.Id;
                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        // Teams conversation IDs often contain channel information
                        if (conversationId.Contains("@thread"))
                        {
                            channelName = "Teams Channel";
                        }
                        else
                        {
                            channelName = "Teams Conversation";
                        }
                    }
                }

                // Determine channel type
                string channelType = "Standard";
                
                // Check if it's a shared channel
                if (extended?.SharedWithTeams?.Count > 0 || extended?.UnsharedFromTeams?.Count > 0)
                {
                    channelType = "Shared";
                }
                else if (!string.IsNullOrEmpty(extended?.EventType))
                {
                    // If we have shared channel data structure, it's likely a shared channel
                    channelType = "Shared";
                }
                else
                {
                    // Check conversation type
                    var conversationType = turnContext.Activity.Conversation?.ConversationType;
                    if (conversationType == "channel")
                    {
                        channelType = "Standard";
                    }
                    else if (conversationType == "groupChat")
                    {
                        channelType = "Group Chat";
                    }
                }

                return (channelName, channelType);
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogWarning(_logger, ex, "Failed to get channel info");
                return ("Unknown Channel", "Unknown");
            }
        }

        /// <summary>
        /// Creates an adaptive card for team member added event with green styling
        /// </summary>
        private Attachment CreateTeamMemberAddedAdaptiveCard(string channelName, string channelType, string eventType, string memberName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Good,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "+",
                                                Size = AdaptiveTextSize.Large,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Team Member Added",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Medium,
                                                Color = AdaptiveTextColor.Good
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event Type:",
                                        Value = eventType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Member:",
                                        Value = $"{memberName} has been added"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        /// <summary>
        /// Creates an adaptive card for team member removed event with red styling
        /// </summary>
        private Attachment CreateTeamMemberRemovedAdaptiveCard(string channelName, string channelType, string eventType, string memberName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Attention,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "-",
                                                Size = AdaptiveTextSize.Large,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Team Member Removed",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Medium,
                                                Color = AdaptiveTextColor.Attention
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event Type:",
                                        Value = eventType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Member:",
                                        Value = $"{memberName} has been removed"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        /// <summary>
        /// Creates an adaptive card for channel shared event with blue/info styling
        /// </summary>
        private Attachment CreateChannelSharedAdaptiveCard(string channelName, string channelType, string teamName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Accent,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Size = AdaptiveTextSize.Large,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Channel Shared With Team",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Large,
                                                Color = AdaptiveTextColor.Dark
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Team Name:",
                                        Value = teamName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event:",
                                        Value = "Channel has been shared with this team"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        /// <summary>
        /// Creates an adaptive card for channel unshared event with warning styling
        /// </summary>
        private Attachment CreateChannelUnsharedAdaptiveCard(string channelName, string channelType, string teamName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Warning,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Size = AdaptiveTextSize.Large,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Channel Unshared From Team",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Large,
                                                Color = AdaptiveTextColor.Dark
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Channel Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Team Name:",
                                        Value = teamName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event:",
                                        Value = "Channel sharing has been removed from this team"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        /// <summary>
        /// Creates an adaptive card for channel created event with good/success styling
        /// </summary>
        private Attachment CreateChannelCreatedAdaptiveCard(string channelName, string channelType, string teamName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Good,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "+",
                                                Size = AdaptiveTextSize.ExtraLarge,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Color = AdaptiveTextColor.Dark,
                                                Weight = AdaptiveTextWeight.Bolder
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Channel Created",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Large,
                                                Color = AdaptiveTextColor.Dark
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Team:",
                                        Value = teamName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event:",
                                        Value = "New channel has been created"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        /// <summary>
        /// Creates an adaptive card for channel deleted event with attention/warning styling
        /// </summary>
        private Attachment CreateChannelDeletedAdaptiveCard(string channelName, string channelType, string teamName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = AdaptiveContainerStyle.Attention,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "X",
                                                Size = AdaptiveTextSize.ExtraLarge,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Color = AdaptiveTextColor.Dark,
                                                Weight = AdaptiveTextWeight.Bolder
                                            }
                                        }
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "stretch",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "Channel Deleted",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Large,
                                                Color = AdaptiveTextColor.Dark
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact
                                    {
                                        Title = "Name:",
                                        Value = channelName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Type:",
                                        Value = channelType
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Team:",
                                        Value = teamName
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Event:",
                                        Value = "Channel has been deleted"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Time:",
                                        Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }
                                }
                            }
                        },
                        Spacing = AdaptiveSpacing.Medium
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        // Keep basic echo so you can poke the bot
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"Echo: {turnContext.Activity.Text}"),
                cancellationToken);
        }
    }
}