// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable ExperimentalTeamsTargeted
#pragma warning disable ExperimentalTeamsSuggestedAction

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Schema;
using Microsoft.Teams.Apps.Schema.Entities;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;
using AdaptiveCard = Microsoft.Teams.Cards.AdaptiveCard;

// Initialize Teams Agent App
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTeamsBotApplication();
var webApp = builder.Build();
TeamsBotApplication teamsApp = webApp.UseTeamsBotApplication();

// In-memory store for active reminders
var activeReminders = new ConcurrentDictionary<string, ReminderInfo>();

var timePattern = new Regex(@"in\s+(\d+)\s*(seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)\b", RegexOptions.IgnoreCase);

(int DelayMs, string Label)? ParseTimeExpression(string text)
{
    var match = timePattern.Match(text.ToLower());
    if (!match.Success) return null;

    var value = int.Parse(match.Groups[1].Value);
    var unit = match.Groups[2].Value.ToLower();

    if (unit.StartsWith("second") || unit.StartsWith("sec") || unit == "s")
        return (value * 1000, $"{value} second{(value != 1 ? "s" : "")}");
    if (unit.StartsWith("minute") || unit.StartsWith("min") || unit == "m")
        return (value * 60_000, $"{value} minute{(value != 1 ? "s" : "")}");
    if (unit.StartsWith("hour") || unit.StartsWith("hr") || unit == "h")
        return (value * 3_600_000, $"{value} hour{(value != 1 ? "s" : "")}");

    return null;
}

string FormatTimeSpan(int ms)
{
    var totalSecs = (int)Math.Round(ms / 1000.0);
    if (totalSecs >= 3600) return $"{totalSecs / 3600}h {(totalSecs % 3600) / 60}m";
    if (totalSecs >= 60) return $"{totalSecs / 60}m {totalSecs % 60}s";
    return $"{totalSecs}s";
}

JsonElement ToCardJson(AdaptiveCard card) => JsonSerializer.Deserialize<JsonElement>(card.Serialize());

SuggestedActions BuildSuggestedCommands(string? userId, params (string Title, string Value)[] items)
{
   return new SuggestedActions
    {
        To = string.IsNullOrEmpty(userId) ? [] : [userId],
        Actions = [.. items.Select(i => new SuggestedAction(
            ActionTypes.Submit,
            i.Title,
            new JsonObject { ["command"] = i.Value }))]
    };
}

string StripBotMention(MessageActivity msg, string botId)
{
    var text = msg.Text ?? "";
    foreach (var mention in msg.GetMentions())
    {
        if (mention.Mentioned?.Id == botId && mention.Text != null)
        {
            text = text.Replace(mention.Text, "").Trim();
        }
    }
    return text;
}

(string? UserId, string? UserName) ExtractMentionedUser(TeamsActivity msg, string botId)
{
    foreach (var mention in msg.GetMentions())
    {
        if (mention.Mentioned != null && mention.Mentioned.Id != botId)
        {
            return (mention.Mentioned.Id, mention.Mentioned.Name ?? "User");
        }
    }
    return (null, null);
}

