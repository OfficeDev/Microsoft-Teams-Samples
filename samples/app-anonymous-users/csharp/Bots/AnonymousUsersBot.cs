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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnonymousUsers.Bots
{
    public class AnonymousUsersBot : TeamsActivityHandler
    {
        /// <summary>
        /// Get the Microsoft appid and assign the value to the _appId properties.
        /// </summary>
        private string _appId;

        /// <summary>
        /// Get the Microsoft password and assign the value to the _appPassword properties.
        /// </summary>
        private string _appPassword;

        /// <summary>
        /// Assgin_logger error data and the _logger field shouldn't be modified.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        /// <param name="config"></param>
        public AnonymousUsersBot(IConfiguration config, ILogger<AnonymousUsersBot> logger)
        {
            _logger = logger;
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        /// <summary>
        /// Read vote card JSON template
        /// </summary>
        private readonly string _voteCardReaderCardTemplate = Path.Combine(".", "Resources", "VoteCard.json");

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                // Remove bot at-mentions for teams/groupchat scope
                turnContext.Activity.RemoveRecipientMention();
                var text = turnContext.Activity.Text.Trim().ToLower();

                if (text.Contains("vote"))
                {
                    await SendVoteCardAsync(turnContext, cancellationToken);
                }
                else if (text.Contains("createconversation"))
                {
                    await CardActivityAsync(turnContext, cancellationToken);
                }
                else if (text.Contains("message"))
                {
                    // Create 1:1 bot conversation with users existing in the current meeting.
                    await CreateConversationWithUsersAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await SendDataOnCardActions(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// Send message all members card.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        private async Task CardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Title = "Message all members",
                        Text = "message"
                    }
                }
            };

            var activity = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(activity, cancellationToken);

        }

        /// <summary>
        /// Fetching member information and sending a vote success message
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        private async Task SendDataOnCardActions(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                // Initializes a new instance of the TeamsChannelAccount class.
                var member = new TeamsChannelAccount();

                try
                {
                    member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                }
                catch (ErrorResponseException exception)
                {
                    if (exception.Body.Error.Code.Equals("MemberNotFoundInConversation", StringComparison.OrdinalIgnoreCase))
                    {
                        await turnContext.SendActivityAsync("Member not found.");
                        return;
                    }
                    else
                    {
                        throw exception;
                    }
                }

                var message = MessageFactory.Text($"{member.Name} voted successfully.");

                await turnContext.SendActivityAsync(message);
            }
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.UserRole != "anonymous" && teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);

                }
                // User role 'anonymous' indicates that newly added member is an anonymous user.
                else if (teamMember.UserRole == "anonymous" && teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome anonymous user to the team."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets a paginated list of members of a team. 
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
        private async Task CreateConversationWithUsersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            int usersCount = 0;
            int anonymousUsersCount = 0;
            bool IsAnonymousUser = false;
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            ConversationReference conversationReference = null;

            var members = await GetPagedMembers(turnContext, cancellationToken);

            foreach (var teamMember in members)
            {
                var proactiveMessage = MessageFactory.Text($"Hello {teamMember.GivenName} {teamMember.Surname}. I'm a Teams conversation bot that support anonymous users.");

                if (teamMember.UserRole != null && teamMember.UserRole == "user")
                {
                    usersCount++;
                }

                if (teamMember.UserRole != null && teamMember.UserRole == "anonymous")
                {
                    anonymousUsersCount++;
                }

                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = new ChannelAccount[] { teamMember },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

                try
                {
                    // Creates a conversation on the specified groupchat and send file consent card on that conversation.
                    await ((CloudAdapter)turnContext.Adapter).CreateConversationAsync(
                        credentials.MicrosoftAppId,
                        teamsChannelId,
                        serviceUrl,
                        credentials.OAuthScope,
                        conversationParameters,
                        async (turnsContextActiT1, cancelTokenC1) =>
                        {
                            conversationReference = turnsContextActiT1.Activity.GetConversationReference();
                            await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                                _appId,
                                conversationReference,
                                async (turnsContextActiT2, cancelTokenC2) =>
                                {
                                    await turnsContextActiT2.SendActivityAsync(proactiveMessage, cancelTokenC2);
                                },
                                cancellationToken);
                        },
                        cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Error:" + exception.Message);

                    //This condition is handling error for anonymous users because "Bot cannot create a conversation with an anonymous user".
                    if (((ErrorResponseException)exception).Response.Content.Contains("anonymous"))
                    {
                        IsAnonymousUser = true;
                    }
                }
            }

            if (IsAnonymousUser == true)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Users count: {usersCount} <br> Anonymous users count: {anonymousUsersCount} <br> Note: Bot cannot create a conversation with an anonymous user."), cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("All messages have been sent."), cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are removed to the conversation.
        /// </summary>
        /// <param name="membersRemoved">A list of all the members removed from the channel.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
            {
                // If AadObjectId property is null, it means it's an anonymous user otherwise normal user.
                if (member.AadObjectId == null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("The anonymous user was removed from the teams meeting."), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("The user was removed from the teams meeting."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Send vote adaptive card template.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        private async Task SendVoteCardAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardJSON = File.ReadAllText(_voteCardReaderCardTemplate);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }
    }
}