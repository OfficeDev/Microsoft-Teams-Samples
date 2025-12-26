// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MessagingExtensionReminder.Models;
using Microsoft.Teams.Api;
using System.Collections.Concurrent;

namespace MessagingExtensionReminder.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        public ReminderBackgroundService(
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails,
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger)
        {
            _conversationReferences = conversationReferences;
            _taskDetails = taskDetails;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // Runs the background service and checks for reminders every minute
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendReminders(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking reminders");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        // Checks for due tasks and sends reminder messages to users
        private async Task CheckAndSendReminders(CancellationToken stoppingToken)
        {
            var now = DateTimeOffset.Now;
            if (!_taskDetails.TryGetValue("taskDetails", out var tasks) || tasks == null || tasks.Count == 0)
            {
                return;
            }
            foreach (var task in tasks)
            {
                var timeDifference = Math.Abs((task.DateTime - now).TotalMinutes);
                if (timeDifference < 1)
                {
                    foreach (var conversationRef in _conversationReferences)
                    {
                        var conversationReference = conversationRef.Value;
                        try
                        {
                            await SendProactiveReminder(conversationReference, task, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send reminder");
                        }
                    }
                }
            }
        }

        // Sends a proactive reminder message to a specific conversation
        private async Task SendProactiveReminder(ConversationReference conversationReference, SaveTaskDetail task, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<Microsoft.Teams.Apps.App>();
            var reminderMessage = $"**Reminder for scheduled task!**\n\n" +
                                $"Task title: {System.Net.WebUtility.UrlDecode(task.Title)}\n\n" +
                                $"Task description: {System.Net.WebUtility.UrlDecode(task.Description)}";
            await app.Send(conversationReference.Conversation.Id, reminderMessage);
        }
    }
}