ParsedReminder ParseReminderCommand(TeamsActivity msg, string commandText)
{
    var botId = msg.Recipient?.Id ?? "";
    var text = commandText.Trim();

    // Remove "remind" prefix
    if (text.StartsWith("remind", StringComparison.OrdinalIgnoreCase))
        text = text[6..].Trim();

    string targetUserId;
    string targetUserName;
    var isSelfReminder = false;

    // Check for "me" (self-reminder)
    if (Regex.IsMatch(text, @"^me(\s|,|$)", RegexOptions.IgnoreCase))
    {
        isSelfReminder = true;
        targetUserId = msg.From?.Id ?? "";
        targetUserName = msg.From?.Name ?? "You";
        text = text[2..].Trim().TrimStart(',').Trim();
    }
    else
    {
        // Check for @mention of a target user
        var (userId, userName) = ExtractMentionedUser(msg, botId);
        if (userId != null)
        {
            targetUserId = userId;
            targetUserName = userName!;
            // Remove the target user's mention tag from text
            foreach (var mention in msg.GetMentions())
            {
                if (mention.Mentioned?.Id == userId && mention.Text != null)
                {
                    text = text.Replace(mention.Text, "").Trim();
                }
            }
        }
        else
        {
            // Default to self if no target specified
            isSelfReminder = true;
            targetUserId = msg.From?.Id ?? "";
            targetUserName = msg.From?.Name ?? "You";
        }
    }

    // Parse time expression
    var time = ParseTimeExpression(text);
    if (time == null)
    {
        return new ParsedReminder { Error = "Could not parse time. Use format like 'in 5 minutes', 'in 1 hour', or 'in 30 seconds'." };
    }

    // Extract reminder text (everything after the time expression)
    var reminderText = timePattern.Replace(text, "").Trim();
    reminderText = reminderText.TrimStart(',').Trim();
    if (reminderText.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
        reminderText = reminderText[3..].Trim();
    if (reminderText.StartsWith("that ", StringComparison.OrdinalIgnoreCase))
        reminderText = reminderText[5..].Trim();
    if (string.IsNullOrEmpty(reminderText))
        reminderText = "You have a reminder!";

    /* Use preferred LLM to parse natural language time expressions */
    /* var nlpParsed = await llmClient.ParseTimeAsync(text); */

    return new ParsedReminder
    {
        TargetUserId = targetUserId,
        TargetUserName = targetUserName,
        IsSelfReminder = isSelfReminder,
        ReminderText = reminderText,
        DelayMs = time.Value.DelayMs
    };
}


AdaptiveCard CreateConfirmationCard(ReminderInfo reminder, int delayMs)
{
    var targetDisplay = reminder.CreatorId == reminder.TargetUserId ? "yourself" : reminder.TargetUserName;
    return new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Reminder Set!") { Weight = TextWeight.Bolder, Size = TextSize.Medium, Color = TextColor.Good },
            new FactSet
            {
                Facts = new List<Fact>
                {
                    new Fact("Reminder:", reminder.ReminderText),
                    new Fact("For:", targetDisplay),
                    new Fact("In:", FormatTimeSpan(delayMs)),
                    new Fact("ID:", reminder.Id)
                }
            },
            new TextBlock("This is a targeted message — only you can see this.") { Size = TextSize.Small, IsSubtle = true, Wrap = true }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new ExecuteAction
            {
                Title = "Cancel Reminder",
                Verb = "cancel_reminder",
                Data = new Union<string, SubmitActionData>(new SubmitActionData
                {
                    NonSchemaProperties = new Dictionary<string, object?>
                    {
                        { "action", "cancel_reminder" },
                        { "reminderId", reminder.Id }
                    }
                })
            }
        }
    };
}

AdaptiveCard CreateDeliveryCard(ReminderInfo reminder)
{
    var fromDisplay = reminder.CreatorId == reminder.TargetUserId ? "yourself" : reminder.CreatorName;
    return new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Reminder") { Weight = TextWeight.Bolder, Size = TextSize.Large, Color = TextColor.Accent },
            new TextBlock(reminder.ReminderText) { Wrap = true, Size = TextSize.Medium },
            new TextBlock($"Set by {fromDisplay}") { Size = TextSize.Small, IsSubtle = true }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new ExecuteAction
            {
                Title = "Dismiss",
                Verb = "dismiss_reminder",
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
                Verb = "snooze_reminder",
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
        }
    };
}

AdaptiveCard CreateSnoozeConfirmationCard(ReminderInfo reminder, int snoozeMinutes)
{
    return new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Snoozed!") { Weight = TextWeight.Bolder, Size = TextSize.Medium, Color = TextColor.Accent },
            new TextBlock(reminder.ReminderText) { Wrap = true },
            new TextBlock($"Will remind you again in {snoozeMinutes} minutes.") { Size = TextSize.Small, IsSubtle = true }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new ExecuteAction
            {
                Title = "Cancel",
                Verb = "cancel_reminder",
                Data = new Union<string, SubmitActionData>(new SubmitActionData
                {
                    NonSchemaProperties = new Dictionary<string, object?>
                    {
                        { "action", "cancel_reminder" },
                        { "reminderId", reminder.Id }
                    }
                })
            }
        }
    };
}


