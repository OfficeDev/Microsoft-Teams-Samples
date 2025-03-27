// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReceiveMessagesWithRSC.Bots
{
    /// <summary>
    /// A bot that handles Teams activity.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private const string SampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
        private const string Option = "Type 1 to know about the permissions required, Type 2 for documentation link";
        private const string PermissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app";
        private const string DocLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc";

        /// <summary>
        /// Handles incoming message activities.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userCommand = turnContext.Activity.Text.Trim();
            string responseMessage;

            switch (userCommand)
            {
                case "1":
                    responseMessage = PermissionRequired;
                    break;
                case "2":
                    responseMessage = DocLink;
                    break;
                default:
                    responseMessage = $"{SampleDescription}\n{Option}";
                    break;
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(responseMessage), cancellationToken);
        }

        /// <summary>
        /// Sends a welcome message when the bot is added to a team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string welcomeText = "Hello and welcome! With this sample your bot can receive user messages across standard channels in a team without being @mentioned";

            foreach (var member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
                }
            }
        }
    }
}
