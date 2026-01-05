using Microsoft.Teams.Api.Entities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Api;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Microsoft.Teams.Apps.Activities.Invokes;
using Task = System.Threading.Tasks.Task;
using Microsoft.Teams.Api.Activities.Invokes;
using Microsoft.Teams.Api.AdaptiveCards;
using MessageActivity = Microsoft.Teams.Api.Activities.MessageActivity;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Clients;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Common;
using Microsoft.Teams.Cards;
using AdaptiveCard = Microsoft.Teams.Cards.AdaptiveCard;

namespace TargetedMessage.Controllers
{
    /// <summary>
    /// Personal Reminder Bot Controller
    /// Demonstrates targeted messaging for private reminders in channels and group chats.
    /// 
    /// Features:
    /// - Set reminders for yourself or other members
    /// - Parse natural language time expressions (e.g., "in 5 minutes")
    /// - Edit reminder content before delivery
    /// - Delete/cancel reminders
    /// - Works in both Teams channels (L1/L2) and group chats
    /// </summary>
    [TeamsController]
    public class ReminderController
    {
        private readonly Microsoft.Teams.Apps.App _app;
        private readonly IConfiguration _configuration;
        private readonly IHttpClient _httpClient;

        // Regex patterns for time parsing
        private static readonly Regex TimeExpressionPattern = new(
            @"in\s+(\d+)\s*(seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TimeExpressionRemovalPattern = new(
            @"in\s+\d+\s*(?:seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // In-memory storage for active reminders (use a database in production)
        private static readonly ConcurrentDictionary<string, ReminderInfo> _activeReminders = new();

        public ReminderController(Microsoft.Teams.Apps.App app, IConfiguration configuration, IHttpClient httpClient)
        {
            _app = app;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Reminder information stored for each active reminder
        /// </summary>
        private class ReminderInfo
        {
            public required string Id { get; set; }
            public required string ConversationId { get; set; }
            public required string ServiceUrl { get; set; }
            public required ConversationType ConversationType { get; set; }
            public required string TargetUserId { get; set; }
            public required string TargetUserName { get; set; }
            public required string CreatorId { get; set; }
            public required string CreatorName { get; set; }
            public required string ReminderText { get; set; }
            public required DateTime DueTime { get; set; }
            public string? MessageId { get; set; }
            public CancellationTokenSource? CancellationSource { get; set; }
        }

        /// <summary>
        /// Parsed reminder command result
        /// </summary>
        private class ParsedReminder
        {
            public string? TargetUserId { get; set; }
            public string? TargetUserName { get; set; }
            public bool IsSelfReminder { get; set; }
            public TimeSpan Delay { get; set; }
            public string ReminderText { get; set; } = "";
            public string? ErrorMessage { get; set; }
        }

        private static string StripBotMentions(MessageActivity activity, string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (activity.ChannelId != "msteams") return text;

            var entities = activity.Entities;
            var botId = activity.Recipient?.Id;

            if (string.IsNullOrEmpty(botId) || entities == null) return text;

            foreach (var entity in entities)
            {
                if (entity.Type == "mention" && entity is MentionEntity mentionEntity)
                {
                    if (mentionEntity.Mentioned?.Id == botId && !string.IsNullOrEmpty(mentionEntity.Text))
                    {
                        text = text.Replace(mentionEntity.Text, "").Trim();
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Extract mentioned user from activity entities (excluding bot mentions)
        /// </summary>
        private static (string? userId, string? userName) ExtractMentionedUser(MessageActivity activity)
        {
            var botId = activity.Recipient?.Id;
            var entities = activity.Entities;

            if (entities == null) return (null, null);

            foreach (var entity in entities)
            {
                if (entity.Type == "mention" && entity is MentionEntity mentionEntity)
                {
                    // Skip bot mentions
                    if (mentionEntity.Mentioned?.Id != botId)
                    {
                        return (mentionEntity.Mentioned?.Id, mentionEntity.Mentioned?.Name);
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Parse time expressions like "in 5 minutes", "in 1 hour", "in 30 seconds"
        /// </summary>
        private static TimeSpan? ParseTimeExpression(string text)
        {
            var match = TimeExpressionPattern.Match(text.ToLower());

            if (!match.Success) return null;

            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value.ToLower();

            // Match longer units first to avoid partial matches
            if (unit.StartsWith("second") || unit.StartsWith("sec") || unit == "s")
                return TimeSpan.FromSeconds(value);
            if (unit.StartsWith("minute") || unit.StartsWith("min") || unit == "m")
                return TimeSpan.FromMinutes(value);
            if (unit.StartsWith("hour") || unit.StartsWith("hr") || unit == "h")
                return TimeSpan.FromHours(value);

            return null;
        }

        /// <summary>
        /// Parse the remind command to extract target user, time, and reminder text
        /// Format: remind [@user|me] in [time] [message]
        /// Examples:
        ///   - remind me in 5 minutes to check email
        ///   - remind @John in 1 hour meeting starts
        ///   - remind me in 30 seconds test reminder
        /// </summary>
        private ParsedReminder ParseReminderCommand(MessageActivity activity, string commandText)
        {
            var result = new ParsedReminder();

            // Remove "remind" prefix
            var text = commandText.Trim();
            if (text.StartsWith("remind", StringComparison.OrdinalIgnoreCase))
            {
                text = text[6..].Trim();
            }

            // Check for "me" (self-reminder)
            if (text.StartsWith("me ", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("me,", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                result.IsSelfReminder = true;
                result.TargetUserId = activity.From?.Id;
                result.TargetUserName = activity.From?.Name ?? "You";
                text = text.Length > 2 ? text[2..].Trim().TrimStart(',').Trim() : "";
            }
            else
            {
                // Check for @mention
                var (userId, userName) = ExtractMentionedUser(activity);
                if (userId != null)
                {
                    result.TargetUserId = userId;
                    result.TargetUserName = userName ?? "User";
                    // Remove the mention text from the command
                    foreach (var entity in activity.Entities ?? [])
                    {
                        if (entity is MentionEntity mention && mention.Mentioned?.Id == userId)
                        {
                            text = text.Replace(mention.Text ?? "", "").Trim();
                            break;
                        }
                    }
                }
                else
                {
                    // Default to self if no target specified
                    result.IsSelfReminder = true;
                    result.TargetUserId = activity.From?.Id;
                    result.TargetUserName = activity.From?.Name ?? "You";
                }
            }

            // Parse time expression
            var delay = ParseTimeExpression(text);
            if (delay == null)
            {
                result.ErrorMessage = "Could not parse time. Use format like 'in 5 minutes', 'in 1 hour', or 'in 30 seconds'.";
                return result;
            }
            result.Delay = delay.Value;

            // Extract reminder text (everything after the time expression)
            var reminderText = TimeExpressionRemovalPattern.Replace(text, "").Trim();

            // Clean up common words
            reminderText = reminderText.TrimStart(',').Trim();
            if (reminderText.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
            {
                reminderText = reminderText[3..].Trim();
            }
            if (reminderText.StartsWith("that ", StringComparison.OrdinalIgnoreCase))
            {
                reminderText = reminderText[5..].Trim();
            }

            if (string.IsNullOrWhiteSpace(reminderText))
            {
                reminderText = "You have a reminder!";
            }

            result.ReminderText = reminderText;
            return result;
        }

        /// <summary>
        /// Convert string conversation type to enum
        /// </summary>
        private static ConversationType ParseConversationType(string? type)
        {
            return type?.ToLower() switch
            {
                "personal" => ConversationType.Personal,
                "groupchat" => ConversationType.GroupChat,
                "channel" => ConversationType.Channel,
                _ => ConversationType.Channel
            };
        }

        [Message]
        public async Task OnReminderMessage(IContext<MessageActivity> context)
        {
            var (log, api, activity, client) = context;

            if (string.IsNullOrEmpty(activity.Text)) return;

            var text = StripBotMentions(activity, activity.Text ?? "");
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts.Length > 0 ? parts[0].ToLower() : "";

            switch (cmd)
            {
                case "remind":
                    await HandleRemindCommand(context, text);
                    break;

                case "reminder-help":
                    await ShowReminderHelp(client);
                    break;

                case "my-reminders":
                    await ShowMyReminders(context);
                    break;

                case "cancel-reminder":
                    if (parts.Length > 1)
                    {
                        await CancelReminder(context, parts[1]);
                    }
                    else
                    {
                        await client.Send("Please specify the reminder ID to cancel. Use `my-reminders` to see your active reminders.");
                    }
                    break;
            }
        }

        private static async Task ShowReminderHelp(IContext.Client client)
        {
            await client.Send(string.Join("\n", new[]
            {
                "**Personal Reminder Bot - Help**",
                "",
                "**Set a Reminder:**",
                "- `remind me in 5 minutes to check email`",
                "- `remind me in 1 hour meeting starts`",
                "- `remind me in 30 seconds test`",
                "- `remind @John in 10 minutes review PR`",
                "",
                "**Supported Time Formats:**",
                "- Seconds: `30 seconds`, `30 secs`, `30s`",
                "- Minutes: `5 minutes`, `5 mins`, `5m`",
                "- Hours: `1 hour`, `2 hrs`, `1h`",
                "",
                "**Manage Reminders:**",
                "- `my-reminders` — View your active reminders",
                "- `cancel-reminder [id]` — Cancel a specific reminder",
                "",
                "**Features:**",
                "- Reminders are sent as **targeted messages** (only visible to recipient)",
                "- Works in both **channels** and **group chats**",
                "- Set reminders for yourself or mention others",
                "- Edit or dismiss reminders via card buttons"
            }));
        }

        private async Task HandleRemindCommand(IContext<MessageActivity> context, string commandText)
        {
            var (log, api, activity, client) = context;

            // Parse the reminder command
            var parsed = ParseReminderCommand(activity, commandText);

            if (parsed.ErrorMessage != null)
            {
                await client.Send($"{parsed.ErrorMessage}\n\nUse `reminder-help` for usage examples.");
                return;
            }

            if (string.IsNullOrEmpty(parsed.TargetUserId))
            {
                await client.Send("Could not determine who to remind. Use `remind me` or mention someone like `remind @John`.");
                return;
            }

            // Create reminder info
            var reminderId = Guid.NewGuid().ToString("N")[..8];
            var conversationId = activity.Conversation?.Id ?? "";
            var channelConversationId = conversationId.Split(';')[0];

            var reminder = new ReminderInfo
            {
                Id = reminderId,
                ConversationId = channelConversationId,
                ServiceUrl = activity.ServiceUrl ?? "",
                ConversationType = ParseConversationType(activity.Conversation?.Type),
                TargetUserId = parsed.TargetUserId,
                TargetUserName = parsed.TargetUserName ?? "User",
                CreatorId = activity.From?.Id ?? "",
                CreatorName = activity.From?.Name ?? "Someone",
                ReminderText = parsed.ReminderText,
                DueTime = DateTime.UtcNow.Add(parsed.Delay),
                CancellationSource = new CancellationTokenSource()
            };

            _activeReminders[reminderId] = reminder;

            // Send confirmation card as targeted message (only visible to the creator)
            var confirmationCard = CreateReminderConfirmationCard(reminder, parsed.Delay);
            confirmationCard.Recipient = new Account { Id = activity.From?.Id, Name = activity.From?.Name };

            await _app.Send(
                channelConversationId,
                confirmationCard,
                reminder.ConversationType,
                activity.ServiceUrl ?? "",
                isTargeted: true);

            log.Info($"[REMINDER] Created reminder {reminderId} for {parsed.TargetUserName} in {parsed.Delay.TotalSeconds} seconds");

            // Schedule the reminder delivery as a fire-and-forget background task
            // Don't pass the request cancellation token - it will cancel when the HTTP request ends
            _ = DeliverReminderAsync(reminder);
        }

        private async Task DeliverReminderAsync(ReminderInfo reminder)
        {
            try
            {
                // Only use the reminder's own cancellation source (not the request's token)
                var cancellationToken = reminder.CancellationSource?.Token ?? CancellationToken.None;

                var delay = reminder.DueTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }

                // Check if reminder was cancelled
                if (!_activeReminders.ContainsKey(reminder.Id))
                {
                    Console.WriteLine($"[REMINDER] Reminder {reminder.Id} was cancelled, skipping delivery");
                    return;
                }

                // Create the reminder card with edit/dismiss options
                var reminderCard = CreateReminderDeliveryCard(reminder);

                // Send targeted reminder to the recipient
                var message = reminderCard;
                message.Recipient = new Account { Id = reminder.TargetUserId, Name = reminder.TargetUserName };

                await _app.Send(
                    reminder.ConversationId,
                    message,
                    reminder.ConversationType,
                    reminder.ServiceUrl,
                    isTargeted: true);

                Console.WriteLine($"[REMINDER] Delivered reminder {reminder.Id} to {reminder.TargetUserName}");

                // Remove from active reminders
                _activeReminders.TryRemove(reminder.Id, out _);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[REMINDER] Reminder {reminder.Id} was cancelled");
                _activeReminders.TryRemove(reminder.Id, out _);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REMINDER] Failed to deliver reminder {reminder.Id}: {ex.Message}");
                _activeReminders.TryRemove(reminder.Id, out _);
            }
        }

        private async Task ShowMyReminders(IContext<MessageActivity> context)
        {
            var (log, api, activity, client) = context;
            var userId = activity.From?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                await client.Send("Could not determine your user ID.");
                return;
            }

            var myReminders = _activeReminders.Values
                .Where(r => r.TargetUserId == userId || r.CreatorId == userId)
                .OrderBy(r => r.DueTime)
                .ToList();

            if (myReminders.Count == 0)
            {
                await client.Send("You have no active reminders.");
                return;
            }

            var reminderList = string.Join("\n", myReminders.Select(r =>
            {
                var timeLeft = r.DueTime - DateTime.UtcNow;
                var timeStr = timeLeft.TotalSeconds > 0
                    ? $"in {FormatTimeSpan(timeLeft)}"
                    : "overdue";
                var target = r.TargetUserId == userId ? "you" : r.TargetUserName;
                return $"• **{r.Id}**: \"{r.ReminderText}\" for {target} ({timeStr})";
            }));

            await client.Send($"**Your Active Reminders:**\n\n{reminderList}\n\nUse `cancel-reminder [id]` to cancel a reminder.");
        }

        private async Task CancelReminder(IContext<MessageActivity> context, string reminderId)
        {
            var (log, api, activity, client) = context;
            var userId = activity.From?.Id;

            if (_activeReminders.TryGetValue(reminderId, out var reminder))
            {
                // Only allow creator or target to cancel
                if (reminder.CreatorId == userId || reminder.TargetUserId == userId)
                    {
                        reminder.CancellationSource?.Cancel();
                        _activeReminders.TryRemove(reminderId, out _);
                        await client.Send($"Reminder **{reminderId}** has been cancelled.");
                        log.Info($"[REMINDER] Reminder {reminderId} cancelled by {activity.From?.Name}");
                    }
                    else
                    {
                        await client.Send("You can only cancel reminders you created or are assigned to you.");
                    }
                }
                else
                {
                    await client.Send($"Reminder **{reminderId}** not found or already completed.");
            }
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
            return $"{(int)ts.TotalSeconds}s";
        }

        private static MessageActivity CreateReminderConfirmationCard(ReminderInfo reminder, TimeSpan delay)
        {
            var targetDisplay = reminder.CreatorId == reminder.TargetUserId
                ? "yourself"
                : reminder.TargetUserName;

            // Build card using Teams SDK typed card builder API
            var card = new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Version = new Microsoft.Teams.Cards.Version("1.5"),
                Body =
                [
                    new TextBlock("Reminder Set!")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Color = TextColor.Good
                    },
                    new FactSet
                    {
                        Facts =
                        [
                            new Fact("Reminder:", reminder.ReminderText),
                            new Fact("For:", targetDisplay),
                            new Fact("In:", FormatTimeSpan(delay)),
                            new Fact("ID:", reminder.Id)
                        ]
                    },
                    new TextBlock("The reminder will be sent as a private targeted message.")
                    {
                        Size = TextSize.Small,
                        IsSubtle = true,
                        Wrap = true
                    }
                ],
                Actions =
                [
                    new ExecuteAction
                    {
                        Title = "Cancel Reminder",
                        Data = new Union<string, SubmitActionData>(new SubmitActionData
                        {
                            NonSchemaProperties = new Dictionary<string, object?>
                            {
                                { "action", "cancel_reminder" },
                                { "reminderId", reminder.Id }
                            }
                        })
                    }
                ]
            };

            return new MessageActivity().AddAttachment(card);
        }

        private static MessageActivity CreateReminderDeliveryCard(ReminderInfo reminder)
        {
            var fromDisplay = reminder.CreatorId == reminder.TargetUserId
                ? "yourself"
                : reminder.CreatorName;

            // Build card using Teams SDK typed card builder API
            var card = new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Version = new Microsoft.Teams.Cards.Version("1.5"),
                Body =
                [
                    new TextBlock("Reminder")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Large,
                        Color = TextColor.Accent
                    },
                    new TextBlock(reminder.ReminderText)
                    {
                        Wrap = true,
                        Size = TextSize.Medium
                    },
                    new TextBlock($"Set by {fromDisplay}")
                    {
                        Size = TextSize.Small,
                        IsSubtle = true
                    }
                ],
                Actions =
                [
                    new ExecuteAction
                    {
                        Title = "Dismiss",
                        Data = new Union<string, SubmitActionData>(new SubmitActionData
                        {
                            NonSchemaProperties = new Dictionary<string, object?>
                            {
                                { "action", "dismiss_reminder" },
                                { "reminderId", reminder.Id }
                            }
                        })
                    },
                    new ExecuteAction
                    {
                        Title = "Snooze 5 min",
                        Data = new Union<string, SubmitActionData>(new SubmitActionData
                        {
                            NonSchemaProperties = new Dictionary<string, object?>
                            {
                                { "action", "snooze_reminder" },
                                { "reminderId", reminder.Id },
                                { "reminderText", reminder.ReminderText },
                                { "snoozeMinutes", "5" }
                            }
                        })
                    }
                ]
            };

            return new MessageActivity().AddAttachment(card);
        }

        [Microsoft.Teams.Apps.Activities.Invokes.AdaptiveCard.Action]
        public async Task<ActionResponse> OnReminderCardAction(
            [Context] AdaptiveCards.ActionActivity activity,
            [Context] IContext.Client client,
            [Context] ApiClient api,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var data = activity.Value?.Action?.Data;
            if (data == null)
            {
                return new ActionResponse.Message("No data specified") { StatusCode = 400 };
            }

            string? GetValue(string key) => data.TryGetValue(key, out var val)
                ? (val is JsonElement el ? el.GetString() : val?.ToString())
                : null;

            var action = GetValue("action");
            var reminderId = GetValue("reminderId");

            log.Info($"[REMINDER_ACTION] Action: {action}, ReminderId: {reminderId}");

            switch (action)
            {
                case "cancel_reminder":
                if (!string.IsNullOrEmpty(reminderId) && _activeReminders.TryRemove(reminderId, out var cancelled))
                {
                    cancelled.CancellationSource?.Cancel();
                    log.Info($"[REMINDER] Cancelled reminder {reminderId}");
                    return new ActionResponse.Message("Reminder cancelled!") { StatusCode = 200 };
                }
                    return new ActionResponse.Message("Reminder not found or already completed.") { StatusCode = 200 };

                case "dismiss_reminder":
                    {
                        // Delete the reminder card
                        var activityId = activity.ReplyToId;
                        var conversationId = activity.Conversation?.Id?.Split(';')[0] ?? "";

                        if (!string.IsNullOrEmpty(activityId))
                        {
                            try
                            {
                                await api.Conversations.Activities.DeleteAsync(conversationId, activityId, isTargeted: true);
                                log.Info($"[REMINDER] Dismissed reminder card {activityId}");
                            }
                            catch (Exception ex)
                            {
                                log.Error($"[REMINDER] Failed to delete reminder card: {ex.Message}");
                                    }
                                }
                                return new ActionResponse.Message("Reminder dismissed!") { StatusCode = 200 };
                    }

                case "snooze_reminder":
                    {
                        var reminderText = GetValue("reminderText") ?? "Snoozed reminder";
                        var snoozeMinutesStr = GetValue("snoozeMinutes") ?? "5";
                        var snoozeMinutes = int.TryParse(snoozeMinutesStr, out var mins) ? mins : 5;

                        // Create a new reminder
                        var newReminderId = Guid.NewGuid().ToString("N")[..8];
                        var conversationId = activity.Conversation?.Id?.Split(';')[0] ?? "";

                        var newReminder = new ReminderInfo
                        {
                            Id = newReminderId,
                            ConversationId = conversationId,
                            ServiceUrl = activity.ServiceUrl ?? "",
                            ConversationType = ParseConversationType(activity.Conversation?.Type),
                            TargetUserId = activity.From?.Id ?? "",
                            TargetUserName = activity.From?.Name ?? "User",
                            CreatorId = activity.From?.Id ?? "",
                            CreatorName = activity.From?.Name ?? "User",
                            ReminderText = reminderText,
                            DueTime = DateTime.UtcNow.AddMinutes(snoozeMinutes),
                            CancellationSource = new CancellationTokenSource()
                        };

                        _activeReminders[newReminderId] = newReminder;

                        // Update the card to show snooze confirmation
                        var activityId = activity.ReplyToId;
                        if (!string.IsNullOrEmpty(activityId))
                        {
                            var updatedCard = CreateSnoozeConfirmationCard(newReminder, snoozeMinutes);
                            updatedCard.Recipient = activity.From;

                            try
                            {
                                await api.Conversations.Activities.UpdateAsync(
                                    conversationId,
                                    activityId,
                                    updatedCard,
                                    isTargeted: true);
                            }
                            catch (Exception ex)
                            {
                                log.Error($"[REMINDER] Failed to update card for snooze: {ex.Message}");
                            }
                        }

                        // Schedule delivery
                        _ = DeliverReminderAsync(newReminder);

                        log.Info($"[REMINDER] Snoozed reminder, new ID: {newReminderId}, delay: {snoozeMinutes} minutes");
                        return new ActionResponse.Message($"Snoozed for {snoozeMinutes} minutes!") { StatusCode = 200 };
                    }

                default:
                    return new ActionResponse.Message("Unknown action") { StatusCode = 400 };
            }
        }

        private static MessageActivity CreateSnoozeConfirmationCard(ReminderInfo reminder, int snoozeMinutes)
        {
            // Build card using Teams SDK typed card builder API
            var card = new AdaptiveCard
            {
                Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                Version = new Microsoft.Teams.Cards.Version("1.5"),
                Body =
                [
                    new TextBlock("Snoozed!")
                    {
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Color = TextColor.Accent
                    },
                    new TextBlock(reminder.ReminderText)
                    {
                        Wrap = true
                    },
                    new TextBlock($"Will remind you again in {snoozeMinutes} minutes.")
                    {
                        Size = TextSize.Small,
                        IsSubtle = true
                    }
                ],
                Actions =
                [
                    new ExecuteAction
                    {
                        Title = "Cancel",
                        Data = new Union<string, SubmitActionData>(new SubmitActionData
                        {
                            NonSchemaProperties = new Dictionary<string, object?>
                            {
                                { "action", "cancel_reminder" },
                                { "reminderId", reminder.Id }
                            }
                        })
                    }
                ]
            };

            return new MessageActivity().AddAttachment(card);
        }
    }
}