async Task DeliverReminder(ReminderInfo reminder)
{
    // Check if reminder was cancelled
    if (!activeReminders.ContainsKey(reminder.Id))
    {
        Console.WriteLine($"[REMINDER] Reminder {reminder.Id} was cancelled, skipping delivery");
        return;
    }

    try
    {
        var card = CreateDeliveryCard(reminder);
        var recipient = new TeamsChannelAccount { Id = reminder.TargetUserId, Name = reminder.TargetUserName };

        // Send targeted reminder proactively — only the recipient can see it
        await teamsApp.SendAsync(
            reminder.ConversationId,
            new MessageActivityInput()
                .WithAdaptiveCardAttachment(ToCardJson(card))
                .WithRecipient(recipient, isTargeted: true),
            reminder.ServiceUrl);

        Console.WriteLine($"[REMINDER] Delivered reminder {reminder.Id} to {reminder.TargetUserName}");
        activeReminders.TryRemove(reminder.Id, out _);
    }
    catch (Exception error)
    {
        Console.WriteLine($"[REMINDER] Failed to deliver reminder {reminder.Id}: {error.Message}");
        activeReminders.TryRemove(reminder.Id, out _);
    }
}


async Task HandleRemindCommand<T>(Context<T> context, string commandText, bool isTargeted, string? targetedMessageId, CancellationToken cancellationToken) where T : TeamsActivity
{
    var activity = context.Activity;
    var parsed = ParseReminderCommand(activity, commandText);

    if (parsed.Error != null)
    {
        await context.SendAsync($"{parsed.Error}\n\nUse `reminder-help` for usage examples.", cancellationToken);
        return;
    }

    if (string.IsNullOrEmpty(parsed.TargetUserId))
    {
        await context.SendAsync("Could not determine who to remind. Use `remind me` or mention someone like `remind @John`.", cancellationToken);
        return;
    }

    // Create reminder
    var reminderId = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x")[^4..]}{Random.Shared.Next(0x10000):x4}";
    var convId = (activity.Conversation?.Id ?? "").Split(';')[0];
    var cts = new CancellationTokenSource();

    var reminder = new ReminderInfo
    {
        Id = reminderId,
        ConversationId = convId,
        ServiceUrl = activity.ServiceUrl,
        TargetUserId = parsed.TargetUserId!,
        TargetUserName = parsed.TargetUserName!,
        CreatorId = activity.From?.Id ?? "",
        CreatorName = activity.From?.Name ?? "Someone",
        ReminderText = parsed.ReminderText!,
        DueTime = DateTimeOffset.UtcNow.AddMilliseconds(parsed.DelayMs).ToUnixTimeMilliseconds(),
        Cts = cts
    };

    activeReminders[reminderId] = reminder;

    // Schedule delivery
    _ = Task.Run(async () =>
    {
        try
        {
            await Task.Delay(parsed.DelayMs, cts.Token);
            await DeliverReminder(reminder);
        }
        catch (TaskCanceledException) { }
    });

    try
    {
        // Send targeted confirmation card to the creator — only they can see it
        var card = CreateConfirmationCard(reminder, parsed.DelayMs);

        var response = new MessageActivityInput()
            .WithAdaptiveCardAttachment(ToCardJson(card));

        if (isTargeted && targetedMessageId != null)
        {
            response.WithTargetedMessageInfo(targetedMessageId);
        }
        response.WithRecipient(activity.From!, isTargeted: true);

        await context.SendAsync(response, cancellationToken);

        Console.WriteLine($"[REMINDER] Created reminder {reminderId} for {parsed.TargetUserName} in {parsed.DelayMs / 1000} seconds");
    }
    catch (Exception error)
    {
        Console.WriteLine($"[REMINDER] Error sending confirmation: {error.Message}");
        activeReminders.TryRemove(reminderId, out _);
        cts.Cancel();
    }
}

