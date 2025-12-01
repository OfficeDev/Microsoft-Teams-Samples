using BotDailyTaskReminder.Models;
using Microsoft.Teams.Api;
using System.Collections.Concurrent;

namespace BotDailyTaskReminder.Services
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service is starting.");

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

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Reminder Background Service is stopping.");
        }

        private async Task CheckAndSendReminders(CancellationToken stoppingToken)
        {
            var now = DateTimeOffset.Now;
            _logger.LogInformation($"Checking reminders at {now:g}");

            foreach (var userTasks in _taskDetails)
            {
                var userId = userTasks.Key;
                var tasks = userTasks.Value;

                if (!_conversationReferences.TryGetValue(userId, out var conversationReference))
                {
                    _logger.LogWarning($"No conversation reference found for user {userId}");
                    continue;
                }

                for (int i = 0; i < tasks.Count; i++)
                {
                    var task = tasks[i];

                    // Check if it's time to send the reminder
                    if (ShouldSendReminder(task, now))
                    {
                        _logger.LogInformation($"Sending reminder for task '{task.Title}' to user {userId}");

                        try
                        {
                            await SendProactiveReminder(conversationReference, task, stoppingToken);

                            // Update the next reminder time
                            task.DateTime = CalculateNextReminderTime(task, now);
                            _logger.LogInformation($"Next reminder for task '{task.Title}' scheduled at {task.DateTime:g}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send reminder for task '{task.Title}'");
                        }
                    }
                }
            }
        }

        private bool ShouldSendReminder(SaveTaskDetail task, DateTimeOffset now)
        {
            // Check if the current day is in the selected days
            if (!task.SelectedDays.Contains(now.DayOfWeek))
            {
                return false;
            }

            // Check if the time has passed (within a 1-minute window)
            var timeDifference = Math.Abs((task.DateTime - now).TotalMinutes);
            return timeDifference < 1;
        }

        private DateTimeOffset CalculateNextReminderTime(SaveTaskDetail task, DateTimeOffset currentTime)
        {
            var nextTime = task.DateTime.AddDays(1);

            // Find the next day that matches the selected days
            while (!task.SelectedDays.Contains(nextTime.DayOfWeek))
            {
                nextTime = nextTime.AddDays(1);
            }

            return nextTime;
        }

        private async Task SendProactiveReminder(ConversationReference conversationReference, SaveTaskDetail task, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<Microsoft.Teams.Apps.App>();

            var reminderMessage = $"**Reminder for a scheduled task!**\n\n" +
                                $"Task title: {task.Title}\n\n" +
                                $"Task description: {task.Description}";

            await app.Send(conversationReference.Conversation.Id, reminderMessage);
        }
    }
}


