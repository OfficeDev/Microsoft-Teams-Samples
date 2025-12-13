// <copyright file="AnonymousUsersBot.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                var text = turnContext.Activity.Text.Trim().ToLower();
                await turnContext.SendActivityAsync(text);
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
                await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
            }
            var threadId = GetTeamsThreadId(turnContext);

            //await turnContext.SendActivityAsync(MessageFactory.Text(threadId));
        }
     
        private string GetTeamsThreadId(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            // If not in a personal scope, the threadId will be included in the activity
            if (turnContext.Activity.Conversation.ConversationType != "personal")
            {
                return turnContext.Activity.Conversation.Id;
            }

            // Verify that the user has a valid aadObjectId
            var userAadId = turnContext.Activity.From.AadObjectId;
            if (string.IsNullOrEmpty(userAadId))
            {
                throw new Exception("Invalid AAD id for user");
            }

            // When in a personal scope, we need to construct the threadId manually
            // This assumes your bot AAD clientId is named BOT_ID in your environment variables
            var botId = "9b8d4c4a-6e5f-404b-90ce-4cb590142a13";
            if (string.IsNullOrEmpty(botId))
            {
                throw new Exception("BOT_ID environment variable not set");
            }

            return $"19:{userAadId}_{botId}@unq.gbl.spaces";
        }
    }
}