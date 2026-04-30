# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import re
import time
import math
from typing import Optional

from dotenv import load_dotenv
from microsoft_teams.api import (
    MessageActivity,
    MessageActivityInput,
    AdaptiveCardActionMessageResponse,
    Account,
)
from microsoft_teams.api.activities.message.message import (
    strip_mentions_text,
    StripMentionsTextOptions,
)
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.cards import AdaptiveCard, TextBlock, FactSet, ExecuteAction

# Load environment variables
load_dotenv()

app = App()


# --- Types ---

class ReminderInfo:
    def __init__(
        self,
        id: str,
        conversation_id: str,
        target_user_id: str,
        target_user_name: str,
        creator_id: str,
        creator_name: str,
        reminder_text: str,
        due_time: float,
        task: Optional[asyncio.Task] = None,
    ):
        self.id = id
        self.conversation_id = conversation_id
        self.target_user_id = target_user_id
        self.target_user_name = target_user_name
        self.creator_id = creator_id
        self.creator_name = creator_name
        self.reminder_text = reminder_text
        self.due_time = due_time
        self.task = task


# In-memory store for active reminders
active_reminders: dict[str, ReminderInfo] = {}

TIME_PATTERN = re.compile(r"in\s+(\d+)\s*(seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)\b", re.IGNORECASE)


def parse_time_expression(text: str):
    match = TIME_PATTERN.search(text.lower())
    if not match:
        return None
    value = int(match.group(1))
    unit = match.group(2).lower()
    if unit.startswith("second") or unit.startswith("sec") or unit == "s":
        return {"delay_s": value, "label": f"{value} second{'s' if value != 1 else ''}"}
    if unit.startswith("minute") or unit.startswith("min") or unit == "m":
        return {"delay_s": value * 60, "label": f"{value} minute{'s' if value != 1 else ''}"}
    if unit.startswith("hour") or unit.startswith("hr") or unit == "h":
        return {"delay_s": value * 3600, "label": f"{value} hour{'s' if value != 1 else ''}"}
    return None


def format_time_span(seconds: float) -> str:
    total = round(seconds)
    if total >= 3600:
        return f"{total // 3600}h {(total % 3600) // 60}m"
    if total >= 60:
        return f"{total // 60}m {total % 60}s"
    return f"{total}s"


def extract_mentioned_user(activity: MessageActivity, bot_id: str):
    for entity in activity.entities or []:
        if getattr(entity, "type", None) == "mention":
            mentioned = getattr(entity, "mentioned", None)
            if mentioned and getattr(mentioned, "id", None) != bot_id:
                return {"user_id": mentioned.id, "user_name": getattr(mentioned, "name", "User")}
    return None


def parse_reminder_command(activity: MessageActivity, command_text: str):
    bot_id = activity.recipient.id if activity.recipient else ""
    text = command_text.strip()

    # Remove "remind" prefix
    if text.lower().startswith("remind"):
        text = text[6:].strip()

    is_self = False

    # Check for "me" (self-reminder)
    if re.match(r"^me(\s|,|$)", text, re.IGNORECASE):
        is_self = True
        target_user_id = activity.from_.id if activity.from_ else ""
        target_user_name = activity.from_.name if activity.from_ else "You"
        text = text[2:].strip().lstrip(",").strip()
    else:
        mentioned = extract_mentioned_user(activity, bot_id)
        if mentioned:
            target_user_id = mentioned["user_id"]
            target_user_name = mentioned["user_name"]
            # Strip the mentioned user's tag from text
            stripped = strip_mentions_text(activity, StripMentionsTextOptions(account_id=mentioned["user_id"]))
            if stripped:
                text = stripped.strip()
        else:
            is_self = True
            target_user_id = activity.from_.id if activity.from_ else ""
            target_user_name = activity.from_.name if activity.from_ else "You"

    parsed_time = parse_time_expression(text)
    if not parsed_time:
        return {"error": "Could not parse time. Use format like 'in 5 minutes', 'in 1 hour', or 'in 30 seconds'."}

    # Extract reminder text (everything after the time expression)
    reminder_text = TIME_PATTERN.sub("", text).strip()
    reminder_text = reminder_text.lstrip(",").strip()
    if reminder_text.lower().startswith("to "):
        reminder_text = reminder_text[3:].strip()
    if reminder_text.lower().startswith("that "):
        reminder_text = reminder_text[5:].strip()
    if not reminder_text:
        reminder_text = "You have a reminder!"

    return {
        "target_user_id": target_user_id,
        "target_user_name": target_user_name,
        "is_self": is_self,
        "reminder_text": reminder_text,
        "delay_s": parsed_time["delay_s"],
    }


