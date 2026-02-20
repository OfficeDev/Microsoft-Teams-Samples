"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import asyncio
import base64
import os
import re
import logging

from dotenv import load_dotenv

from microsoft_teams.api import MessageActivity
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.apps.events import SignInEvent

from storage import (
    UserState,
    get_user_state,
    set_user_state,
)
from group_chat_service import get_graph_client, handle_chats_command

# Load environment variables from env/.env.local
env_path = os.path.join(os.path.dirname(__file__), "env", ".env.local")
if os.path.exists(env_path):
    load_dotenv(env_path)
else:
    load_dotenv()  # Fallback to default .env

# Configure logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")

# Suppress verbose SDK logging
logging.getLogger("microsoft_teams").setLevel(logging.WARNING)
logging.getLogger("httpx").setLevel(logging.WARNING)
logging.getLogger("httpcore").setLevel(logging.WARNING)

# Configuration
CLIENT_ID = os.environ.get("CLIENT_ID", "")
CLIENT_SECRET = os.environ.get("CLIENT_SECRET", "")
TENANT_ID = os.environ.get("TENANT_ID", "")
CONNECTION_NAME = os.environ.get("CONNECTION_NAME", "")


# App Configuration

# Create app with OAuth configuration
app = App(
    client_id=CLIENT_ID if CLIENT_ID else None,
    client_secret=CLIENT_SECRET if CLIENT_SECRET else None,
    tenant_id=TENANT_ID if TENANT_ID else None,
    default_connection_name=CONNECTION_NAME if CONNECTION_NAME else None
)


# Helper Functions

def strip_mentions_text(text: str) -> str:
    """Remove bot mentions from message text."""
    if not text:
        return ""
    cleaned = re.sub(r'<at>.*?</at>', '', text)
    return cleaned.strip()

# Auth Helpers      

async def handle_logout(ctx: ActivityContext) -> bool:
    """Handle logout command."""
    await ctx.sign_out()
    await ctx.send("You have been signed out.")
    return True


async def handle_login(ctx: ActivityContext, is_explicit_login_command: bool) -> bool:
    """Handle login command."""
    try:
        await ctx.sign_in()
        return True
    except Exception as error:
        logging.error(f"Sign-in error: {error}")
        if is_explicit_login_command:
            print("There was a problem signing you in. Please try again in a moment.")
        return False


async def display_user_details(
    ctx: ActivityContext, token: str, user_id: str
) -> None:
    """Display user details after successful sign-in."""
    user_state = get_user_state(user_id)
    user_state.token = token

    # Create Graph client using the SSO token
    graph_client = get_graph_client(token)

    try:
        # Get user profile using Graph SDK
        me = await graph_client.me.get()
        display_name = me.display_name if me else "Unknown"
        user_principal_name = me.user_principal_name if me else "Unknown"
        job_title = me.job_title if me and me.job_title else None

        # Send user details
        await ctx.send(
            f"You're logged in as {display_name} ({user_principal_name}); "
            f"your job title is: {job_title}; your photo is:"
        )

        # Fetch profile photo using Graph SDK
        try:
            photo_bytes = await graph_client.me.photo.content.get()

            if photo_bytes:
                base64_photo = base64.b64encode(photo_bytes).decode('utf-8')
                photo_html = (
                    f'<img src="data:image/jpeg;base64,{base64_photo}" '
                    f'alt="Profile Photo" />'
                )
                await ctx.send(photo_html)
        except Exception:
            pass

    except Exception as e:
        logging.error(f"Error fetching user details: {e}")


# Event Handlers


@app.on_install_add
async def on_install_add(ctx: ActivityContext):
    """Handle install add event - welcome new users."""
    await ctx.send("Welcome to the Bot Auth Quickstart.")


@app.event("sign_in")
async def on_sign_in(event: SignInEvent):
    """Handle successful sign in event."""
    ctx = event.activity_ctx
    token = event.token_response.token
    from_user = (
        getattr(ctx.activity, 'from_', None)
        or getattr(ctx.activity, 'from_property', None)
    )
    user_id = from_user.id if from_user else "unknown"

    await ctx.send("You have been signed in successfully!")

    # Automatically display user details after successful sign-in
    await display_user_details(ctx, token, user_id)


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """Handle all messages."""
    raw_text = ctx.activity.text if ctx.activity.text else ""
    text = strip_mentions_text(raw_text)
    text_lower = text.lower().strip()

    from_user = (
        getattr(ctx.activity, 'from_', None)
        or getattr(ctx.activity, 'from_property', None)
    )
    user_id = from_user.id if from_user else "unknown"
    user_state = get_user_state(user_id)

    # Handle logout/signout commands
    if text_lower in ("logout", "signout"):
        await handle_logout(ctx)
        return

    # Handle chats command — lists group chats
    if text_lower == "chats":
        user_state = get_user_state(user_id)
        if user_state.token:
            await handle_chats_command(ctx, user_state.token)
        else:
            await ctx.send(
                "You need to sign in first. Type **login** to authenticate."
            )
        return

    # Handle login/signin commands or any other message (triggers login)
    is_explicit_login_command = text_lower in ("login", "signin")
    await handle_login(ctx, is_explicit_login_command)


if __name__ == "__main__":
    asyncio.run(app.start())
