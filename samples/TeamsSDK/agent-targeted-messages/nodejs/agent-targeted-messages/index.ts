// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { App } from '@microsoft/teams.apps';
import { Client, MessageActivity, MessageReactionActivity, stripMentionsText } from '@microsoft/teams.api';
import { AdaptiveCard, TextBlock, FactSet, ExecuteAction } from '@microsoft/teams.cards';

const app = new App();

/*
 * === Agentic Flow Stub: LLM Client Setup ===
 * Initialize your preferred LLM client here for agentic capabilities.
 *
 * import { OpenAIClient } from '@azure/openai';
 * const llmClient = new OpenAIClient(endpoint, credential);
 */


interface ReminderInfo {
    id: string;
    conversationId: string;
    targetUserId: string;
    targetUserName: string;
    creatorId: string;
    creatorName: string;
    reminderText: string;
    dueTime: number;
    timer: ReturnType<typeof setTimeout>;
}

// In-memory store for active reminders
const activeReminders = new Map<string, ReminderInfo>();


const TIME_PATTERN = /in\s+(\d+)\s*(seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)\b/i;

function parseTimeExpression(text: string): { delayMs: number; label: string } | null {
    const match = TIME_PATTERN.exec(text.toLowerCase());
    if (!match) return null;

    const value = parseInt(match[1]);
    const unit = match[2].toLowerCase();

    if (unit.startsWith('second') || unit.startsWith('sec') || unit === 's')
        return { delayMs: value * 1000, label: `${value} second${value !== 1 ? 's' : ''}` };
    if (unit.startsWith('minute') || unit.startsWith('min') || unit === 'm')
        return { delayMs: value * 60_000, label: `${value} minute${value !== 1 ? 's' : ''}` };
    if (unit.startsWith('hour') || unit.startsWith('hr') || unit === 'h')
        return { delayMs: value * 3_600_000, label: `${value} hour${value !== 1 ? 's' : ''}` };

    return null;
}

function formatTimeSpan(ms: number): string {
    const totalSecs = Math.round(ms / 1000);
    if (totalSecs >= 3600) return `${Math.floor(totalSecs / 3600)}h ${Math.floor((totalSecs % 3600) / 60)}m`;
    if (totalSecs >= 60) return `${Math.floor(totalSecs / 60)}m ${totalSecs % 60}s`;
    return `${totalSecs}s`;
}


function extractMentionedUser(msg: MessageActivity, botId: string): { userId: string; userName: string } | null {
    for (const entity of msg.entities || []) {
        if (entity.type === 'mention' && (entity as any).mentioned?.id !== botId) {
            return { userId: (entity as any).mentioned.id, userName: (entity as any).mentioned.name || 'User' };
        }
    }
    return null;
}


function parseReminderCommand(msg: MessageActivity, commandText: string): {
    targetUserId: string; targetUserName: string; isSelfReminder: boolean;
    reminderText: string; delayMs: number; error?: string;
} | { error: string } {
    const botId = msg.recipient?.id || '';
    let text = commandText.trim();

    // Remove "remind" prefix
    if (text.toLowerCase().startsWith('remind')) text = text.slice(6).trim();

    let targetUserId: string;
    let targetUserName: string;
    let isSelfReminder = false;

    // Check for "me" (self-reminder)
    if (/^me(\s|,|$)/i.test(text)) {
        isSelfReminder = true;
        targetUserId = msg.from?.id || '';
        targetUserName = msg.from?.name || 'You';
        text = text.slice(2).trim().replace(/^,/, '').trim();
    } else {
        // Check for @mention of a target user
        const mentioned = extractMentionedUser(msg, botId);
        if (mentioned) {
            targetUserId = mentioned.userId;
            targetUserName = mentioned.userName;
            // Remove the target user's mention tag from the local text (not the raw activity)
            for (const entity of msg.entities || []) {
                if (entity.type === 'mention' && (entity as any).mentioned?.id === mentioned.userId && (entity as any).text) {
                    text = text.replace((entity as any).text, '').trim();
                }
            }
        } else {
            // Default to self if no target specified
            isSelfReminder = true;
            targetUserId = msg.from?.id || '';
            targetUserName = msg.from?.name || 'You';
        }
    }

    // Parse time expression
    const time = parseTimeExpression(text);
    if (!time) {
        return { error: "Could not parse time. Use format like 'in 5 minutes', 'in 1 hour', or 'in 30 seconds'." };
    }

    // Extract reminder text (everything after the time expression)
    let reminderText = text.replace(TIME_PATTERN, '').trim();
    reminderText = reminderText.replace(/^,/, '').trim();
    if (reminderText.toLowerCase().startsWith('to ')) reminderText = reminderText.slice(3).trim();
    if (reminderText.toLowerCase().startsWith('that ')) reminderText = reminderText.slice(5).trim();
    if (!reminderText) reminderText = 'You have a reminder!';

    return { targetUserId, targetUserName, isSelfReminder, reminderText, delayMs: time.delayMs };
}


