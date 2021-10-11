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
    public class ActivityBot : TeamsActivityHandler
    {
        /// <summary>
        /// Sample description text.
        /// </summary>
        private const string SampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned";

        /// <summary>
        /// Options text.
        /// </summary>
        private const string Option = "Type 1 to know about the permissions required,  Type 2 for documentation link";

        /// <summary>
        /// Permission required text.
        /// </summary>
        private const string PermissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app";

        /// <summary>
        /// Docs link.
        /// </summary>
        private const string DocLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc";

        /// <summary>
        /// Handle when a message is addressed to the bot
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userCommand = turnContext.Activity.Text;
            switch (userCommand)
            {
                case "1":
                    await turnContext.SendActivityAsync(MessageFactory.Text(PermissionRequired));
                    return;

                case "2":
                    await turnContext.SendActivityAsync(MessageFactory.Text(DocLink));
                    return;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(SampleDescription));
                    await turnContext.SendActivityAsync(MessageFactory.Text(Option));
                    return;
            }
        }

        /// <summary>
        /// Overriding to send welcome card once Bot/ME is installed in team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Welcome card  when bot is added first time by user.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome! With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
            foreach (var member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}