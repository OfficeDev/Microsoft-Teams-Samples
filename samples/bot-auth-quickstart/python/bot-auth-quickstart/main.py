"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import asyncio
import base64
import os
import re
import logging
from typing import Dict, Any, Optional, List

import httpx
from dotenv import load_dotenv
from azure.core.credentials import AccessToken, TokenCredential
from msgraph import GraphServiceClient

from microsoft_teams.api import MessageActivity
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.apps.events import SignInEvent

from storage import (
    LocalStorage,
    UserState,
    storage,
    conversation_references,
    GetUserState,
    SetUserState,
    StoreConversationReference,
)
from ProactiveInstallationService import (
    install_app_for_team_members,
    send_proactive_notification,
)

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
TEAMS_APP_ID = os.environ.get("TEAMS_APP_ID", "")
APP_CATALOG_TEAM_APP_ID = os.environ.get("APP_CATALOG_TEAM_APP_ID", "")


# Token Credential

class TokenCredentialFromString(TokenCredential):
    """Custom token credential that uses an existing access token."""

    def __init__(self, access_token: str):
        self._token = access_token

    def get_token(self, *scopes, **kwargs) -> AccessToken:
        return AccessToken(self._token, 0)


# App Configuration

# Create app with OAuth configuration
app = App(
    client_id=CLIENT_ID if CLIENT_ID else None,
    client_secret=CLIENT_SECRET if CLIENT_SECRET else None,
    tenant_id=TENANT_ID if TENANT_ID else None,
    default_connection_name=CONNECTION_NAME if CONNECTION_NAME else None
)


# Helper Functions

def StripMentionsText(text: str) -> str:
    """Remove bot mentions from message text."""
    if not text:
        return ""
    cleaned = re.sub(r'<at>.*?</at>', '', text)
    return cleaned.strip()

# Auth Helpers      

async def HandleLogout(ctx: ActivityContext) -> bool:
    """Handle logout command."""
    await ctx.sign_out()
    await ctx.send("You have been signed out.")
    return True


async def HandleLogin(ctx: ActivityContext, is_explicit_login_command: bool) -> bool:
    """Handle login command."""
    try:
        await ctx.sign_in()
    except Exception as error:
        logging.error(f"Sign-in error: {error}")
    return True


async def HandleTokenConfirmation(
    ctx: ActivityContext, text_lower: str, user_id: str, user_state: UserState
) -> bool:
    """Handle token confirmation response (Yes/No)."""
    if not user_state.waiting_for_token_confirmation:
        return False

    if text_lower == "yes":
        if user_state.token:
            await ctx.send(f"Here is your token: {user_state.token}")
        else:
            await ctx.send(
                "Token is available but not accessible in this context. "
                "Authentication is working correctly."
            )
    elif text_lower == "no":
        await ctx.send("Thank you.")
    else:
        return False

    # Reset state
    user_state.waiting_for_token_confirmation = False
    SetUserState(user_id, user_state)
    return True


async def DisplayUserDetails(
    ctx: ActivityContext, token: str, user_id: str
) -> None:
    """Display user details after successful sign-in."""
    user_state = GetUserState(user_id)
    user_state.token = token

    # Create Graph client using the SSO token
    credential = TokenCredentialFromString(token)
    graph_client = GraphServiceClient(credentials=credential)

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

        # Ask for token confirmation
        await ctx.send("Would you like to view your token?\n\n**Yes or No**")

        user_state.waiting_for_token_confirmation = True
        SetUserState(user_id, user_state)

    except Exception as e:
        logging.error(f"Error fetching user details: {e}")


# Event Handlers


@app.on_install_add
async def OnInstallAdd(ctx: ActivityContext):
    """Handle install add event - welcome new users."""
    StoreConversationReference(ctx.activity)

    await ctx.send("Welcome to TeamsBot with Proactive Installation!")


@app.event("sign_in")
async def OnSignIn(event: SignInEvent):
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
    await DisplayUserDetails(ctx, token, user_id)


@app.on_message
async def HandleMessage(ctx: ActivityContext[MessageActivity]):
    """Handle all messages."""
    raw_text = ctx.activity.text if ctx.activity.text else ""
    text = StripMentionsText(raw_text)
    text_lower = text.lower().strip()

    from_user = (
        getattr(ctx.activity, 'from_', None)
        or getattr(ctx.activity, 'from_property', None)
    )
    user_id = from_user.id if from_user else "unknown"
    user_state = GetUserState(user_id)

    # Store conversation reference for proactive messaging
    StoreConversationReference(ctx.activity)

    # Handle logout/signout commands
    if text_lower in ("logout", "signout"):
        await HandleLogout(ctx)
        return

    # Handle install command - install app for team members
    if text_lower == "install":
        await install_app_for_team_members(
            ctx, CLIENT_ID, CLIENT_SECRET, TENANT_ID,
            APP_CATALOG_TEAM_APP_ID, TEAMS_APP_ID, conversation_references
        )
        return

    # Handle send command - send proactive messages
    if text_lower == "send":
        await send_proactive_notification(
            ctx, app.id, CLIENT_ID, CLIENT_SECRET, TENANT_ID,
            APP_CATALOG_TEAM_APP_ID, TEAMS_APP_ID, conversation_references
        )
        return

    # Check if user is responding to token confirmation
    if await HandleTokenConfirmation(ctx, text_lower, user_id, user_state):
        return

    # Handle login/signin commands or any other message (triggers login)
    is_explicit_login_command = text_lower in ("login", "signin")
    await HandleLogin(ctx, is_explicit_login_command)


if __name__ == "__main__":
    asyncio.run(app.start())