async Task ShowMyReminders<T>(Context<T> context, bool isTargeted, string? targetedMessageId, CancellationToken cancellationToken) where T : TeamsActivity
{
    var activity = context.Activity;
    var userId = activity.From?.Id;
    if (string.IsNullOrEmpty(userId))
    {
        await context.SendAsync("Could not determine your user ID.", cancellationToken);
        return;
    }

    var sender = activity.From!;

    var myReminders = activeReminders.Values
        .Where(r => r.TargetUserId == userId || r.CreatorId == userId)
        .OrderBy(r => r.DueTime)
        .ToList();

    if (myReminders.Count == 0)
    {
        var emptyResponse = new MessageActivityInput()
            .WithText("You have no active reminders.")
            .WithSuggestedActions(BuildSuggestedCommands(
                userId,
                ("Remind me in 5 minutes test", "remind me in 5 minutes test"),
                ("Remind me in 1 hour meeting", "remind me in 1 hour meeting"),
                ("Show help", "reminder-help")));

        if (isTargeted && targetedMessageId != null)
        {
            emptyResponse.WithTargetedMessageInfo(targetedMessageId);
            emptyResponse.WithRecipient(sender, isTargeted: true);
        }

        await context.SendAsync(emptyResponse, cancellationToken);
        return;
    }

    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var lines = myReminders.Select(r =>
    {
        var timeLeft = r.DueTime - now;
        var timeStr = timeLeft > 0 ? $"in {FormatTimeSpan((int)timeLeft)}" : "overdue";
        var target = r.CreatorId == r.TargetUserId ? "yourself" : r.TargetUserName;
        return $"- **{r.Id}**: \"{r.ReminderText}\" for {target} ({timeStr})";
    });

    var list = string.Join("\n", lines);
    var listResponse = new MessageActivityInput()
        .WithText($"**Your Active Reminders:**\n\n{list}\n\nUse `cancel-reminder [id]` to cancel a reminder.");

    if (isTargeted && targetedMessageId != null)
    {
        listResponse.WithTargetedMessageInfo(targetedMessageId);
        listResponse.WithRecipient(sender, isTargeted: true);
    }

    await context.SendAsync(listResponse, cancellationToken);
}

async Task CancelReminder(Context<MessageActivity> context, string reminderId, bool isTargeted, string? targetedMessageId, CancellationToken cancellationToken)
{
    var activity = context.Activity;
    var userId = activity.From?.Id;
    var sender = activity.From!;

    if (string.IsNullOrEmpty(reminderId))
    {
        var noIdResponse = new MessageActivityInput()
            .WithText("Please specify a reminder ID. Use `my-reminders` to see your active reminders.");

        if (isTargeted && targetedMessageId != null)
        {
            noIdResponse.WithTargetedMessageInfo(targetedMessageId);
            noIdResponse.WithRecipient(sender, isTargeted: true);
        }

        await context.SendAsync(noIdResponse, cancellationToken);
        return;
    }

    if (!activeReminders.TryGetValue(reminderId, out var reminder))
    {
        var notFoundResponse = new MessageActivityInput()
            .WithText($"Reminder **{reminderId}** not found or already completed.");

        if (isTargeted && targetedMessageId != null)
        {
            notFoundResponse.WithTargetedMessageInfo(targetedMessageId);
            notFoundResponse.WithRecipient(sender, isTargeted: true);
        }

        await context.SendAsync(notFoundResponse, cancellationToken);
        return;
    }

    // Only allow creator or target to cancel
    if (reminder.CreatorId == userId || reminder.TargetUserId == userId)
    {
        reminder.Cts.Cancel();
        activeReminders.TryRemove(reminderId, out _);

        var cancelledResponse = new MessageActivityInput()
            .WithText($"Reminder **{reminderId}** has been cancelled.");

        if (isTargeted && targetedMessageId != null)
        {
            cancelledResponse.WithTargetedMessageInfo(targetedMessageId);
            cancelledResponse.WithRecipient(sender, isTargeted: true);
        }

        await context.SendAsync(cancelledResponse, cancellationToken);
        Console.WriteLine($"[REMINDER] Reminder {reminderId} cancelled by {activity.From?.Name}");
    }
    else
    {
        var deniedResponse = new MessageActivityInput()
            .WithText("You can only cancel reminders you created or are assigned to you.");

        if (isTargeted && targetedMessageId != null)
        {
            deniedResponse.WithTargetedMessageInfo(targetedMessageId);
            deniedResponse.WithRecipient(sender, isTargeted: true);
        }

        await context.SendAsync(deniedResponse, cancellationToken);
    }
}

