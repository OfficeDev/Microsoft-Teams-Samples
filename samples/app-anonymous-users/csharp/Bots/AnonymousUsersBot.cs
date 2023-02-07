// <copyright file="AnonymousUsersBot.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class AnonymousUsersBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        /// <param name="config"></param>
        public AnonymousUsersBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        private readonly string _anonymousCardReaderCardTemplate = Path.Combine(".", "Resources", "VoteCard.json");

        /// <summary>
        /// Override this in a derived class to provide logic specific to Message activities, such as the conversational logic.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                turnContext.Activity.RemoveRecipientMention();
                var text = turnContext.Activity.Text.Trim().ToLower();

                if (text.Contains("vote"))
                {
                    await SendAnonymousCardAsync(turnContext, cancellationToken);
                }
                else if (text.Contains("createconversation"))
                {
                    try
                    {
                        await MessageAllMembersAsync(turnContext, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await turnContext.SendActivityAsync(((ErrorResponseException)ex).Response.Content);
                    }
                }
            }
            await SendDataOnCardActions(turnContext, cancellationToken);
        }

        /// <summary>
        /// Gets the account of a single conversation member. This works in one-on-one, group, and teams scoped conversations.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        private async Task SendDataOnCardActions(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
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

                var message = MessageFactory.Text($"{member.Name} voted successfully.");
                await turnContext.SendActivityAsync(message);
            }
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot join the channel, such as your bot's welcome logic.
        /// </summary>
        /// <param name="membersAdded">Override this in a derived class to provide logic for when members other than the bot join the channel, such as your bot's welcome logic.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.UserRole != "anonymous" && teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);

                }
                else if (teamMember.UserRole == "anonymous" && teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome anonymous users to the team."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets a paginated list of members of one-on-one, group, or team conversation.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
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

        /// <summary>
        /// The conversation information to use to create the conversation.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
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
                    Members = new ChannelAccount[] { teamMember },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

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
                                await t2.SendActivityAsync(proactiveMessage, c2);
                            },
                            cancellationToken);
                    },
                    cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("All messages have been sent."), cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot leave the channel, such as your bot's good-bye logic.
        /// </summary>
        /// <param name="membersRemoved">A list of all the members removed from the channel, as described by the conversation update activity.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
            {
                if (member.AadObjectId == null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"The anonymous user was removed from team."), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"The user was removed from team."), cancellationToken);
                }
            }
        }
        /// <summary>
        /// Adaptive Card - This card is highly customizable and can contain any combination of text, speech, images, buttons, and input fields.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        private async Task SendAnonymousCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardJSON = File.ReadAllText(_anonymousCardReaderCardTemplate);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }
    }
}