function createConfirmationCard(reminder: ReminderInfo, delayMs: number) {
    const targetDisplay = reminder.creatorId === reminder.targetUserId ? 'yourself' : reminder.targetUserName;
    return new AdaptiveCard(
        new TextBlock('Reminder Set!', { weight: 'Bolder', size: 'Medium', color: 'Good' }),
        new FactSet(
            { title: 'Reminder:', value: reminder.reminderText },
            { title: 'For:', value: targetDisplay },
            { title: 'In:', value: formatTimeSpan(delayMs) },
            { title: 'ID:', value: reminder.id }
        ),
        new TextBlock('This is a targeted message — only you can see this.', { size: 'Small', isSubtle: true, wrap: true })
    ).withActions(
        new ExecuteAction()
            .withTitle('Cancel Reminder')
            .withVerb('cancel_reminder')
            .withData({ action: 'cancel_reminder', reminderId: reminder.id })
    );
}

function createDeliveryCard(reminder: ReminderInfo) {
    const fromDisplay = reminder.creatorId === reminder.targetUserId ? 'yourself' : reminder.creatorName;
    return new AdaptiveCard(
        new TextBlock('Reminder', { weight: 'Bolder', size: 'Large', color: 'Accent' }),
        new TextBlock(reminder.reminderText, { wrap: true, size: 'Medium' }),
        new TextBlock(`Set by ${fromDisplay}`, { size: 'Small', isSubtle: true })
    ).withActions(
        new ExecuteAction()
            .withTitle('Dismiss')
            .withVerb('dismiss_reminder')
            .withData({ action: 'dismiss_reminder', reminderId: reminder.id }),
        new ExecuteAction()
            .withTitle('Snooze 5 min')
            .withVerb('snooze_reminder')
            .withData({ action: 'snooze_reminder', reminderId: reminder.id, reminderText: reminder.reminderText, snoozeMinutes: '5' })
    );
}

function createSnoozeConfirmationCard(reminder: ReminderInfo, snoozeMinutes: number) {
    return new AdaptiveCard(
        new TextBlock('Snoozed!', { weight: 'Bolder', size: 'Medium', color: 'Accent' }),
        new TextBlock(reminder.reminderText, { wrap: true }),
        new TextBlock(`Will remind you again in ${snoozeMinutes} minutes.`, { size: 'Small', isSubtle: true })
    ).withActions(
        new ExecuteAction()
            .withTitle('Cancel')
            .withVerb('cancel_reminder')
            .withData({ action: 'cancel_reminder', reminderId: reminder.id })
    );
}


async function deliverReminder(reminder: ReminderInfo): Promise<void> {
    // Check if reminder was cancelled
    if (!activeReminders.has(reminder.id)) {
        console.log(`[REMINDER] Reminder ${reminder.id} was cancelled, skipping delivery`);
        return;
    }

    try {
        const card = createDeliveryCard(reminder);
        const recipient = { id: reminder.targetUserId, name: reminder.targetUserName, role: 'user' as const };

        // Send targeted reminder via app.send — only the recipient can see it
        await app.send(
            reminder.conversationId,
            new MessageActivity(`Reminder: ${reminder.reminderText}`)
                .addCard('adaptive', card)
                .withRecipient(recipient, true)
        );

        console.log(`[REMINDER] Delivered reminder ${reminder.id} to ${reminder.targetUserName}`);
        activeReminders.delete(reminder.id);
    } catch (error) {
        console.error(`[REMINDER] Failed to deliver reminder ${reminder.id}:`, error);
        activeReminders.delete(reminder.id);
    }
}

