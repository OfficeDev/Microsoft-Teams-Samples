// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.SPListBot.Models;
using Microsoft.BotBuilderSamples.SPListBot.Repositories;

namespace Microsoft.BotBuilderSamples.SPListBot.Bots
{
    public class SharePointListBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded, 
            ITurnContext<IConversationUpdateActivity> turnContext, 
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await SendWelcomeCardAsync(turnContext, cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(turnContext.Activity.Text) && turnContext.Activity.Value != null)
            {
                // Process form submission
                await SaveDataAsync(turnContext, cancellationToken);
                
                // Notification
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("Your data has been saved, enter any key to continue..."), 
                    cancellationToken);
            }
            else
            {
                // Send adaptive card form
                await SendInfoFormAsync(turnContext, cancellationToken);
            }
        }

        private async Task SendWelcomeCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var username = turnContext.Activity.From.Name;

            var card = new HeroCard
            {
                Title = $"Hi {username}, Welcome to the Teams Bot using SharePoint List",
                Subtitle = "SharePoint List Bot is designed to save conversations to a SharePoint List.",
                Text = "Type anything to start..."
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task SendInfoFormAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Load adaptive card from resource file
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardPath = Path.Combine(paths);
            
            if (!File.Exists(adaptiveCardPath))
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text("Error: Adaptive card resource not found."), 
                    cancellationToken);
                return;
            }

            var adaptiveCardJson = await File.ReadAllTextAsync(adaptiveCardPath, cancellationToken);

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonSerializer.Deserialize<object>(adaptiveCardJson)
            };

            var response = MessageFactory.Attachment(adaptiveCardAttachment);
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task SaveDataAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(turnContext.Activity.Value);
                var userProfile = JsonSerializer.Deserialize<UserProfile>(jsonData);

                if (userProfile == null)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Error: Could not parse user data."), 
                        cancellationToken);
                    return;
                }

                var body = new Values
                {
                    Description = jsonData,
                    Name = userProfile.Name,
                    Address = userProfile.Address,
                    Username = turnContext.Activity.From.Name,
                };

                // Save data to SharePoint
                await SharepointRepository.WriteConversationToSPListAsync(body);
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"Error saving data: {ex.Message}"), 
                    cancellationToken);
            }
        }
    }
}
