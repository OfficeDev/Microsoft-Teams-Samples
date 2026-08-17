# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
from typing import Optional

from microsoft_teams.api import InstalledActivity, MessageActivity, MessageActivityInput
from microsoft_teams.apps import ActivityContext, App

app = App()

# This would be some persistent storage in a real app. It maps a user's
# Microsoft Entra object id to the conversation id we can message them on.
conversation_id_store: dict[str, str] = {}


def save_conversation(user_id: Optional[str], conversation_id: str) -> None:
    """Save the conversation_id so it can be used for proactive messaging later."""
    if not user_id:
        return

    conversation_id_store[user_id] = conversation_id


async def send_proactive_notification(user_id: Optional[str]) -> None:
    """Retrieve the conversation_id from storage and use it to send the message."""
    conversation_id = conversation_id_store.get(user_id) if user_id else None
    if not conversation_id:
        return

    activity = MessageActivityInput(text="Hey! It's been a while. How are you?")
    await app.send(conversation_id, activity)


def schedule_proactive_notification(user_id: Optional[str], delay_seconds: float) -> None:
    """A stand-in for a real notification queue / background worker."""
    if not user_id:
        return

    async def _deliver() -> None:
        try:
            await asyncio.sleep(delay_seconds)
            await send_proactive_notification(user_id)
        except Exception as error:  # noqa: BLE001
            print(f"[PROACTIVE] Failed to deliver notification: {error}")

    asyncio.create_task(_deliver())


# Installation is just one place to get the conversation_id. Every activity
# carries the conversation id, so any handler can capture it.
@app.on_install_add
async def handle_install_add(ctx: ActivityContext[InstalledActivity]) -> None:
    save_conversation(ctx.activity.from_.aad_object_id, ctx.activity.conversation.id)

    await ctx.send("Hi! I am going to remind you to say something to me soon!")

    # Queue up a proactive notification to be sent in 10 seconds.
    schedule_proactive_notification(ctx.activity.from_.aad_object_id, 10)


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    user_id = ctx.activity.from_.aad_object_id
    save_conversation(user_id, ctx.activity.conversation.id)

    text = (ctx.activity.text or "").strip().lower()

    if "remind" in text:
        await ctx.send("Got it. I will send you a proactive message in 10 seconds.")
        schedule_proactive_notification(user_id, 10)
    elif "notify" in text:
        await send_proactive_notification(user_id)
    else:
        await ctx.send(
            "Welcome to the proactive message bot! Send 'notify' to receive a proactive message "
            "right away, or 'remind' to receive one in 10 seconds."
        )


if __name__ == "__main__":
    asyncio.run(app.start())