app.on('message', async ({ activity, send, api }) => {
    const msg = activity as MessageActivity;
    if (!msg.text) return;

    // Check if this is a targeted message (TM) from the user via slash command
    const isTargeted = !!(msg.recipient as any)?.isTargeted;
    if (isTargeted) {
        console.log(`[TM] Received targeted message from ${msg.from?.name || 'unknown'}`);
    }

    // Use SDK's built-in stripMentionsText to remove bot mention
    const botId = msg.recipient?.id || '';
    let text = (stripMentionsText(msg, { accountId: botId }) || msg.text || '').trim();

    // Route commands
    const lower = text.toLowerCase();

    if (lower === 'reminder-help' || lower === 'help') {
        await showHelp(send);
    } else if (lower.startsWith('remind')) {
        await handleRemindCommand({ activity, send, isTargeted }, text);
    } else if (lower === 'my-reminders') {
        await showMyReminders({ activity, send, isTargeted });
    } else if (lower.startsWith('cancel-reminder')) {
        const reminderId = text.replace(/^cancel-reminder\s*/i, '').trim();
        await cancelReminder({ activity, send, isTargeted }, reminderId);
    } else if (lower.startsWith('add-reaction')) {
        await handleAddReaction({ activity, send, api }, text);
    } else if (lower.startsWith('remove-reaction')) {
        await handleRemoveReaction({ activity, send, api }, text);
    } else {
        await send('Use `reminder-help` to see available commands.');
    }
});

async function handleRemindCommand(ctx: { activity: any; send: Function; isTargeted: boolean }, commandText: string): Promise<void> {
    const { activity, send, isTargeted } = ctx;

    const parsed = parseReminderCommand(activity as MessageActivity, commandText);
    if ('error' in parsed && !('targetUserId' in parsed)) {
        await send(`${parsed.error}\n\nUse \`reminder-help\` for usage examples.`);
        return;
    }
    const { targetUserId, targetUserName, reminderText, delayMs } = parsed as any;

    if (!targetUserId) {
        await send("Could not determine who to remind. Use `remind me` or mention someone like `remind @John`.");
        return;
    }

    // Create reminder
    const reminderId = `${Date.now().toString(36).slice(-4)}${Math.random().toString(36).slice(2, 6)}`;
    const convId = activity.conversation.id.split(';')[0];
    const reminder: ReminderInfo = {
        id: reminderId,
        conversationId: convId,
        targetUserId,
        targetUserName,
        creatorId: activity.from?.id || '',
        creatorName: activity.from?.name || 'Someone',
        reminderText,
        dueTime: Date.now() + delayMs,
        timer: setTimeout(() => deliverReminder(reminder), delayMs)
    };

    activeReminders.set(reminderId, reminder);

    try {
        // Send targeted confirmation card to the creator — only they can see it
        const card = createConfirmationCard(reminder, delayMs);
        const creator = { id: activity.from.id, name: activity.from.name, role: 'user' as const };

        const response = new MessageActivity('Reminder has been set!')
            .addCard('adaptive', card);

        if (isTargeted) {
            response.withRecipient(creator, true).addTargetedMessageInfo(activity.id);
        }

        await send(response);

        console.log(`[REMINDER] Created reminder ${reminderId} for ${targetUserName} in ${delayMs / 1000} seconds`);
    } catch (error) {
        console.error(`[REMINDER] Error sending confirmation:`, error);
        activeReminders.delete(reminderId);
        clearTimeout(reminder.timer);
    }
}

async function showMyReminders(ctx: { activity: any; send: Function; isTargeted: boolean }): Promise<void> {
    const { activity, send, isTargeted } = ctx;
    const userId = activity.from?.id;
    if (!userId) { await send('Could not determine your user ID.'); return; }

    const myReminders = [...activeReminders.values()]
        .filter(r => r.targetUserId === userId || r.creatorId === userId)
        .sort((a, b) => a.dueTime - b.dueTime);

    const sender = { id: activity.from.id, name: activity.from.name, role: 'user' as const };

    if (myReminders.length === 0) {
        const response = new MessageActivity('You have no active reminders.');
        if (isTargeted) {
            response.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
        }
        await send(response);
        return;
    }

    const list = myReminders.map(r => {
        const timeLeft = r.dueTime - Date.now();
        const timeStr = timeLeft > 0 ? `in ${formatTimeSpan(timeLeft)}` : 'overdue';
        const target = r.creatorId === r.targetUserId ? 'yourself' : r.targetUserName;
        return `- **${r.id}**: "${r.reminderText}" for ${target} (${timeStr})`;
    }).join('\n');

    const response = new MessageActivity(`**Your Active Reminders:**\n\n${list}\n\nUse \`cancel-reminder [id]\` to cancel a reminder.`);
    if (isTargeted) {
        response.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
    }
    await send(response);
}

