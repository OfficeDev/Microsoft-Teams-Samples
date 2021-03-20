// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.SPListBot.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.SPListBot.Bots
{
    public class SharePointListBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await WelcomeCardAsync(turnContext, cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Save any state changes that might have occurred during the turn.
            if (string.IsNullOrWhiteSpace(turnContext.Activity.Text) && turnContext.Activity.Value != null)
            {
                //turnContext.Activity.Text = JsonConvert.SerializeObject(turnContext.Activity.Value);
                await SaveData(turnContext);

                // Notification
                await turnContext.SendActivityAsync(MessageFactory.Text("Your data has been saved, enter any key to continue..."), cancellationToken);
            }
            else
            {
                // Send details
                await SendInfoFormAsync(turnContext, cancellationToken);
            }
        }

        private async Task WelcomeCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string username = turnContext.Activity.From.Name;

            var card = new HeroCard
            {
                Title = $"Hi " + username + ",  Welcome to the teams Bot using SharePoint List",
                Subtitle = $"SharePoint List Bot is to save conversations at SharePoint List.",
                Text = @"Type anything to start ... "
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task SendInfoFormAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            var response = MessageFactory.Attachment(adaptiveCardAttachment);
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task SaveData(ITurnContext turnContext)
        {
            var _data = JsonConvert.SerializeObject(turnContext.Activity.Value);
            UserProfile userProfile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(_data);

            Values body = new Values
            {
                Description = _data,
                Name = userProfile.Name,
                Address = userProfile.Address,
                Username = turnContext.Activity.From.Name,
            };

            // Save data to SP
            await Repositories.SharepointRepository.WriteConversationToSPList(body);
        }
    }
}
