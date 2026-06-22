// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using MessagingExtensionReminder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;

namespace MessagingExtensionReminder.Controllers
{
    /// <summary>
    /// Class with properties related to task reminder.
    /// </summary>
    [Route("api/task")]
    [ApiController]
    public class TaskReminderController : ControllerBase
    {
        private readonly CloudAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public TaskReminderController(CloudAdapter adapter,
          IConfiguration configuration,
          ConcurrentDictionary<string, ConversationReference> conversationReferences,
          ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _taskDetails = taskDetails;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        /// <summary>
        /// This endpoint is called to send task reminder card.
        /// </summary>
        [HttpGet]
        public async Task GetTaskReminder()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default);
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _taskDetails.TryGetValue("taskDetails", out var taskList);

            if (taskList == null)
                return;

            foreach (var task in taskList)
            {
                var now = DateTimeOffset.Now;

                if (task.DateTime.Minute == now.Minute && task.DateTime.Hour == now.Hour && task.DateTime.Day == now.Day && task.DateTime.Month == now.Month && task.DateTime.Year == now.Year)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskReminder(task.Title, task.Description)), cancellationToken);
                }
            }
        }

        private static Attachment GetAdaptiveCardForTaskReminder(string title, string description)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Reminder for scheduled task!",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Task title: " + title,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Task description: " + description,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    }
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}