async function cancelReminder(ctx: { activity: any; send: Function; isTargeted: boolean }, reminderId: string): Promise<void> {
    const { activity, send, isTargeted } = ctx;
    const userId = activity.from?.id;
    const sender = { id: activity.from.id, name: activity.from.name, role: 'user' as const };

    if (!reminderId) {
        const response = new MessageActivity('Please specify a reminder ID. Use `my-reminders` to see your active reminders.');
        if (isTargeted) {
            response.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
        }
        await send(response);
        return;
    }

    const reminder = activeReminders.get(reminderId);
    if (!reminder) {
        const response = new MessageActivity(`Reminder **${reminderId}** not found or already completed.`);
        if (isTargeted) {
            response.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
        }
        await send(response);
        return;
    }

    // Only allow creator or target to cancel
    if (reminder.creatorId === userId || reminder.targetUserId === userId) {
        clearTimeout(reminder.timer);
        activeReminders.delete(reminderId);
        const cancelledResponse = new MessageActivity(`Reminder **${reminderId}** has been cancelled.`);
        if (isTargeted) {
            cancelledResponse.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
        }
        await send(cancelledResponse);
        console.log(`[REMINDER] Reminder ${reminderId} cancelled by ${activity.from?.name}`);
    } else {
        const deniedResponse = new MessageActivity('You can only cancel reminders you created or are assigned to you.');
        if (isTargeted) {
            deniedResponse.withRecipient(sender, true).addTargetedMessageInfo(activity.id);
        }
        await send(deniedResponse);
    }
}

async function showHelp(send: Function): Promise<void> {
    await send([
        '**Personal Reminder Agent - Help**',
        '',
        '**Set a Reminder:**',
        '- `remind me in 5 minutes to check email`',
        '- `remind me in 1 hour meeting starts`',
        '- `remind me in 30 seconds test`',
        '- `remind @John in 10 minutes review PR`',
        '',
        '**Supported Time Formats:**',
        '- Seconds: `30 seconds`, `30 secs`, `30s`',
        '- Minutes: `5 minutes`, `5 mins`, `5m`',
        '- Hours: `1 hour`, `2 hrs`, `1h`',
        '',
        '**Manage Reminders:**',
        '- `my-reminders` — View your active reminders',
        '- `cancel-reminder [id]` — Cancel a specific reminder',
        '- `reminder-help` — Show this help message',
        '',
        '**How It Works:**',
        '- Reminders are delivered as **targeted messages** (only the recipient can see them)',
        '- Works in both **channels** and **group chats**',
        '- Set reminders for yourself or mention others',
        '- Dismiss or snooze reminders via card buttons',
        '',
        '**Reactions:**',
        '- `add-reaction [type]` — Bot adds a reaction to your message',
        '- `remove-reaction [type]` — Bot removes a reaction from your message',
        '- React to any bot message and the bot will acknowledge it!',
        '',
        '**Supported Reaction Types:**',
        '- `like` 👍, `heart` ❤️, `1f440_eyes` 👀, `2705_whiteheavycheckmark` ✅, `launch` 🚀, `1f4cc_pushpin` 📌'
    ].join('\n'));
}

