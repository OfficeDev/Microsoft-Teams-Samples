# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
Microsoft Teams Proactive Messaging Bot

This sample demonstrates how to send proactive messages to users in Microsoft Teams
using the Microsoft Teams SDK for Python. The bot:
1. Stores conversation references when users interact
2. Sends welcome messages when installed  
3. Provides a /api/notify endpoint to send proactive messages
"""

import os
from pathlib import Path
from dotenv import load_dotenv
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.api import MessageActivity, MessageActivityInput, ConversationReference
from starlette.responses import HTMLResponse, PlainTextResponse

# Load environment variables from .env file (root first, then env folder)
# Root .env takes priority for credentials
root_env_path = Path(__file__).parent / ".env"
if root_env_path.exists():
    load_dotenv(root_env_path)

# Also try to load from env folder (Teams Toolkit style)
env_path = Path(__file__).parent / "env" / ".env.local"
if env_path.exists():
    load_dotenv(env_path, override=False)

# Also try .localConfigs (Teams Toolkit pattern)
local_configs_path = Path(__file__).parent / ".localConfigs"
if local_configs_path.exists():
    load_dotenv(local_configs_path, override=False)

# Get configuration from environment
PORT = int(os.environ.get("PORT", 3978))
BASE_URL = os.environ.get("BaseUrl", f"http://localhost:{PORT}")

# Get Bot credentials from environment
BOT_ID = os.environ.get("CLIENT_ID")
BOT_PASSWORD = os.environ.get("CLIENT_SECRET")

# Create simple in-memory storage (matches Node.js LocalStorage pattern)
storage = {}

# Create the Teams AI application with storage and credentials for JWT validation
app = App(
    storage=storage,
    client_id=BOT_ID,
    client_secret=BOT_PASSWORD
)

# Store conversation references for proactive messaging
conversation_references = {}

def build_conversation_reference(activity) -> ConversationReference:
    """Build a ConversationReference from an activity."""
    return ConversationReference(
        activity_id=activity.id,
        user=activity.from_,
        bot=activity.recipient,
        conversation=activity.conversation,
        channel_id=activity.channel_id,
        service_url=activity.service_url,
        locale=activity.locale
    )


def get_notify_instruction() -> str:
    """Get the standard notification instruction message."""
    return (
        f"Navigate to <a href=\"{BASE_URL}/api/notify\">{BASE_URL}/api/notify</a> to proactively message "
        f"everyone who has previously messaged this bot."
    )


# Message handler - processes all incoming messages from Teams
@app.on_message
async def on_message(context: ActivityContext[MessageActivity]):
    """Handle incoming message activities"""
    # Get the message text from the activity
    activity = context.activity
    text = (activity.text or "").strip()
    
    # Store conversation reference for proactive messaging
    conversation_ref = build_conversation_reference(activity)
    conversation_references[conversation_ref.conversation.id] = conversation_ref
    
    # Check if this is a first-time user (welcome message)
    if not text:
        await context.send(f"**Welcome to the Proactive Bot sample!**\n\n{get_notify_instruction()}")
        return
    
    # Echo back the message with instructions for proactive messaging
    await context.send(f"You sent: '{text}'\n\n{get_notify_instruction()}")


# Installation handler - sends welcome when bot is installed
@app.on_install_add
async def on_install(context: ActivityContext):
    """Handle bot installation - send welcome message."""
    activity = context.activity
    
    # Store conversation reference for proactive messaging
    conversation_ref = build_conversation_reference(activity)
    conversation_references[conversation_ref.conversation.id] = conversation_ref
    
    await context.send(f"**Welcome to the Proactive Bot sample!**\n\n{get_notify_instruction()}")


# Proactive notification handler - sends messages to all stored conversations
async def send_proactive_notifications():
    """Send proactive messages to all stored conversation references."""
    sent_count = 0
    
    for conversation_id in conversation_references:
        try:
            activity = MessageActivityInput(text="Proactive Hello!")
            await app.send(conversation_id=conversation_id, activity=activity)
            sent_count += 1
        except Exception:
            pass  # Skip failed conversations
    
    return sent_count, len(conversation_references)


# HTTP handler for /api/notify endpoint using Teams SDK's HTTP plugin
@app.http.get("/api/notify")
async def notify_handler():
    """HTTP endpoint to trigger proactive messages to all stored conversations."""
    if not conversation_references:
        return HTMLResponse(
            content="<html><body><h1>No conversations to notify.</h1>"
                    "<p>Message the bot first to register for notifications.</p></body></html>",
            status_code=200,
        )
    
    sent_count, total_count = await send_proactive_notifications()
    
    return HTMLResponse(
        content=f"<html><body><h1>Proactive messages have been sent.</h1>"
                f"<p>Successfully sent to {sent_count} of {total_count} conversations.</p></body></html>",
        status_code=200,
    )


# Health check endpoint using Teams SDK's HTTP plugin
@app.http.get("/health")
async def health_check():
    """Health check endpoint."""
    return PlainTextResponse(content="OK", status_code=200)


# Main entry point
if __name__ == "__main__":
    import asyncio
    
    async def run():
        # Start the Teams SDK app (includes built-in HTTP server)
        await app.start(port=PORT)
    
    asyncio.run(run())
