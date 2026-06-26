// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using AdaptiveCards.Templating;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Collections.Concurrent;
using System.Collections;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private static int _counter = 0;
        private static List<string> users = new List<string>();
        private static ConcurrentDictionary<string, TeamsChannelAccount> teamMemberDetails = new ConcurrentDictionary<string, TeamsChannelAccount>();
        private static ConcurrentDictionary<string, string> teamMemberMessageIdDetails = new ConcurrentDictionary<string, string>();

        public TeamsConversationBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        private readonly string _adaptiveCardTemplate = Path.Combine(".", "Resources", "UserMentionCardTemplate.json");

        private readonly string _immersiveReaderCardTemplate = Path.Combine(".", "Resources", "ImmersiveReaderCard.json");

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("mention me"))
                await MentionAdaptiveCardActivityAsync(turnContext, cancellationToken);
            else if (text.Contains("mention"))
                await MentionActivityAsync(turnContext, cancellationToken);
            else if (text.Contains("who"))
                await GetSingleMemberAsync(turnContext, cancellationToken);
            else if (text.Contains("update"))
                await CardActivityAsync(turnContext, true, cancellationToken);
            else if (text.Contains("aadid"))
                await MessageAllMembersAsync(turnContext, cancellationToken, true);
            else if (text.Contains("message"))
                await MessageAllMembersAsync(turnContext, cancellationToken, false);
            else if (text.Contains("immersivereader"))
                await SendImmersiveReaderCardAsync(turnContext, cancellationToken);
            else if (text.Contains("delete"))
                await DeleteCardActivityAsync(turnContext, cancellationToken);
            else if (text.Contains("check"))
                await CheckReadUserCount(turnContext, cancellationToken);
            else if (text.Contains("reset"))
                await ResetReadUserCount(turnContext, cancellationToken);
            else if (text.Contains("label"))
                await AddAILabel(turnContext, cancellationToken);
            else if (text.Contains("feedback"))
                await AddFeedbackButtons(turnContext, cancellationToken);
            else if (text.Contains("sensitivity"))
                await AddSensitivityLabel(turnContext, cancellationToken);
            else if (text.Contains("citation"))
                await AddCitations(turnContext, cancellationToken);
            else if (text.Contains("aitext"))
                await SendAIMessage(turnContext, cancellationToken);
            else
                await CardActivityAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
                }
            }
        }

        private async Task AddAILabel(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
            new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Hey I'm a friendly AI bot. This message is generated via AI",
                Entities = new List<Entity>
               {
                   new Entity
                   {
                       Type = "https://schema.org/Message",
                       Properties = JObject.FromObject(new Dictionary<string, object>
                       {
                           { "@type", "Message" },
                           { "@context", "https://schema.org" },
                           { "additionalType", new List<string> { "AIGeneratedContent" } }
                       })
                   }
               }
            }
        );
        }

        private async Task AddFeedbackButtons(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "This is an example for Feedback buttons that helps to provide feedback for a bot message",
                    ChannelData = new { feedbackLoopEnabled = true },
                }
            );
        }

        private async Task AddSensitivityLabel(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
            new Activity
            {
                Type = ActivityTypes.Message,
                Text = "This is an example for sensitivity label that helps users identify the confidentiality of a message",
                Entities = new List<Entity>
               {
                   new Entity
                   {
                       Type = "https://schema.org/Message",
                       Properties = JObject.FromObject(new Dictionary<string, object>
                       {
                           { "@type", "Message" },
                           { "@context", "https://schema.org" }, // AI Generated label
                           { "usageInfo", new Dictionary<string, object>
                               {
                                   { "@type", "CreativeWork" },
                                   { "description", "Please be mindful of sharing outside of your team" }, // Sensitivity description
                                   { "name", "Confidential \\ Contoso FTE" } // Sensitivity title
                               }
                           }
                       })
                   }
               }
            }, cancellationToken);
        }

        private async Task SendAIMessage(ITurnContext<IMessageActivity> context, CancellationToken cancellationToken)
        {
            await context.SendActivityAsync(
            new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Hey I'm a friendly AI bot. This message is generated via AI [1]",
                ChannelData = JObject.FromObject(new Dictionary<string, object>
                {
                    { "feedbackLoopEnabled", true }
                }),
                Entities = new List<Entity>
               {
                   new Entity
                   {
                       Type = "https://schema.org/Message",
                       Properties = JObject.FromObject(new Dictionary<string, object>
                       {
                           { "@type", "Message" },
                           { "@context", "https://schema.org" },
                           { "usageInfo", new Dictionary<string, object>
                               {
                                   { "@type", "CreativeWork" },
                                   { "@id", "sensitivity1" }
                               }
                           },
                           { "additionalType", new List<string> { "AIGeneratedContent" } },
                           { "citation", new List<object>
                               {
                                   new Dictionary<string, object>
                                   {
                                       { "@type", "Claim" },
                                       { "position", 1 },
                                       { "appearance", new Dictionary<string, object>
                                           {
                                               { "@type", "DigitalDocument" },
                                               { "name", "Some secret citation" },
                                               { "url", "https://example.com/claim-1" },
                                               { "abstract", "Excerpt" },
                                               { "encodingFormat", "docx" },
                                               { "keywords", new List<string> { "Keyword1 - 1", "Keyword1 - 2", "Keyword1 - 3" } },
                                               { "usageInfo", new Dictionary<string, object>
                                                   {
                                                       { "@type", "CreativeWork" },
                                                       { "@id", "sensitivity1" },
                                                       { "name", "Sensitivity title" },
                                                       { "description", "Sensitivity description" }
                                                   }
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       })
                   }
               }
            }, cancellationToken);
        }

        private async Task AddCitations(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
            new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Hey I'm a friendly AI bot. This message is generated through AI [1]", // cite with [1]
                Entities = new List<Entity>
               {
                   new Entity
                   {
                       Type = "https://schema.org/Message",
                       Properties = JObject.FromObject(new Dictionary<string, object>
                       {
                           { "@type", "Message" },
                           { "@context", "https://schema.org" },
                           { "citation", new List<object>
                               {
                                   new Dictionary<string, object>
                                   {
                                       { "@type", "Claim" },
                                       { "position", 1 }, // Required. Must match the [1] in the text
                                       { "appearance", new Dictionary<string, object>
                                           {
                                               { "@type", "DigitalDocument" },
                                               { "name", "AI bot" }, // Title
                                               { "url", "https://example.com/claim-1" }, // Hyperlink on the title
                                               { "abstract", "Excerpt description" }, // Appears in the citation pop-up window
                                               { "text", "{\"type\":\"AdaptiveCard\",\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"version\":\"1.6\",\"body\":[{\"type\":\"TextBlock\",\"text\":\"Adaptive Card text\"}]}" }, // Stringified Adaptive Card
                                               { "keywords", new List<string> { "keyword 1", "keyword 2", "keyword 3" } }, // Appears in the citation pop-up window
                                               { "encodingFormat", "application/vnd.microsoft.card.adaptive" },
                                               { "usageInfo", new Dictionary<string, object>
                                                   {
                                                       { "@type", "CreativeWork" },
                                                       { "name", "Confidential \\ Contoso FTE" }, // Sensitivity title
                                                       { "description", "Only accessible to Contoso FTE" } // Sensitivity description
                                                   }
                                               },
                                               { "image", new Dictionary<string, object>
                                                   {
                                                       { "@type", "ImageObject" },
                                                       { "name", "Microsoft Word" }
                                                   }
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       })
                   }
               }
            }, cancellationToken);
        }

        /// <summary>
        /// Checks the count of members who have read the message sent by MessageAllMembers command
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task CheckReadUserCount(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (teamMemberDetails.Count != 0)
            {
                var userList = string.Join(", ", users);
                await turnContext.SendActivityAsync(MessageFactory.Text($"Number of members read the message : {_counter} \n\n Members : {userList}"), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Read count is zero. Please make sure to send message to all members firstly to check the count of members who have read your message."), cancellationToken);
            }
        }

        /// <summary>
        /// Resets the check count of members who have read the message sent by MessageAllMembers command
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task ResetReadUserCount(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            teamMemberDetails = new ConcurrentDictionary<string, TeamsChannelAccount>();
            _counter = 0;
            users = new List<string>();
            teamMemberMessageIdDetails = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Invoked when user read the message sent by bot in personal scope
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsReadReceiptAsync(ReadReceiptInfo readReceiptInfo, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            teamMemberMessageIdDetails.TryGetValue(turnContext.Activity.From.AadObjectId, out string messageId);
            if (readReceiptInfo.IsMessageRead(messageId))
            {
                teamMemberDetails.TryGetValue(turnContext.Activity.From.AadObjectId, out TeamsChannelAccount memberDetails);
                _counter++;
                users.Add(memberDetails.Name);
                teamMemberMessageIdDetails.TryRemove(turnContext.Activity.From.AadObjectId, out string valueRemoved);
            }
        }

        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                await turnContext.SendActivityAsync($"Welcome to Microsoft Teams conversationUpdate events demo bot. This bot is configured in {channelData.Team.Name}.");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Microsoft Teams conversationUpdate events demo bot.");
            }
        }

        private async Task CardActivityAsync(ITurnContext<IMessageActivity> turnContext, bool update, CancellationToken cancellationToken)
        {

            var card = new HeroCard
            {
                Buttons = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message all members",
                                Text = "MessageAllMembers"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Message all members using AADId",
                                Text = "MessageAllMembersUsingAADId"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Who am I?",
                                Text = "whoami"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Send Immersive Reader Card",
                                Text = "ImmersiveReader"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Find me in Adaptive Card",
                                Text = "mention me"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Delete card",
                                Text = "Delete"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Check read count",
                                Text = "check"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Reset read count",
                                Text = "reset"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "AI label",
                                Text = "label"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Feedback buttons",
                                Text = "feedback"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Sensitivity label",
                                Text = "sensitivity"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Citations",
                                Text = "citation"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Send AI message",
                                Text = "aitext"
                            }
                        }
            };


            if (update)
            {
                await SendUpdatedCard(turnContext, card, cancellationToken);
            }
            else
            {
                await SendWelcomeCard(turnContext, card, cancellationToken);
            }

        }

        private async Task GetSingleMemberAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var member = new TeamsChannelAccount();

            try
            {
                member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            }
            catch (ErrorResponseException e)
            {
                if (e.Body.Error.Code.Equals("MemberNotFoundInConversation", StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync("Member not found.");
                    return;
                }
                else
                {
                    throw e;
                }
            }

            var message = MessageFactory.Text($"You are: {member.Name}.");
            var res = await turnContext.SendActivityAsync(message);

        }

        private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
        }

        /// <summary>
        /// Using proactive messaging, messages sent to all members of the chat.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="isAadId"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, bool isAadId)
        {
            await ResetReadUserCount(turnContext, cancellationToken);
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            ConversationReference conversationReference = null;

            var members = await GetPagedMembers(turnContext, cancellationToken);

            foreach (var teamMember in members)
            {
                var proactiveMessage = MessageFactory.Text($"Hello {teamMember.GivenName} {teamMember.Surname}. I'm a Teams conversation bot.");

                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = isAadId ? new ChannelAccount[] { new ChannelAccount(teamMember.AadObjectId) } : new ChannelAccount[] { teamMember },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };
                try
                {
                    await ((CloudAdapter)turnContext.Adapter).CreateConversationAsync(
                   credentials.MicrosoftAppId,
                   teamsChannelId,
                   serviceUrl,
                   credentials.OAuthScope,
                   conversationParameters,
                   async (t1, c1) =>
                   {
                       conversationReference = t1.Activity.GetConversationReference();
                       await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                           _appId,
                           conversationReference,
                           async (t2, c2) =>
                           {
                               var message = await t2.SendActivityAsync(proactiveMessage, c2);
                               teamMemberDetails.TryAdd(teamMember.AadObjectId, teamMember);
                               teamMemberMessageIdDetails.TryAdd(teamMember.AadObjectId, message.Id);
                           },
                           cancellationToken);
                   },
                   cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


            }

            await turnContext.SendActivityAsync(MessageFactory.Text($"All messages have been sent to {teamMemberDetails.Count} members."), cancellationToken);
        }

        private static async Task<List<TeamsChannelAccount>> GetPagedMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var members = new List<TeamsChannelAccount>();
            string continuationToken = null;

            do
            {
                var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                members = members.Concat(currentPage.Members).ToList();
            }
            while (continuationToken != null);

            return members;
        }

        private static async Task SendWelcomeCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            var initialValue = new JObject { { "count", 0 } };
            card.Title = "Welcome!";
            card.Buttons.Add(new CardAction
            {
                Type = ActionTypes.MessageBack,
                Title = "Update Card",
                Text = "UpdateCardAction",
                Value = initialValue
            });

            var activity = MessageFactory.Attachment(card.ToAttachment());

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        private static async Task SendUpdatedCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            card.Title = "I've been updated";

            var data = turnContext.Activity.Value as JObject;
            data = JObject.FromObject(data);
            data["count"] = data["count"].Value<int>() + 1;
            card.Text = $"Update count - {data["count"].Value<int>()}";

            card.Buttons.Add(new CardAction
            {
                Type = ActionTypes.MessageBack,
                Title = "Update Card",
                Text = "UpdateCardAction",
                Value = data
            });

            var activity = MessageFactory.Attachment(card.ToAttachment());
            activity.Id = turnContext.Activity.ReplyToId;

            await turnContext.UpdateActivityAsync(activity, cancellationToken);
        }

        private async Task MentionAdaptiveCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var member = new TeamsChannelAccount();

            try
            {
                member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            }
            catch (ErrorResponseException e)
            {
                if (e.Body.Error.Code.Equals("MemberNotFoundInConversation", StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync("Member not found.");
                    return;
                }
                else
                {
                    throw e;
                }
            }

            var templateJSON = File.ReadAllText(_adaptiveCardTemplate);
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJSON);
            var memberData = new
            {
                userName = member.Name,
                userUPN = member.UserPrincipalName,
                userAAD = member.AadObjectId
            };
            string cardJSON = template.Expand(memberData);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }

        private async Task SendImmersiveReaderCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardJSON = File.ReadAllText(_immersiveReaderCardTemplate);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }

        private async Task MentionActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var mention = new Mention
            {
                Mentioned = turnContext.Activity.From,
                Text = $"<at>{XmlConvert.EncodeName(turnContext.Activity.From.Name)}</at>",
            };

            var replyActivity = MessageFactory.Text($"Hello {mention.Text}.");
            replyActivity.Entities = new List<Entity> { mention };

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }


        //-----Subscribe to Conversation Events in Bot integration
        protected override async Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel created");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the new Channel name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{channelInfo.Name} is the Channel deleted");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }

        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (TeamsChannelAccount member in membersRemoved)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    // The bot was removed
                    // You should clear any cached data you have for this team
                }
                else
                {
                    var heroCard = new HeroCard(text: $"{member.Name} was removed from {teamInfo.Name}");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
                }
            }
        }

        protected override async Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard(text: $"{teamInfo.Name} is the new Team name");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
        }
        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                var newReaction = $"You reacted with '{reaction.Type}' to the following message: '{turnContext.Activity.ReplyToId}'";
                var replyActivity = MessageFactory.Text(newReaction);
                await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                var newReaction = $"You removed the reaction '{reaction.Type}' from the following message: '{turnContext.Activity.ReplyToId}'";
                var replyActivity = MessageFactory.Text(newReaction);
                await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        // This method is invoked when message sent by user is updated in chat.
        protected override async Task OnTeamsMessageEditAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is updated");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        // This method is invoked when message sent by user is undeleted in chat.
        protected override async Task OnTeamsMessageUndeleteAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is undeleted");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        // This method is invoked when message sent by user is soft deleted in chat.
        protected override async Task OnTeamsMessageSoftDeleteAsync(ITurnContext<IMessageDeleteActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyActivity = MessageFactory.Text("Message is soft deleted");
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }
    }
}