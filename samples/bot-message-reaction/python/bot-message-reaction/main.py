"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import asyncio
from typing import Dict, Any, Optional

from microsoft_teams.api import MessageActivity, MessageReactionActivity
from microsoft_teams.apps import ActivityContext, App

app = App()

# Simple in-memory activity log to track sent messages
_activity_log: Dict[str, Any] = {}


async def activity_log_append(activity_id: str, activity_data: Any) -> None:
    """Store an activity in the log."""
    if activity_id is None:
        raise TypeError("activity_id is required")
    if activity_data is None:
        raise TypeError("activity_data is required")
    _activity_log[activity_id] = {"activity": activity_data}


async def activity_log_find(activity_id: str) -> Optional[Any]:
    """Find an activity in the log by its ID."""
    if activity_id is None:
        raise TypeError("activity_id is required")
    item = _activity_log.get(activity_id)
    if item:
        return item.get("activity")
    return None


async def send_message_and_log_activity_id(ctx: ActivityContext, text: str) -> None:
    """
    Send a message to the user and log the activity ID for future reference.
    This allows us to look up the original message text when users
    react to the bot's messages.
    """
    response = await ctx.send(text)
    if response and response.id:
        await activity_log_append(response.id, {"text": text})


def strip_mentions(activity: MessageActivity) -> str:
    """Remove bot mentions from the message text."""
    text = activity.text or ""
    if activity.entities:
        for entity in activity.entities:
            if entity.type == "mention":
                mention = getattr(entity, "text", None)
                if mention:
                    text = text.replace(mention, "").strip()
    return text.strip()


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """
    Handle incoming message activities.
    Responds to the user with an echo of their message and logs the
    activity ID so we can reference it when reactions are added/removed.
    """
    text = strip_mentions(ctx.activity)
    await send_message_and_log_activity_id(ctx, f"echo: {text}")


@app.on_message_reaction
async def handle_message_reaction(ctx: ActivityContext[MessageReactionActivity]):
    """
    Handle message reaction activities (both added and removed).
    When a user adds or removes a reaction to a message, this handler
    looks up the original message and responds with information about
    the reaction and the message content.
    """
    reply_to_id = ctx.activity.reply_to_id

    # Handle reactions added
    if ctx.activity.reactions_added:
        for reaction in ctx.activity.reactions_added:
            activity = await activity_log_find(reply_to_id)
            if not activity:
                await send_message_and_log_activity_id(
                    ctx,
                    f"Activity {reply_to_id} not found in the log."
                )
            else:
                original_text = activity.get("text", "unknown")
                await send_message_and_log_activity_id(
                    ctx,
                    f"You added '{reaction.type}' regarding '{original_text}'"
                )

    # Handle reactions removed
    if ctx.activity.reactions_removed:
        for reaction in ctx.activity.reactions_removed:
            activity = await activity_log_find(reply_to_id)
            if not activity:
                await send_message_and_log_activity_id(
                    ctx,
                    f"Activity {reply_to_id} not found in the log."
                )
            else:
                original_text = activity.get("text", "unknown")
                await send_message_and_log_activity_id(
                    ctx,
                    f"You removed '{reaction.type}' regarding '{original_text}'"
                )


if __name__ == "__main__":
    asyncio.run(app.start())
