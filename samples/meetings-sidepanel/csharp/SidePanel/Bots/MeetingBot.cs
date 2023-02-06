// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using SidePanel.Controllers;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class MeetingBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = "Hello and welcome **" + turnContext.Activity.From.Name + "** to the Meeting Extensibility SidePanel app.";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome " + turnContext.Activity.From.Name + " to the Meeting Extensibility SidePanel app.";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            HomeController.serviceUrl = turnContext.Activity.ServiceUrl;
            HomeController.conversationId = turnContext.Activity.Conversation.Id;
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
    }
}