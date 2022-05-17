// <copyright file="ActivityBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Bots
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using ReleaseManagement.Models;
    using ReleaseManagement.Services;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ActivityBot : ActivityHandler
    {
        private readonly ConcurrentDictionary<string, ReleaseManagementTask> taskDetails;
        private readonly ICardFactory cardFactory;

        public ActivityBot (ConcurrentDictionary<string, ReleaseManagementTask> taskDetails, ICardFactory cardFactory)
        {
            this.taskDetails = taskDetails;
            this.cardFactory = cardFactory;
        }

        /// <summary>
        /// Invoked when members are added/removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var isPresent = taskDetails.TryGetValue(Constant.TaskDetails, out ReleaseManagementTask releaseManagementTask);

            // Checking if task details is present and if bot is installed.
            if (isPresent && turnContext.Activity.MembersAdded.Count > 0)
            {
                var adaptiveAttachment = this.cardFactory.CreateAdaptiveCardAttachement(Path.Combine(".", "Resources", "WorkitemCardTemplate.json"), releaseManagementTask);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
            }
        }
    }
}