async Task ShowHelp<T>(Context<T> context, CancellationToken cancellationToken) where T : TeamsActivity
{
    var helpText =
        "**Personal Reminder Bot - Help**\n\n" +
        "**Set a Reminder:**\n" +
        "- `remind me in 5 minutes to check email`\n" +
        "- `remind me in 1 hour meeting starts`\n" +
        "- `remind me in 30 seconds test`\n" +
        "**Supported Time Formats:**\n" +
        "- Seconds: `30 seconds`, `30 secs`, `30s`\n" +
        "- Minutes: `5 minutes`, `5 mins`, `5m`\n" +
        "- Hours: `1 hour`, `2 hrs`, `1h`\n\n" +
        "**Manage Reminders:**\n" +
        "- `my-reminders` — View your active reminders\n" +
        "- `cancel-reminder [id]` — Cancel a specific reminder\n" +
        "- `reminder-help` — Show this help message\n\n" +
        "**How It Works:**\n" +
        "- Reminders are delivered as **targeted messages** (only the recipient can see them)\n" +
        "- Works in both **channels** and **group chats**\n" +
        "- Set reminders for yourself or mention others\n" +
        "- Dismiss or snooze reminders via card buttons\n\n" +
        "**Reactions:**\n" +
        "- `add-reaction [type]` — Bot adds a reaction to your message\n" +
        "- `remove-reaction [type]` — Bot removes a reaction from your message\n" +
        "- React to any bot message and the bot will acknowledge it!\n\n" +
        "**Supported Reaction Types:**\n" +
        "- `like` \ud83d\udc4d, `heart` \u2764\ufe0f, `1f440_eyes` \ud83d\udc40, `2705_whiteheavycheckmark` \u2705, `launch` \ud83d\ude80, `1f4cc_pushpin` \ud83d\udccc";

    var activity = context.Activity;

    var helpResponse = new MessageActivityInput()
        .WithText(helpText)
        .WithSuggestedActions(BuildSuggestedCommands(
            activity.From?.Id,
            ("Set a 30-second test reminder", "remind me in 30 seconds test"),
            ("Set a 5-minute reminder", "remind me in 5 minutes check email"),
            ("My reminders", "my-reminders")));

    helpResponse.WithRecipient(activity.From!, isTargeted: true);

    await context.SendAsync(helpResponse, cancellationToken);
}


async Task HandleAddReaction(Context<MessageActivity> context, string commandText, CancellationToken cancellationToken)
{
    var activity = context.Activity;
    var reactionType = Regex.Replace(commandText, @"^add-reaction\s*", "", RegexOptions.IgnoreCase).Trim();

    if (string.IsNullOrEmpty(reactionType))
    {
        await context.SendAsync("Please specify a reaction type. Example: `add-reaction like`\n\nSupported types: `like`, `heart`, `1f440_eyes`, `2705_whiteheavycheckmark`, `launch`, `1f4cc_pushpin`", cancellationToken);
        return;
    }

    try
    {
        await context.Api.Conversations.AddReactionAsync(
            activity.Conversation!.Id,
            activity.Id!,
            new ReactionType("1f44b_wavinghand"),
            cancellationToken: cancellationToken);
        await context.SendAsync($"Added a **{reactionType}** reaction to your message!", cancellationToken);
        Console.WriteLine($"[REACTION] Added {reactionType} reaction to message {activity.Id}");
    }
    catch (Exception error)
    {
        Console.WriteLine($"[REACTION] Failed to add reaction: {error.Message}");
        await context.SendAsync("Sorry, I had trouble adding that reaction.", cancellationToken);
    }
}

async Task HandleRemoveReaction(Context<MessageActivity> context, string commandText, CancellationToken cancellationToken)
{
    var activity = context.Activity;
    var reactionType = Regex.Replace(commandText, @"^remove-reaction\s*", "", RegexOptions.IgnoreCase).Trim();

    if (string.IsNullOrEmpty(reactionType))
    {
        await context.SendAsync("Please specify a reaction type. Example: `remove-reaction like`", cancellationToken);
        return;
    }

    try
    {
        await context.Api.Conversations.DeleteReactionAsync(
            activity.Conversation!.Id,
            activity.Id!,
            new ReactionType(reactionType),
            cancellationToken: cancellationToken);
        await context.SendAsync($"Removed the **{reactionType}** reaction from your message!", cancellationToken);
        Console.WriteLine($"[REACTION] Removed {reactionType} reaction from message {activity.Id}");
    }
    catch (Exception error)
    {
        Console.WriteLine($"[REACTION] Failed to remove reaction: {error.Message}");
        await context.SendAsync("Sorry, I had trouble removing that reaction.", cancellationToken);
    }
}

// --- Event Handlers ---

