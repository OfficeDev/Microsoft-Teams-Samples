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

        /// <summary>
        /// Creates the bot instance. Uses <see cref="TeamsActivityHandler"/> to receive Teams-specific activities.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        public SharedChannelDataBot(ILogger<SharedChannelDataBot> logger) => _logger = logger;

        /// <summary>
        /// Entry point for all conversationUpdate activities. Filters for shared channel specific eventTypes
        /// (channelShared / channelUnshared / channelMemberAdded / channelMemberRemoved) and handles them directly.
        /// For member add/remove we bypass the base implementation to avoid 404 lookups in shared channels.
        /// All other updates fall through to the base handler.
        /// </summary>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var tcd = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var extended = turnContext.Activity.GetChannelData<SharedChannelChannelData>();
            var eventType = tcd?.EventType?.ToLowerInvariant();
            var raw = turnContext.Activity.ChannelData as JObject ?? new JObject();
            _logger.LogInformation("ConversationUpdate eventType={EventType} channelId={ChannelId} teamId={TeamId}", eventType, tcd?.Channel?.Id, tcd?.Team?.Id);

            switch (eventType)
            {
                case "channelshared":
                    await HandleChannelSharedAsync(turnContext, extended, cancellationToken);
                    break;
                case "channelunshared":
                    await HandleChannelUnsharedAsync(turnContext, extended, cancellationToken);
                    break;
                case "channelmemberadded":
                    await HandleSharedChannelMemberEventAsync(turnContext, true, extended, raw, cancellationToken);
                    return; // IMPORTANT: skip base to avoid TeamsActivityHandler member lookup NotFound
                case "channelmemberremoved":
                    await HandleSharedChannelMemberEventAsync(turnContext, false, extended, raw, cancellationToken);
                    return; // skip base
                default:
                    break;
            }

            // Debug dump (optional)
            _logger.LogInformation("channelData (raw): {Json}", raw.ToString(Formatting.Indented));
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Handles member add / remove events for shared channels delivered as conversationUpdate with eventType
        /// channelMemberAdded or channelMemberRemoved. Builds either a direct membership card or a transitive
        /// membership card (when membershipSource indicates transitive). Does not call TeamsInfo for safety.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="added">True if member added; false if removed.</param>
        /// <param name="extended">Extended shared channel data from channelData.</param>
        /// <param name="raw">Raw JObject channelData for fallback extraction.</param>
        /// <param name="ct">Cancellation token.</param>
        private async Task HandleSharedChannelMemberEventAsync(ITurnContext<IConversationUpdateActivity> turnContext, bool added, SharedChannelChannelData extended, JObject raw, CancellationToken ct)
        {
            var channelInfo = GetChannelInfo(turnContext);
            var source = extended?.MembershipSource ?? raw["membershipSource"]?.ToObject<MembershipSourceEx>();
            var membershipTypeRaw = source?.MembershipType?.ToString();
            bool isTransitive = string.Equals(membershipTypeRaw, "transitive", StringComparison.OrdinalIgnoreCase) || membershipTypeRaw == "1"; // numeric fallback

            var names = new List<string>();
            var activityMembers = added ? turnContext.Activity.MembersAdded : turnContext.Activity.MembersRemoved;
            if (activityMembers != null)
            {
                foreach (var m in activityMembers)
                {
                    if (m is TeamsChannelAccount tca)
                    {
                        names.Add(!string.IsNullOrEmpty(tca.Name) ? tca.Name : (!string.IsNullOrEmpty(tca.GivenName) ? (string.IsNullOrEmpty(tca.Surname) ? tca.GivenName : tca.GivenName + " " + tca.Surname) : (tca.Id?.Split(':').LastOrDefault()?.Substring(0, 8) ?? "User")));
                    }
                    else if (!string.IsNullOrEmpty(m.Name))
                    {
                        names.Add(m.Name);
                    }
                }
            }
            if (names.Count == 0)
            {
                var rawNames = ExtractMemberNamesFromRaw(raw, added ? "membersAdded" : "membersRemoved");
                names.AddRange(rawNames);
            }
            if (names.Count == 0) names.Add(added ? "A member has been added" : "A member has been removed");

            string sourceTeamName = isTransitive ? await ResolveSourceTeamNameAsync(source, turnContext) : null;

            foreach (var name in names)
            {
                Attachment card = isTransitive
                    ? CreateTransitiveMembershipAdaptiveCard(channelInfo.ChannelName, name, added ? "Added" : "Removed", sourceTeamName, source)
                    : (added
                        ? CreateTeamMemberAddedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, "Channel Member Added", name)
                        : CreateTeamMemberRemovedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, "Channel Member Removed", name));
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card), ct);
            }
        }

        /// <summary>
        /// Handles channel shared events. Emits a card per target team or a placeholder when no team info is present.
        /// </summary>
        private async Task HandleChannelSharedAsync(ITurnContext turnContext, SharedChannelChannelData extended, CancellationToken ct)
        {
            var hostTeam = extended?.Team;
            var sharedWith = extended?.SharedWithTeams ?? new List<TeamInfoEx>();
            _logger.LogInformation("ChannelShared hostTeam={HostTeam} count={Count}", hostTeam?.Id, sharedWith.Count);
            var channelInfo = GetChannelInfo(turnContext);
            if (sharedWith.Count == 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CreateChannelSharedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, "Unknown Team")), ct);
                return;
            }
            foreach (var team in sharedWith)
            {
                var name = await ResolveTeamNameAsync(team, turnContext);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CreateChannelSharedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, name)), ct);
            }
        }

        /// <summary>
        /// Handles channel unshared events. Emits a card per team from which sharing was removed.
        /// </summary>
        private async Task HandleChannelUnsharedAsync(ITurnContext turnContext, SharedChannelChannelData extended, CancellationToken ct)
        {
            var unsharedFrom = extended?.UnsharedFromTeams ?? new List<TeamInfoEx>();
            var channelInfo = GetChannelInfo(turnContext);
            if (unsharedFrom.Count == 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CreateChannelUnsharedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, "Unknown Team")), ct);
                return;
            }
            foreach (var team in unsharedFrom)
            {
                var name = await ResolveTeamNameAsync(team, turnContext);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CreateChannelUnsharedAdaptiveCard(channelInfo.ChannelName, channelInfo.ChannelType, name)), ct);
            }
        }

        /// <summary>
        /// Resolves a human readable source team name for transitive membership events using (a) inline name
        /// if present, (b) TeamsInfo lookup, (c) shortened id or group id fallback.
        /// </summary>
        private async Task<string> ResolveSourceTeamNameAsync(MembershipSourceEx source, ITurnContext turnContext)
        {
            if (source == null) return "Unknown Team";
            var raw = turnContext.Activity.ChannelData as JObject;
            var srcToken = raw?["membershipSource"] as JObject;
            var directName = srcToken?["name"]?.ToString();
            if (!string.IsNullOrEmpty(directName)) return directName;
            if (!string.IsNullOrEmpty(source.Id))
            {
                try
                {
                    var details = await TeamsInfo.GetTeamDetailsAsync(turnContext, source.Id);
                    if (!string.IsNullOrEmpty(details?.Name)) return details.Name;
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(source.Id))
            {
                var part = source.Id.Split(':').Last();
                return part.Length > 8 ? $"Team-{part[..8]}" : $"Team-{part}";
            }
            if (!string.IsNullOrEmpty(source.TeamGroupId))
            {
                var g = source.TeamGroupId;
                return g.Length > 8 ? $"Team-{g[..8]}" : $"Team-{g}";
            }
            return "Unknown Team";
        }

        /// <summary>
        /// Called for standard Teams channel creation. Sends a simple Adaptive Card describing the new channel.
        /// </summary>
        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = CreateChannelCreatedAdaptiveCard(channelInfo.Name ?? "Unknown Channel", GetChannelTypeFromChannelInfo(channelInfo), teamInfo?.Name ?? "Unknown Team");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
            await base.OnTeamsChannelCreatedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
        }

        /// <summary>
        /// Called for standard Teams channel deletion. Sends a simple Adaptive Card describing the removed channel.
        /// </summary>
        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = CreateChannelDeletedAdaptiveCard(channelInfo.Name ?? "Unknown Channel", GetChannelTypeFromChannelInfo(channelInfo), teamInfo?.Name ?? "Unknown Team");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
            await base.OnTeamsChannelDeletedAsync(channelInfo, teamInfo, turnContext, cancellationToken);
        }

        /// <summary>
        /// Minimal channel type derivation (currently returns Standard or Unknown). Placeholder for enhancement.
        /// </summary>
        private string GetChannelTypeFromChannelInfo(ChannelInfo channelInfo)
        {
            if (channelInfo == null) return "Unknown";
            try { return string.IsNullOrEmpty(channelInfo.Name) ? "Standard" : "Standard"; } catch { return "Unknown"; }
        }

        /// <summary>
        /// Extracts member display names from raw channelData arrays (membersAdded / membersRemoved).
        /// </summary>
        private List<string> ExtractMemberNamesFromRaw(JObject raw, string arrayPropertyName)
        {
            var names = new List<string>();
            if (raw[arrayPropertyName] is not JArray arr) return names;
            foreach (var memberToken in arr.OfType<JObject>())
            {
                var name = memberToken["name"]?.ToString() ?? memberToken["displayName"]?.ToString() ?? memberToken["givenName"]?.ToString();
                if (!string.IsNullOrEmpty(name)) names.Add(name);
            }
            return names;
        }

        /// <summary>
        /// Resolves a team name for shared/unshared events via existing data or fallback heuristics.
        /// </summary>
        private async Task<string> ResolveTeamNameAsync(TeamInfoEx team, ITurnContext turnContext)
        {
            if (!string.IsNullOrEmpty(team.Name)) return team.Name;
            try
            {
                if (!string.IsNullOrEmpty(team.Id))
                {
                    var details = await TeamsInfo.GetTeamDetailsAsync(turnContext, team.Id);
                    if (!string.IsNullOrEmpty(details?.Name)) return details.Name;
                }
            }
            catch { }
            if (!string.IsNullOrEmpty(team.Id))
            {
                var part = team.Id.Split(':').Last();
                return part.Length > 8 ? $"Team-{part[..8]}" : $"Team-{part}";
            }
            return "Unknown Team";
        }

        /// <summary>
        /// Derives a stable display name for the current channel (name + inferred type) from channelData.
        /// </summary>
        private (string ChannelName, string ChannelType) GetChannelInfo(ITurnContext turnContext)
        {
            try
            {
                var tcd = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var extended = turnContext.Activity.GetChannelData<SharedChannelChannelData>();
                var name = extended?.Channel?.Name ?? tcd?.Channel?.Name ?? "Unknown Channel";
                var type = (extended?.SharedWithTeams?.Count ?? 0) + (extended?.UnsharedFromTeams?.Count ?? 0) > 0 ? "Shared" : (turnContext.Activity.Conversation?.ConversationType == "groupChat" ? "Group Chat" : "Standard");
                return (name, type);
            }
            catch { return ("Unknown Channel", "Unknown"); }
        }

        /// <summary>
        /// Builds the Adaptive Card for a direct member added event.
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
                            new AdaptiveTextBlock { Text = "Team Member Added", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Medium, Color = AdaptiveTextColor.Good },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Channel Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Channel Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Event Type:", Value = eventType },
                                    new AdaptiveFact { Title = "Member:", Value = memberName + " has been added" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a direct member removed event.
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
                            new AdaptiveTextBlock { Text = "Team Member Removed", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Medium, Color = AdaptiveTextColor.Attention },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Channel Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Channel Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Event Type:", Value = eventType },
                                    new AdaptiveFact { Title = "Member:", Value = memberName + " has been removed" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a channel shared event.
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
                            new AdaptiveTextBlock { Text = "Channel Shared With Team", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Channel Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Channel Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Team Name:", Value = teamName },
                                    new AdaptiveFact { Title = "Event:", Value = "Channel has been shared with this team" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a channel unshared event.
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
                            new AdaptiveTextBlock { Text = "Channel Unshared From Team", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Channel Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Channel Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Team Name:", Value = teamName },
                                    new AdaptiveFact { Title = "Event:", Value = "Channel sharing has been removed from this team" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a channel created event.
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
                            new AdaptiveTextBlock { Text = "Channel Created", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Team:", Value = teamName },
                                    new AdaptiveFact { Title = "Event:", Value = "New channel has been created" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a channel deleted event.
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
                            new AdaptiveTextBlock { Text = "Channel Deleted", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Name:", Value = channelName },
                                    new AdaptiveFact { Title = "Type:", Value = channelType },
                                    new AdaptiveFact { Title = "Team:", Value = teamName },
                                    new AdaptiveFact { Title = "Event:", Value = "Channel has been deleted" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Builds the Adaptive Card for a transitive membership add/remove event, including source team name.
        /// </summary>
        private Attachment CreateTransitiveMembershipAdaptiveCard(string channelName, string memberName, string action, string sourceTeamName, MembershipSourceEx source)
        {
            var membershipType = source?.MembershipType ?? "transitive";
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.4"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Style = action == "Added" ? AdaptiveContainerStyle.Good : AdaptiveContainerStyle.Attention,
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock { Text = $"Transitive Member {action}", Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                            new AdaptiveFactSet
                            {
                                Facts = new List<AdaptiveFact>
                                {
                                    new AdaptiveFact { Title = "Channel:", Value = channelName },
                                    new AdaptiveFact { Title = "Member:", Value = memberName },
                                    new AdaptiveFact { Title = "Membership Type:", Value = membershipType },
                                    new AdaptiveFact { Title = "Source Team:", Value = sourceTeamName },
                                    new AdaptiveFact { Title = "Source Type:", Value = source?.SourceType ?? "team" },
                                    new AdaptiveFact { Title = "Time:", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                                }
                            }
                        }
                    }
                }
            };
            return new Attachment { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        /// <summary>
        /// Simple echo handler so the bot can respond to test messages.
        /// </summary>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
        }
    }
}