def create_confirmation_card(reminder: ReminderInfo, delay_s: int) -> AdaptiveCard:
    target = "yourself" if reminder.creator_id == reminder.target_user_id else reminder.target_user_name
    return AdaptiveCard(
        version="1.5",
        body=[
            TextBlock(text="Reminder Set!", weight="Bolder", size="Medium", color="Good"),
            FactSet(facts=[
                {"title": "Reminder:", "value": reminder.reminder_text},
                {"title": "For:", "value": target},
                {"title": "In:", "value": format_time_span(delay_s)},
                {"title": "ID:", "value": reminder.id},
            ]),
            TextBlock(text="This is a targeted message — only you can see this.", size="Small", isSubtle=True, wrap=True),
        ],
        actions=[
            ExecuteAction(title="Cancel Reminder", verb="cancel_reminder", data={"action": "cancel_reminder", "reminderId": reminder.id}),
        ],
    )


def create_delivery_card(reminder: ReminderInfo) -> AdaptiveCard:
    from_display = "yourself" if reminder.creator_id == reminder.target_user_id else reminder.creator_name
    return AdaptiveCard(
        version="1.5",
        body=[
            TextBlock(text="Reminder", weight="Bolder", size="Large", color="Accent"),
            TextBlock(text=reminder.reminder_text, wrap=True, size="Medium"),
            TextBlock(text=f"Set by {from_display}", size="Small", isSubtle=True),
        ],
        actions=[
            ExecuteAction(title="Dismiss", verb="dismiss_reminder", data={"action": "dismiss_reminder", "reminderId": reminder.id}),
            ExecuteAction(title="Snooze 5 min", verb="snooze_reminder", data={"action": "snooze_reminder", "reminderId": reminder.id, "reminderText": reminder.reminder_text, "snoozeMinutes": "5"}),
        ],
    )


def create_snooze_card(reminder: ReminderInfo, snooze_minutes: int) -> AdaptiveCard:
    return AdaptiveCard(
        version="1.5",
        body=[
            TextBlock(text="Snoozed!", weight="Bolder", size="Medium", color="Accent"),
            TextBlock(text=reminder.reminder_text, wrap=True),
            TextBlock(text=f"Will remind you again in {snooze_minutes} minutes.", size="Small", isSubtle=True),
        ],
        actions=[
            ExecuteAction(title="Cancel", verb="cancel_reminder", data={"action": "cancel_reminder", "reminderId": reminder.id}),
        ],
    )


def targeted_account(user_id: str, user_name: str) -> Account:
    """Create an Account for targeted messaging."""
    return Account(id=user_id, name=user_name)


async def deliver_reminder(reminder: ReminderInfo) -> None:
    if reminder.id not in active_reminders:
        print(f"[REMINDER] Reminder {reminder.id} was cancelled, skipping delivery")
        return

    try:
        await asyncio.sleep(reminder.due_time - time.time())
    except asyncio.CancelledError:
        return

    if reminder.id not in active_reminders:
        return

    try:
        card = create_delivery_card(reminder)
        recipient = targeted_account(reminder.target_user_id, reminder.target_user_name)
        msg = MessageActivityInput(text=f"Reminder: {reminder.reminder_text}").add_card(card).with_recipient(recipient, is_targeted=True)
        await app.send(reminder.conversation_id, msg)
        print(f"[REMINDER] Delivered reminder {reminder.id} to {reminder.target_user_name}")
        active_reminders.pop(reminder.id, None)
    except Exception as error:
        print(f"[REMINDER] Failed to deliver reminder {reminder.id}: {error}")
        active_reminders.pop(reminder.id, None)