// Handles incoming messages and routes to appropriate functions
teamsApp.OnMessage(async (context, cancellationToken) =>
{
    var msg = context.Activity;
    if (string.IsNullOrEmpty(msg.Text)) return;

    // Check if this is a targeted message (TM) from the user via slash command
    var isTargeted = context.Activity.Recipient?.IsTargeted == true;
    // Always capture the incoming message ID for prompt preview
    var targetedMessageId = msg.Id;
    if (isTargeted)
    {
        Console.WriteLine($"[TM] Received targeted message from {msg.From?.Name ?? "unknown"}");
    }

    // Strip bot mention from message text
    var botId = msg.Recipient?.Id ?? "";
    var text = StripBotMention(msg, botId).Trim();

    // Route commands
    var lower = text.ToLower();

    if (lower == "reminder-help" || lower == "help")
    {
        await ShowHelp(context, cancellationToken);
    }
    else if (lower.StartsWith("remind"))
    {
        await HandleRemindCommand(context, text, isTargeted, targetedMessageId, cancellationToken);
    }
    else if (lower == "my-reminders")
    {
        await ShowMyReminders(context, isTargeted, targetedMessageId, cancellationToken);
    }
    else if (lower.StartsWith("cancel-reminder"))
    {
        var reminderId = Regex.Replace(text, @"^cancel-reminder\s*", "", RegexOptions.IgnoreCase).Trim();
        await CancelReminder(context, reminderId, isTargeted, targetedMessageId, cancellationToken);
    }
    else if (lower.StartsWith("add-reaction"))
    {
        await HandleAddReaction(context, text, cancellationToken);
    }
    else if (lower.StartsWith("remove-reaction"))
    {
        await HandleRemoveReaction(context, text, cancellationToken);
    }
    else
    {
        /* Use preferred LLM to get summarized answer */
        /* var llmResponse = await llmClient.GetCompletionAsync(text); */
        /* await context.SendAsync(llmResponse, cancellationToken); */
        var fallbackResponse = new MessageActivityInput()
            .WithText("Use `reminder-help` to see available commands.")
            .WithSuggestedActions(BuildSuggestedCommands(
                msg.From?.Id,
                ("Show help", "reminder-help"),
                ("Remind me in 5 minutes test", "remind me in 5 minutes test"),
                ("My reminders", "my-reminders")))
            .AddAIGenerated();

        if (isTargeted && targetedMessageId != null)
        {
            fallbackResponse.WithTargetedMessageInfo(targetedMessageId);
            fallbackResponse.WithRecipient(msg.From!, isTargeted: true);
        }

        await context.SendAsync(fallbackResponse, cancellationToken);
    }
});

// Handle suggestedActions/submit invoke when user clicks an Action.Submit suggested action chip
teamsApp.OnSuggestedActionSubmit(async (context, cancellationToken) =>
{
    var value = context.Activity.Value;
    var command = value?["command"]?.GetValue<string>();

    Console.WriteLine($"[SUGGESTED_ACTION_SUBMIT] value={value}");

    if (string.IsNullOrEmpty(command))
    {
        await context.SendAsync("No command specified.", cancellationToken);
        return InvokeResponse.Ok();
    }

    // Route the command the same way as regular messages
    var lower = command.ToLower();
    if (lower == "reminder-help" || lower == "help")
    {
        await ShowHelp(context, cancellationToken);
    }
    else if (lower.StartsWith("remind"))
    {
        await HandleRemindCommand(context, command, isTargeted: false, targetedMessageId: null, cancellationToken);
    }
    else if (lower == "my-reminders")
    {
        await ShowMyReminders(context, isTargeted: false, targetedMessageId: null, cancellationToken);
    }
    else
    {
        await context.SendAsync($"Executing: {command}", cancellationToken);
    }

    return InvokeResponse.Ok();
});

