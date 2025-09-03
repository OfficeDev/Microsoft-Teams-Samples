// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using MessagingExtensionReminder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        /// This enpoint is called to send task reminder card.
        /// </summary>
        [HttpGet]
        public async void GetTaskReminder()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
        }

        /// <summary>
        /// Callback method to send activity.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var taskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out taskList);

            foreach (var task in taskList)
            {
                var time = new DateTimeOffset(DateTime.Now);

                if(task.DateTime.Minute == time.Minute && task.DateTime.Hour == time.Hour && task.DateTime.Day == time.Day && task.DateTime.Month == time.Month && task.DateTime.Year == time.Year)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskReminder(task.Title, task.Description)), cancellationToken);
                }              
            }
        }

        /// <summary>
        /// Sample Adaptive card task reminder.
        /// </summary>
        private Attachment GetAdaptiveCardForTaskReminder(string title, string description)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
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
                        Text = "Task title: "+ title,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Task description: "+ description,
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