async def handle_remind_command(ctx: ActivityContext[MessageActivity], command_text: str) -> None:
    activity = ctx.activity
    parsed = parse_reminder_command(activity, command_text)

    if "error" in parsed:
        await ctx.send(f"{parsed['error']}\n\nUse `reminder-help` for usage examples.")
        return

    target_user_id = parsed["target_user_id"]
    target_user_name = parsed["target_user_name"]
    reminder_text = parsed["reminder_text"]
    delay_s = parsed["delay_s"]

    if not target_user_id:
        await ctx.send("Could not determine who to remind. Use `remind me` or mention someone like `remind @John`.")
        return

    # Create reminder
    reminder_id = f"{int(time.time()) % 100000:05x}{id(ctx) % 0xFFFF:04x}"
    conv_id = activity.conversation.id.split(";")[0]
    reminder = ReminderInfo(
        id=reminder_id,
        conversation_id=conv_id,
        target_user_id=target_user_id,
        target_user_name=target_user_name,
        creator_id=activity.from_.id if activity.from_ else "",
        creator_name=activity.from_.name if activity.from_ else "Someone",
        reminder_text=reminder_text,
        due_time=time.time() + delay_s,
    )
    active_reminders[reminder_id] = reminder
    reminder.task = asyncio.create_task(deliver_reminder(reminder))

    try:
        card = create_confirmation_card(reminder, delay_s)
        creator = targeted_account(activity.from_.id, activity.from_.name)
        msg = MessageActivityInput(text="Reminder has been set!").add_card(card).with_recipient(creator, is_targeted=True)
        await ctx.send(msg)
        print(f"[REMINDER] Created reminder {reminder_id} for {target_user_name} in {delay_s} seconds")
    except Exception as error:
        print(f"[REMINDER] Error sending confirmation: {error}")
        active_reminders.pop(reminder_id, None)
        if reminder.task:
            reminder.task.cancel()


async def show_my_reminders(ctx: ActivityContext[MessageActivity]) -> None:
    activity = ctx.activity
    user_id = activity.from_.id if activity.from_ else None
    if not user_id:
        await ctx.send("Could not determine your user ID.")
        return

    my = sorted(
        [r for r in active_reminders.values() if r.target_user_id == user_id or r.creator_id == user_id],
        key=lambda r: r.due_time,
    )
    sender = targeted_account(activity.from_.id, activity.from_.name)

    if not my:
        await ctx.send(MessageActivityInput(text="You have no active reminders.").with_recipient(sender, is_targeted=True))
        return

    lines = []
    for r in my:
        time_left = r.due_time - time.time()
        time_str = f"in {format_time_span(time_left)}" if time_left > 0 else "overdue"
        target = "yourself" if r.creator_id == r.target_user_id else r.target_user_name
        lines.append(f'- **{r.id}**: "{r.reminder_text}" for {target} ({time_str})')

    text = "**Your Active Reminders:**\n\n" + "\n".join(lines) + "\n\nUse `cancel-reminder [id]` to cancel a reminder."
    await ctx.send(MessageActivityInput(text=text).with_recipient(sender, is_targeted=True))


async def cancel_reminder_command(ctx: ActivityContext[MessageActivity], reminder_id: str) -> None:
    activity = ctx.activity
    user_id = activity.from_.id if activity.from_ else None
    sender = targeted_account(activity.from_.id, activity.from_.name)

    if not reminder_id:
        await ctx.send(
            MessageActivityInput(text="Please specify a reminder ID. Use `my-reminders` to see your active reminders.").with_recipient(sender, is_targeted=True)
        )
        return

    reminder = active_reminders.get(reminder_id)
    if not reminder:
        await ctx.send(
            MessageActivityInput(text=f"Reminder **{reminder_id}** not found or already completed.").with_recipient(sender, is_targeted=True)
        )
        return

    if reminder.creator_id == user_id or reminder.target_user_id == user_id:
        if reminder.task:
            reminder.task.cancel()
        active_reminders.pop(reminder_id, None)
        await ctx.send(
            MessageActivityInput(text=f"Reminder **{reminder_id}** has been cancelled.").with_recipient(sender, is_targeted=True)
        )
        print(f"[REMINDER] Reminder {reminder_id} cancelled by {activity.from_.name}")
    else:
        await ctx.send(
            MessageActivityInput(text="You can only cancel reminders you created or are assigned to you.").with_recipient(sender, is_targeted=True)
        )