app.on('card.action', async ({ activity, send }) => {
    const data = (activity as any).value?.action?.data;
    if (!data) return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: 'No data specified.' } as AdaptiveCardActionResponse;

    const action = data.action;
    const reminderId = data.reminderId;

    switch (action) {
        case 'cancel_reminder': {
            if (reminderId && activeReminders.has(reminderId)) {
                const reminder = activeReminders.get(reminderId)!;
                clearTimeout(reminder.timer);
                activeReminders.delete(reminderId);
                console.log(`[REMINDER] Cancelled reminder ${reminderId}`);
                return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: 'Reminder cancelled!' } as AdaptiveCardActionResponse;
            }
            return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: 'Reminder not found or already completed.' } as AdaptiveCardActionResponse;
        }

        case 'dismiss_reminder': {
            if (reminderId) activeReminders.delete(reminderId);
            return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: 'Reminder dismissed!' } as AdaptiveCardActionResponse;
        }

        case 'snooze_reminder': {
            const reminderText = data.reminderText || 'Snoozed reminder';
            const snoozeMinutes = parseInt(data.snoozeMinutes || '5') || 5;

            // Create a new snoozed reminder
            const newId = `${Date.now().toString(36).slice(-4)}${Math.random().toString(36).slice(2, 6)}`;
            const convId = activity.conversation?.id?.split(';')[0] || '';
            const newReminder: ReminderInfo = {
                id: newId,
                conversationId: convId,
                targetUserId: activity.from?.id || '',
                targetUserName: activity.from?.name || 'User',
                creatorId: activity.from?.id || '',
                creatorName: activity.from?.name || 'User',
                reminderText,
                dueTime: Date.now() + snoozeMinutes * 60_000,
                timer: setTimeout(() => deliverReminder(newReminder), snoozeMinutes * 60_000)
            };
            activeReminders.set(newId, newReminder);

            // Update the card with snooze confirmation
            const snoozeCard = createSnoozeConfirmationCard(newReminder, snoozeMinutes);
            const recipient = { id: activity.from?.id || '', name: activity.from?.name || '', role: 'user' as const };

            await send(
                new MessageActivity(`Snoozed for ${snoozeMinutes} minutes`)
                    .addCard('adaptive', snoozeCard)
                    .withRecipient(recipient, true)
            );

            console.log(`[REMINDER] Snoozed reminder, new ID: ${newId}, delay: ${snoozeMinutes} minutes`);
            return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: `Snoozed for ${snoozeMinutes} minutes!` } as AdaptiveCardActionResponse;
        }

        default:
            return { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: 'Unknown action.' } as AdaptiveCardActionResponse;
    }
});

// === Reactions Feature ===

type ReactionParameter = Parameters<Client['reactions']['add']>[2];

async function handleAddReaction(ctx: { activity: any; send: Function; api?: any }, commandText: string): Promise<void> {
    const { activity, send, api } = ctx;
    const reactionType = commandText.replace(/^add-reaction\s*/i, '').trim();

    if (!reactionType) {
        await send('Please specify a reaction type. Example: `add-reaction like`\n\nSupported types: `like`, `heart`, `1f440_eyes`, `2705_whiteheavycheckmark`, `launch`, `1f4cc_pushpin`');
        return;
    }

    if (!api) {
        await send('API client is not available.');
        return;
    }

    try {
        await api.reactions.add(
            activity.conversation.id,
            activity.id,
            reactionType as ReactionParameter
        );
        await send(`Added a **${reactionType}** reaction to your message!`);
        console.log(`[REACTION] Added ${reactionType} reaction to message ${activity.id}`);
    } catch (error) {
        console.error('[REACTION] Failed to add reaction:', error);
        await send('Sorry, I had trouble adding that reaction.');
    }
}

async function handleRemoveReaction(ctx: { activity: any; send: Function; api?: any }, commandText: string): Promise<void> {
    const { activity, send, api } = ctx;
    const reactionType = commandText.replace(/^remove-reaction\s*/i, '').trim();

    if (!reactionType) {
        await send('Please specify a reaction type. Example: `remove-reaction like`');
        return;
    }

    if (!api) {
        await send('API client is not available.');
        return;
    }

    try {
        await api.reactions.delete(
            activity.conversation.id,
            activity.id,
            reactionType as ReactionParameter
        );
        await send(`Removed the **${reactionType}** reaction from your message!`);
        console.log(`[REACTION] Removed ${reactionType} reaction from message ${activity.id}`);
    } catch (error) {
        console.error('[REACTION] Failed to remove reaction:', error);
        await send('Sorry, I had trouble removing that reaction.');
    }
}

// Handle messageReaction events (when users add/remove reactions on messages)
app.on('messageReaction', async ({ activity, send }) => {
    const reactionActivity = activity as MessageReactionActivity;

    // Handle added reactions
    if (reactionActivity.reactionsAdded && reactionActivity.reactionsAdded.length > 0) {
        for (const reaction of reactionActivity.reactionsAdded) {
            const userName = reaction.user?.displayName || 'Someone';
            const reactionType = reaction.type;
            console.log(`[REACTION] ${userName} added a ${reactionType} reaction`);
            await send(`Thanks for the **${reactionType}** reaction, ${userName}!`);
        }
    }

    // Handle removed reactions
    if (reactionActivity.reactionsRemoved && reactionActivity.reactionsRemoved.length > 0) {
        for (const reaction of reactionActivity.reactionsRemoved) {
            const userName = reaction.user?.displayName || 'Someone';
            const reactionType = reaction.type;
            console.log(`[REACTION] ${userName} removed a ${reactionType} reaction`);
        }
    }
});

app.start().catch(console.error);
