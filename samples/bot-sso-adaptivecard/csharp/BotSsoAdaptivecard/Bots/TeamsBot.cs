// <copyright file="TeamsBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This bot is derived (view DialogBot<T>) from the TeamsActivityHandler class currently included as part of this sample.
    public class TeamsBot : DialogBot<MainDialog>
    {
        public TeamsBot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<DialogBot<MainDialog>> logger, IConfiguration configuration)
            : base(conversationState, userState, dialog, logger, configuration["ConnectionName"])
        {
            if (string.IsNullOrEmpty(configuration["ConnectionName"]))
            {
                logger.LogError("ConnectionName is missing from configuration.");
            }
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members, except the bot, join the conversation, such as your bot's welcome logic.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Iterate over all members added to the conversation.
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Send a welcome message to new members.
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to Universal Adaptive Cards. Type 'login' to sign in using Universal SSO."), cancellationToken);
                }
            }
        }
    }
}
