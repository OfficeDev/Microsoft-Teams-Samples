// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using BotDailyTaskReminder.Models;
using Microsoft.AspNetCore.Http;
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

namespace BotDailyTaskReminder.Controllers
{
    /// <summary>
    /// Controller to handle task reminders.
    /// </summary>
    [Route("api/task")]
    [ApiController]
    public class TaskReminderController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskReminderController"/> class.
        /// </summary>
        /// <param name="adapter">The bot framework HTTP adapter.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="conversationReferences">The conversation references.</param>
        /// <param name="taskDetails">The task details.</param>
        public TaskReminderController(IBotFrameworkHttpAdapter adapter,
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
        /// This endpoint is called to send task reminder cards.
        /// </summary>
        [HttpGet]
        public async Task GetTaskReminder()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default);
            }
        }

        /// <summary>
        /// Callback method to send activity.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (_taskDetails.TryGetValue("taskDetails", out var taskList))
            {
                var currentDateTime = DateTime.Now;

                foreach (var task in taskList)
                {
                    if (task.DateTime.Hour == currentDateTime.Hour &&
                        task.DateTime.Minute == currentDateTime.Minute &&
                        task.DateTime.Date == currentDateTime.Date)
                    {
                        foreach (var day in task.SelectedDays)
                        {
                            if ((int)day == (int)currentDateTime.DayOfWeek ||
                                ((int)day == 7 && (int)currentDateTime.DayOfWeek == 0))
                            {
                                await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskReminder(task.Title, task.Description)), cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates and returns an adaptive card for task reminder.
        /// </summary>
        /// <param name="title">The task title.</param>
        /// <param name="description">The task description.</param>
        /// <returns>The adaptive card as an attachment.</returns>
        private Attachment GetAdaptiveCardForTaskReminder(string title, string description)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = "Reminder for a scheduled task!",
                            Weight = AdaptiveTextWeight.Bolder,
                            Spacing = AdaptiveSpacing.Medium,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = "Task title: " + title,
                            Weight = AdaptiveTextWeight.Default,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = "Task description: " + description,
                            Weight = AdaptiveTextWeight.Default,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true,
                        }
                    },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}