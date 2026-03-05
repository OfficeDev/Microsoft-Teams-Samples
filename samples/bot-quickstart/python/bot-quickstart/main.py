# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio

from dotenv import load_dotenv
from microsoft_teams.api import MessageActivity, MessageActivityInput
from microsoft_teams.apps import ActivityContext, App

# Load environment variables
load_dotenv()

# Initialize Teams App - automatically uses CLIENT_ID and CLIENT_SECRET from environment variables
# Note: .env file is only required when running on Teams (not needed for local development with devtools)
app = App()

async def send_welcome_message(ctx: ActivityContext) -> None:
    """Sends a welcome message with available commands."""
    welcome_message = (
        "Welcome to the Teams Quickstart Bot!"
    )
    await ctx.send(MessageActivityInput(text=welcome_message))


async def echo_message(ctx: ActivityContext, text: str) -> None:
    """Echo back the user's message."""
    await ctx.send(MessageActivityInput(text=f"**Echo:** {text}"))


async def get_single_member(ctx: ActivityContext[MessageActivity]) -> None:
    """Retrieves and displays information about the current user."""
    try:
        conversationId = ctx.activity.conversation.id
        userId = ctx.activity.from_.id
        user = await ctx.api.conversations.members(conversationId).get(userId)
        await ctx.send(MessageActivityInput(text=f"You are: {user.name}"))
    except Exception as error:
        print(f"Error getting member: {error}")


async def mention_user(ctx: ActivityContext[MessageActivity]) -> None:
    """Mention a user in a message."""
    try:

        conversationId = ctx.activity.conversation.id
        userId = ctx.activity.from_.id

        # Get user info directly from the activity
        user = await ctx.api.conversations.members(conversationId).get(userId)
        
        # Create a text message with user mention
        mention_text = f"<at>{user.name}</at>"
        await ctx.send(MessageActivityInput(
            text=f"Hello {mention_text}",
            entities=[
                {
                    "type": "mention",
                    "text": mention_text,
                    "mentioned": {
                        "id": userId,
                        "name": user.name,
                        "role": "user"
                    }
                }
            ]
        ))
    except Exception as error:
        print(f"Error mentioning user: {error}")


@app.on_conversation_update
async def handle_conversation_update(ctx: ActivityContext) -> None:
    """Handle conversation update events (when bot is added or members join)."""
    members_added = getattr(ctx.activity, 'members_added', [])
    
    for member in members_added:
        # Check if bot was added to the conversation
        if member.id == ctx.activity.recipient.id:
            await send_welcome_message(ctx)


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handles incoming messages and routes to appropriate functions based on message content."""
    # Get message text and normalize it
    text = (ctx.activity.text or "").strip().lower()
        
    # Handle mention me command
    if "mentionme" in text or "mention me" in text:
        await mention_user(ctx)
    # Handle whoami command
    elif "whoami" in text:
        await get_single_member(ctx)
    # Handle welcome command
    elif 'welcome' in text:
        await send_welcome_message(ctx)
    # Handle hi/hello - echo back
    elif "hi" in text or "hello" in text:
        await echo_message(ctx, text)


# Starts the Teams bot application and listens for incoming requests
if __name__ == "__main__":
    asyncio.run(app.start())