async def show_help(ctx: ActivityContext) -> None:
    await ctx.send(
        "**Personal Reminder Bot - Help**\n\n"
        "**Set a Reminder:**\n"
        "- `remind me in 5 minutes to check email`\n"
        "- `remind me in 1 hour meeting starts`\n"
        "- `remind me in 30 seconds test`\n"
        "- `remind @John in 10 minutes review PR`\n\n"
        "**Supported Time Formats:**\n"
        "- Seconds: `30 seconds`, `30 secs`, `30s`\n"
        "- Minutes: `5 minutes`, `5 mins`, `5m`\n"
        "- Hours: `1 hour`, `2 hrs`, `1h`\n\n"
        "**Manage Reminders:**\n"
        "- `my-reminders` — View your active reminders\n"
        "- `cancel-reminder [id]` — Cancel a specific reminder\n"
        "- `reminder-help` — Show this help message\n\n"
        "**How It Works:**\n"
        "- Reminders are delivered as **targeted messages** (only the recipient can see them)\n"
        "- Works in both **channels** and **group chats**\n"
        "- Set reminders for yourself or mention others\n"
        "- Dismiss or snooze reminders via card buttons"
    )


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    activity = ctx.activity
    if not activity.text:
        return

    bot_id = activity.recipient.id if activity.recipient else ""
    stripped = strip_mentions_text(activity, StripMentionsTextOptions(account_id=bot_id))
    text = (stripped or activity.text or "").strip()

    lower = text.lower()

    if lower in ("reminder-help", "help"):
        await show_help(ctx)
    elif lower.startswith("remind"):
        await handle_remind_command(ctx, text)
    elif lower == "my-reminders":
        await show_my_reminders(ctx)
    elif lower.startswith("cancel-reminder"):
        reminder_id = re.sub(r"^cancel-reminder\s*", "", text, flags=re.IGNORECASE).strip()
        await cancel_reminder_command(ctx, reminder_id)
    else:
        await ctx.send("Use `reminder-help` to see available commands.")


@app.on_card_action
async def handle_card_action(ctx: ActivityContext):
    data = getattr(getattr(getattr(ctx.activity, "value", None), "action", None), "data", None)
    if not data:
        return AdaptiveCardActionMessageResponse(value="No data specified.")

    action = data.get("action") if isinstance(data, dict) else getattr(data, "action", None)
    reminder_id = data.get("reminderId") if isinstance(data, dict) else getattr(data, "reminderId", None)

    if action == "cancel_reminder":
        reminder = active_reminders.get(reminder_id) if reminder_id else None
        if reminder:
            if reminder.task:
                reminder.task.cancel()
            active_reminders.pop(reminder_id, None)
            print(f"[REMINDER] Cancelled reminder {reminder_id}")
            return AdaptiveCardActionMessageResponse(value="Reminder cancelled!")
        return AdaptiveCardActionMessageResponse(value="Reminder not found or already completed.")

    if action == "dismiss_reminder":
        if reminder_id:
            active_reminders.pop(reminder_id, None)
        return AdaptiveCardActionMessageResponse(value="Reminder dismissed!")

    if action == "snooze_reminder":
        reminder_text = (data.get("reminderText") if isinstance(data, dict) else getattr(data, "reminderText", None)) or "Snoozed reminder"
        snooze_minutes = int((data.get("snoozeMinutes") if isinstance(data, dict) else getattr(data, "snoozeMinutes", None)) or "5")

        new_id = f"{int(time.time()) % 100000:05x}{id(ctx) % 0xFFFF:04x}"
        conv_id = (ctx.activity.conversation.id if ctx.activity.conversation else "").split(";")[0]
        new_reminder = ReminderInfo(
            id=new_id,
            conversation_id=conv_id,
            target_user_id=ctx.activity.from_.id if ctx.activity.from_ else "",
            target_user_name=ctx.activity.from_.name if ctx.activity.from_ else "User",
            creator_id=ctx.activity.from_.id if ctx.activity.from_ else "",
            creator_name=ctx.activity.from_.name if ctx.activity.from_ else "User",
            reminder_text=reminder_text,
            due_time=time.time() + snooze_minutes * 60,
        )
        active_reminders[new_id] = new_reminder
        new_reminder.task = asyncio.create_task(deliver_reminder(new_reminder))

        card = create_snooze_card(new_reminder, snooze_minutes)
        recipient = targeted_account(
            ctx.activity.from_.id if ctx.activity.from_ else "",
            ctx.activity.from_.name if ctx.activity.from_ else "",
        )
        msg = MessageActivityInput(text=f"Snoozed for {snooze_minutes} minutes").add_card(card).with_recipient(recipient, is_targeted=True)
        await ctx.send(msg)

        print(f"[REMINDER] Snoozed reminder, new ID: {new_id}, delay: {snooze_minutes} minutes")
        return AdaptiveCardActionMessageResponse(value=f"Snoozed for {snooze_minutes} minutes!")

    return AdaptiveCardActionMessageResponse(value="Unknown action.")


if __name__ == "__main__":
    asyncio.run(app.start())
