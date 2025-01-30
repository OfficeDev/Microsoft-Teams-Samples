// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace TagMentionBot.Bots
{
    public class TeamsConversationBot<T> : DialogBot<T> where T : Dialog
    {
        private string _appId;
        private string _appPassword;

        public TeamsConversationBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
                    : base(conversationState, userState, dialog, logger)
        {
        }

        /// <summary>
        /// Called when a new user is joins the team, or when the application is first installed
        /// </summary>
        /// <param name="membersAdded"></param>
        /// <param name="teamInfo"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
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

        /// <summary>
        /// This is triggered during the app installation update activity
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Tag mention Teams bot app. Please follow the below commands for mentioning the tags: \r\n\r\n\r\n1. Command: \"`@<Bot-name> <your-tag-name>`\" - It will work only if you have Graph API permissions to fetch the tags and bot will mention the tag accordingly in team's channel scope.\r\n\r\n\r\n2. Command \"`@<Bot-name> @<your-tag>`\" - It will work without Graph API permissions but you need to provide the tag as command to experience tag mention using bot.");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Tag mention demo bot. Type anything to get logged in. Type 'logout' to sign-out");
            }
        }

        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

            // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.
            // Run the Dialog with the new Invoke Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
