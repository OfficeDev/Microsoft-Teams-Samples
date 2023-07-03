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

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsConversationBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;

        public TeamsConversationBot(IConfiguration config)
        {
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        private readonly string _adaptiveCardTemplate = Path.Combine(".", "Resources", "UserMentionCardTemplate.json");

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
                await MentionAdaptiveCardActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if(teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
                }
            }
        }

        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if(turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Microsoft Teams Tag mention demo bot. This bot is configured in {turnContext.Activity.Conversation.Name}");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Microsoft Teams Tag mention demo bot.");
            }
        }

        private async Task MentionAdaptiveCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var templateJSON = File.ReadAllText(_adaptiveCardTemplate);
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(templateJSON),
            };
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);
        }
    }
}
