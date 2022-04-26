// <copyright file="ActivityBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using ReleaseManagement.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReleaseManagement.Bots
{
    public class ActivityBot : ActivityHandler
    {
        private readonly ConcurrentDictionary<string, ReleaseManagementTask> taskDetails;
        public ActivityBot (ConcurrentDictionary<string, ReleaseManagementTask> taskDetails)
        {
            this.taskDetails = taskDetails;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var isPresent = taskDetails.TryGetValue(Constant.TaskDetails, out ReleaseManagementTask releaseManagementTask);

            // Checking if task details is present and if bot is installed.
            if (isPresent && turnContext.Activity.MembersAdded.Count > 0)
            {
                var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4))
                {
                    Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = "New task created",
                                Weight = AdaptiveTextWeight.Bolder
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"{releaseManagementTask.TaskTitle}"
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"Assigned to- {releaseManagementTask.AssignedToName}"
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"State- {releaseManagementTask.State}"
                            },
                        }
                };
                var adaptiveAttachment = new Attachment
                {
                    Content = adaptiveCard,
                    ContentType = AdaptiveCard.ContentType,
                };
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
            }
        }
    }
}