// Handle adaptive card actions (cancel, dismiss, snooze)
teamsApp.OnAdaptiveCardAction(async (context, cancellationToken) =>
{
    var data = context.Activity.Value?.Action?.Data;
    if (data == null || data.Count == 0)
        return AdaptiveCardResponse.CreateMessageResponse("No data specified.", 200);

    string? GetValue(string key)
    {
        if (data.TryGetValue(key, out var val))
        {
            if (val is JsonElement element)
                return element.GetString();
            return val?.ToString();
        }
        return null;
    }

    var action = GetValue("action") ?? "";
    var reminderId = GetValue("reminderId") ?? "";

    switch (action)
    {
        case "cancel_reminder":
        {
            if (!string.IsNullOrEmpty(reminderId) && activeReminders.TryRemove(reminderId, out var reminder))
            {
                reminder.Cts.Cancel();
                Console.WriteLine($"[REMINDER] Cancelled reminder {reminderId}");
                return AdaptiveCardResponse.CreateMessageResponse("Reminder cancelled!", 200);
            }
            return AdaptiveCardResponse.CreateMessageResponse("Reminder not found or already completed.", 200);
        }

        case "dismiss_reminder":
        {
            if (!string.IsNullOrEmpty(reminderId))
                activeReminders.TryRemove(reminderId, out _);
            return AdaptiveCardResponse.CreateMessageResponse("Reminder dismissed!", 200);
        }

        case "snooze_reminder":
        {
            var reminderText = GetValue("reminderText") ?? "Snoozed reminder";
            var snoozeMinutes = int.TryParse(GetValue("snoozeMinutes"), out var sm) ? sm : 5;

            // Create a new snoozed reminder
            var newId = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x")[^4..]}{Random.Shared.Next(0x10000):x4}";
            var convId = (context.Activity.Conversation?.Id ?? "").Split(';')[0];
            var cts = new CancellationTokenSource();

            var newReminder = new ReminderInfo
            {
                Id = newId,
                ConversationId = convId,
                ServiceUrl = context.Activity.ServiceUrl,
                TargetUserId = context.Activity.From?.Id ?? "",
                TargetUserName = context.Activity.From?.Name ?? "User",
                CreatorId = context.Activity.From?.Id ?? "",
                CreatorName = context.Activity.From?.Name ?? "User",
                ReminderText = reminderText,
                DueTime = DateTimeOffset.UtcNow.AddMinutes(snoozeMinutes).ToUnixTimeMilliseconds(),
                Cts = cts
            };
            activeReminders[newId] = newReminder;

            // Schedule delivery
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(snoozeMinutes * 60_000, cts.Token);
                    await DeliverReminder(newReminder);
                }
                catch (TaskCanceledException) { }
            });

            // Send snooze confirmation as targeted message
            var snoozeCard = CreateSnoozeConfirmationCard(newReminder, snoozeMinutes);

            await context.SendAsync(
                new MessageActivityInput()
                    .WithText($"Snoozed for {snoozeMinutes} minutes")
                    .WithAdaptiveCardAttachment(ToCardJson(snoozeCard))
                    .WithRecipient(context.Activity.From!, isTargeted: true),
                cancellationToken);

            Console.WriteLine($"[REMINDER] Snoozed reminder, new ID: {newId}, delay: {snoozeMinutes} minutes");
            return AdaptiveCardResponse.CreateMessageResponse($"Snoozed for {snoozeMinutes} minutes!", 200);
        }

        default:
            return AdaptiveCardResponse.CreateMessageResponse("Unknown action.", 200);
    }
});

// Handle messageReaction events (when users add/remove reactions on messages)
teamsApp.OnMessageReaction(async (context, cancellationToken) =>
{
    var activity = context.Activity;
    var userName = activity.From?.Name ?? "Someone";

    // Handle added reactions
    if (activity.ReactionsAdded != null && activity.ReactionsAdded.Count > 0)
    {
        foreach (var reaction in activity.ReactionsAdded)
        {
            var reactionType = reaction.Type;
            Console.WriteLine($"[REACTION] {userName} added a {reactionType} reaction");
            await context.SendAsync($"Thanks for the **{reactionType}** reaction, {userName}!", cancellationToken);
        }
    }

    // Handle removed reactions
    if (activity.ReactionsRemoved != null && activity.ReactionsRemoved.Count > 0)
    {
        foreach (var reaction in activity.ReactionsRemoved)
        {
            var reactionType = reaction.Type;
            Console.WriteLine($"[REACTION] {userName} removed a {reactionType} reaction");
        }
    }
});

// Starts the Teams agent application and listens for incoming requests
webApp.Run();

// --- Types ---

class ReminderInfo
{
    public required string Id { get; set; }
    public required string ConversationId { get; set; }
    public Uri? ServiceUrl { get; set; }
    public required string TargetUserId { get; set; }
    public required string TargetUserName { get; set; }
    public required string CreatorId { get; set; }
    public required string CreatorName { get; set; }
    public required string ReminderText { get; set; }
    public long DueTime { get; set; }
    public CancellationTokenSource Cts { get; set; } = new();
}

class ParsedReminder
{
    public string? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public bool IsSelfReminder { get; set; }
    public string? ReminderText { get; set; }
    public int DelayMs { get; set; }
    public string? Error { get; set; }
}
