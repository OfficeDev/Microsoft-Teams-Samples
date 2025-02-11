// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsStartNewThreadInTeam : ActivityHandler
    {
        private readonly string _appId;
        private readonly string _tenantId;

        public TeamsStartNewThreadInTeam(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"];
            _tenantId = configuration["MicrosoftAppTenantId"];
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text?.Trim().ToLower();

            if (string.IsNullOrEmpty(text)) return;

            if (text.Contains("listchannels"))
            {
                await ListTeamChannelsAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("threadchannel"))
            {
                await StartNewThreadInChannelAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("getteammember"))
            {
                await GetTeamMemberAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("getpagedteammembers"))
            {
                await GetPagedTeamMembersAsync(turnContext, _tenantId, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("I didn't understand that command. Please try again.", cancellationToken: cancellationToken);
            }
        }

        private async Task StartNewThreadInChannelAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            if (string.IsNullOrEmpty(teamsChannelId))
            {
                await turnContext.SendActivityAsync("No channel information found. Please make sure you're in a team channel.");
                return;
            }

            var activity = MessageFactory.Text("This will start a new thread in the channel.");
            try
            {
                var details = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, activity, teamsChannelId, _appId, cancellationToken);
                await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                    botAppId: _appId,
                    reference: details.Item1,
                    callback: async (t, ct) =>
                    {
                        await t.SendActivityAsync(MessageFactory.Text("This will be the first response to the new thread."), ct);
                    },
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Error starting thread: {ex.Message}");
            }
        }

        private async Task ListTeamChannelsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var teamId = turnContext.Activity.TeamsGetTeamInfo().Id;
                IEnumerable<ChannelInfo> channels = await TeamsInfo.GetTeamChannelsAsync(turnContext, teamId, cancellationToken);

                if (!channels.Any())
                {
                    await turnContext.SendActivityAsync("No channels found in this team.");
                    return;
                }

                // Adaptive Card JSON structure
                var adaptiveCardJson = new
                {
                    type = "AdaptiveCard",
                    version = "1.4",
                    body = new List<object>
            {
                new
                {
                    type = "TextBlock",
                    text = "List of Channels",
                    weight = "Bolder",
                    size = "Medium"
                }
            }
                };

                foreach (var (channel, index) in channels.Select((channel, index) => (channel, index + 1)))
                {
                    var channelName = string.IsNullOrEmpty(channel.Name) ? "General" : channel.Name;
                    adaptiveCardJson.body.Add(new
                    {
                        type = "TextBlock",
                        text = $"{index}. {channelName}",
                        wrap = true
                    });
                }

                var adaptiveCardAttachment = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = adaptiveCardJson
                };

                var reply = MessageFactory.Attachment(adaptiveCardAttachment);
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Error: {ex.Message}");
            }
        }

        private async Task GetTeamMemberAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var text = turnContext.Activity.Text.Trim().ToLower();
                var _aad = turnContext.Activity.From.AadObjectId;
                var _id = turnContext.Activity.TeamsGetTeamInfo().Id;
                var teamMember = await TeamsInfo.GetTeamMemberAsync(turnContext, _aad, _id, cancellationToken);
                if (teamMember != null)
                {
                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "User Information",
                        Size = AdaptiveTextSize.Default
                    });

                    card.Body.Add(new AdaptiveFactSet()
                    {
                        Separator = true,
                        Facts =
                        {
                            new AdaptiveFact
                            {
                                Title = "Name:",
                                Value = $"{teamMember.Name}"
                            },
                            new AdaptiveFact
                            {
                                Title = "Given Name:",
                                Value = $"{teamMember.GivenName}"
                            },
                            new AdaptiveFact
                            {
                                Title = "Surname:",
                                Value = $"{teamMember.Surname}"
                            },
                            new AdaptiveFact
                            {
                                Title = "Email:",
                                Value = $"{teamMember.Email}"
                            },
                            new AdaptiveFact
                            {
                                Title = "AAD Object ID:",
                                Value = $"{teamMember.AadObjectId}"
                            }
                        }
                    });

                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card,
                    };

                    var reply = MessageFactory.Attachment(adaptiveCardAttachment);
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Team member not found."), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Error getting member: {ex.Message}");
            }
        }

        private async Task GetPagedTeamMembersAsync(ITurnContext<IMessageActivity> turnContext, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                var teamId = turnContext.Activity.TeamsGetTeamInfo().Id;
                var members = new List<TeamsChannelAccount>();
                string continuationToken = null;

                do
                {
                    var currentPage = await TeamsInfo.GetPagedTeamMembersAsync(turnContext, teamId, continuationToken, null, cancellationToken);
                    continuationToken = currentPage.ContinuationToken;
                    members.AddRange(currentPage.Members);
                }
                while (continuationToken != null);

                if (members.Count == 0)
                {
                    await turnContext.SendActivityAsync("No team members found.");
                    return;
                }

                // Create an Adaptive Card with team member details
                var adaptiveCard = new
                {
                    type = "AdaptiveCard",
                    version = "1.4",
                    body = new List<object>
            {
                new
                {
                    type = "TextBlock",
                    text = "🔹 **Team Members**",
                    weight = "Bolder",
                    size = "Medium"
                }
            }
                };

                foreach (var member in members)
                {
                    adaptiveCard.body.Add(new
                    {
                        type = "Container",
                        items = new List<object>
                {
                    new { type = "TextBlock", text = $"**Name:** {member.Name}", wrap = true },
                    new { type = "TextBlock", text = $"**Email:** {member.Email}", wrap = true },
                    new { type = "TextBlock", text = $"**Given Name:** {member.GivenName}", wrap = true },
                    new { type = "TextBlock", text = $"**Surname:** {member.Surname}", wrap = true },
                    new { type = "TextBlock", text = $"**Role:** {member.Role ?? "N/A"}", wrap = true },
                    new { type = "TextBlock", text = $"**User Principal Name:** {member.UserPrincipalName}", wrap = true },
                    new { type = "TextBlock", text = "─────────────────────────────────────────────", weight = "Lighter" } // Separator
                }
                    });
                }

                var attachment = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = adaptiveCard
                };

                var reply = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(reply);
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync($"Error retrieving team members: {ex.Message}");
                await turnContext.SendActivityAsync($"Stack Trace: {ex.StackTrace}");
            }
        }


    }
}