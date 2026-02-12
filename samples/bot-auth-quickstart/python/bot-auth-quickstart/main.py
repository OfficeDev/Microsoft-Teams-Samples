"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import asyncio
import base64
import os
import re
import logging
import json
from typing import Dict, Any, Optional, List

import httpx
from dotenv import load_dotenv
from azure.core.credentials import AccessToken, TokenCredential
from msgraph import GraphServiceClient

from microsoft_teams.api import MessageActivity, ConversationReference, ConversationAccount
from microsoft_teams.api.models.account import Account
from microsoft_teams.api.clients.conversation.params import CreateConversationParams
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.apps.events import SignInEvent

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
APP_CATALOG_TEAM_APP_ID = os.environ.get("APP_CATALOG_TEAM_APP_ID", "")


# Storage and State Management

class LocalStorage:
    """Simple in-memory storage for user and conversation state."""

    def __init__(self):
        self._data: Dict[str, Any] = {}

    def get(self, key: str) -> Optional[Any]:
        return self._data.get(key)

    def set(self, key: str, value: Any) -> None:
        self._data[key] = value

    def delete(self, key: str) -> None:
        if key in self._data:
            del self._data[key]


class UserState:
    """User state for tracking token confirmation flow."""

    def __init__(self):
        self.waiting_for_token_confirmation: bool = False
        self.token: str = ""


# Create storage instance
storage = LocalStorage()

# Store conversation references for proactive messaging
conversation_references: Dict[str, Any] = {}


def GetUserState(user_id: str) -> UserState:
    """Get or create user state for a given user ID."""
    key = f"user_{user_id}"
    state = storage.get(key)
    if state is None:
        state = UserState()
        storage.set(key, state)
    return state


def SetUserState(user_id: str, state: UserState) -> None:
    """Save user state for a given user ID."""
    key = f"user_{user_id}"
    storage.set(key, state)


def StoreConversationReference(activity: Any) -> None:
    """Store conversation reference for proactive messaging."""
    from_user = getattr(activity, 'from_', None) or getattr(activity, 'from_property', None)
    if from_user:
        user_id = getattr(from_user, 'aad_object_id', None) or from_user.id
        conversation = activity.conversation
        conversation_references[user_id] = {
            "userId": from_user.id,
            "conversationId": conversation.id if conversation else "",
            "serviceUrl": getattr(activity, 'service_url', ''),
            "tenantId": getattr(conversation, 'tenant_id', '') if conversation else "",
        }


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


async def InstallAppForUser(user_id: str, user_token: str) -> int:
    """
    Install the Teams app for a specific user using Graph API.
    Uses the user's delegated token.

    :param user_id: The user's Azure AD object ID.
    :param user_token: The user's delegated access token.
    :return: Status code (201=installed, 409=already installed).
    """
    if not APP_CATALOG_TEAM_APP_ID:
        raise ValueError("APP_CATALOG_TEAM_APP_ID is not configured")

    headers = {"Authorization": f"Bearer {user_token}"}

    # Check if app is already installed
    try:
        check_url = (
            f"https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps"
            f"?$expand=teamsAppDefinition"
            f"&$filter=teamsAppDefinition/teamsAppId eq '{APP_CATALOG_TEAM_APP_ID}'"
        )
        async with httpx.AsyncClient() as client:
            check_response = await client.get(check_url, headers=headers)
            if check_response.status_code == 200:
                data = check_response.json()
                if data.get("value") and len(data["value"]) > 0:
                    return 409  # Already installed
    except Exception:
        pass

    # Install app for user
    install_url = f"https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps"
    install_headers = {
        "Authorization": f"Bearer {user_token}",
        "Content-Type": "application/json"
    }
    install_data = json.dumps({
        "teamsApp@odata.bind": f"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{APP_CATALOG_TEAM_APP_ID}"
    })

    async with httpx.AsyncClient() as client:
        install_response = await client.post(
            install_url, headers=install_headers, content=install_data
        )
        if install_response.status_code in (200, 201):
            return 201  # Newly installed
        else:
            error_text = install_response.text
            raise Exception(
                f"Failed to install app: {install_response.status_code} - {error_text}"
            )


async def GetConversationMembers(
    ctx: ActivityContext, user_token: str
) -> List[Dict[str, Any]]:
    """
    Get team/chat members using Graph API with the user's delegated token.

    :param ctx: The activity context.
    :param user_token: The user's delegated access token.
    :return: List of member objects.
    """
    conversation = ctx.activity.conversation
    if not conversation or not hasattr(conversation, 'id'):
        return []

    conv_id = conversation.id
    conversation_type = getattr(conversation, 'conversation_type', None)
    is_group = getattr(conversation, 'is_group', None)
    headers = {"Authorization": f"Bearer {user_token}"}

    if conversation_type == 'channel':
        # In a channel, the conversation ID is like "19:xxx@thread.tacv2"
        # which is NOT a valid team GUID. The team ID comes from channel_data.
        channel_data = getattr(ctx.activity, 'channel_data', None) or {}
        if isinstance(channel_data, dict):
            team_info = channel_data.get('team', {})
            team_id = team_info.get('id', '') if isinstance(team_info, dict) else ''
        else:
            team_info = getattr(channel_data, 'team', None)
            team_id = getattr(team_info, 'id', '') if team_info else ''
        
        if not team_id:
            # Fallback: try the conversation ID itself (some older formats)
            team_id = conv_id.split(';')[0] if ';' in conv_id else conv_id
        
        url = f"https://graph.microsoft.com/v1.0/teams/{team_id}/members"
    elif conversation_type == 'groupChat' or is_group:
        # Group chat scenario
        url = f"https://graph.microsoft.com/v1.0/chats/{conv_id}/members"
    else:
        # Fallback - try as chat
        url = f"https://graph.microsoft.com/v1.0/chats/{conv_id}/members"

    try:
        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=headers)
            if response.status_code == 200:
                data = response.json()
                members = data.get("value", [])
                return members
            else:
                logging.error(
                    f"Members API error: {response.status_code} - {response.text}"
                )
                return []
    except Exception as e:
        logging.error(f"Error getting members: {e}")
        return []


async def CheckAndInstallForAllMembers(
    ctx: ActivityContext, user_token: str
) -> Dict[str, Any]:
    """
    Check and install the app for all members in a team/chat.
    Uses the user's delegated token.

    :param ctx: The activity context.
    :param user_token: The user's delegated access token.
    :return: Dictionary with newInstalls, existing, and errors.
    """
    new_installs = 0
    existing = 0
    errors: List[str] = []

    members = await GetConversationMembers(ctx, user_token)

    if not members:
        raise Exception(
            "Could not retrieve team members. "
            "Make sure this command is run in a team or group chat."
        )

    for member in members:
        user_id = member.get("userId") or member.get("id")
        display_name = member.get("displayName", "Unknown")

        if not user_id:
            continue

        try:
            status_code = await InstallAppForUser(user_id, user_token)
            if status_code == 201:
                new_installs += 1
            elif status_code == 409:
                existing += 1
        except Exception as e:
            logging.error(f"Error installing app for {display_name}: {e}")
            errors.append(f"{display_name}: {str(e)}")

    return {"newInstalls": new_installs, "existing": existing, "errors": errors}


async def SendProactiveMessageToAll(
    ctx: ActivityContext, user_token: str
) -> Dict[str, Any]:
    """
    Send a proactive message to all members in a team/chat.
    Uses the user's delegated token for member lookup and the bot's
    credentials for creating conversations and sending messages.

    Key insights from SDK source:
    - app.api uses a static service URL (https://smba.trafficmanager.net/teams)
      which causes 403 errors for tenant-specific operations.
    - ctx.api uses the service URL from the incoming activity, which is the
      correct tenant-specific URL.
    - ConversationResource model requires activityId and serviceUrl, but the
      Bot Framework API only returns {"id": "..."} for 1:1 conversations
      without an initial activity, causing Pydantic validation errors.

    :param ctx: The activity context.
    :param user_token: The user's delegated access token.
    :return: Dictionary with sent count and errors.
    """
    sent = 0
    errors: List[str] = []

    members = await GetConversationMembers(ctx, user_token)

    if not members:
        raise Exception(
            "Could not retrieve team members. "
            "Make sure this command is run in a team or group chat."
        )

    conversation = ctx.activity.conversation
    tenant_id = getattr(conversation, 'tenant_id', None) or TENANT_ID

    # Use the service URL from the incoming activity (ctx.api) — this is the
    # correct tenant-specific URL, unlike app.api which defaults to a generic one.
    service_url = ctx.api.service_url

    for member in members:
        user_aad_id = member.get("userId") or member.get("id")
        display_name = member.get("displayName", "Unknown")

        if not user_aad_id:
            continue

        try:
            # Create 1:1 conversation using the bot's credentials via ctx.api.
            # We make the HTTP call directly and parse only the 'id' field
            # to avoid the ConversationResource Pydantic validation error.
            params = CreateConversationParams(
                bot=Account(id=app.id, role="bot"),
                members=[Account(id=user_aad_id, role="user")],
                tenant_id=tenant_id,
                is_group=False,
            )
            payload = params.model_dump(by_alias=True)

            try:
                response = await ctx.api.conversations.http.post(
                    f"{service_url}/v3/conversations",
                    json=payload,
                )
                response_data = response.json()
            except httpx.HTTPStatusError as http_err:
                error_body = (
                    http_err.response.text
                    if http_err.response
                    else "no response body"
                )
                logging.error(
                    f"Create conversation failed for {display_name}: "
                    f"{http_err.response.status_code} - {error_body}"
                )
                errors.append(f"{display_name}: {error_body}")
                continue

            conversation_id = response_data.get("id")
            if not conversation_id:
                logging.error(
                    f"No conversation ID returned for {display_name}: "
                    f"{response_data}"
                )
                errors.append(f"{display_name}: No conversation ID returned")
                continue

            # Build a ConversationReference with the correct service URL
            # so ctx.send() dispatches to the right endpoint.
            conv_ref = ConversationReference(
                channel_id="msteams",
                service_url=service_url,
                bot=Account(id=app.id, role="bot"),
                conversation=ConversationAccount(
                    id=conversation_id, conversation_type="personal"
                ),
            )

            await ctx.send("Proactive hello.", conversation_ref=conv_ref)
            sent += 1

        except Exception as e:
            logging.error(f"Failed to send message to {display_name}: {e}")
            errors.append(f"{display_name}: {str(e)}")

    return {"sent": sent, "errors": errors}

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

    # Handle check and install command (requires authentication)
    if text_lower in ("check and install", "install"):
        if not user_state.token:
            await ctx.send("Please sign in first to use this feature.")
            await HandleLogin(ctx, False)
            return

        try:
            await ctx.send("Checking and installing app for all members...")
            result = await CheckAndInstallForAllMembers(
                ctx, user_state.token
            )

            message = (
                f"**Installation Complete**\n\n"
                f"Existing: {result['existing']}\n"
                f"Newly Installed: {result['newInstalls']}"
            )

            if result['errors']:
                message += f"\n\n**Errors:**\n" + "\n".join(result['errors'])

            await ctx.send(message)
        except Exception as e:
            logging.error(f"Error in check and install: {e}")
        return

    # Handle send message command (requires authentication)
    if text_lower in ("send message", "send"):
        if not user_state.token:
            await ctx.send("Please sign in first to use this feature.")
            await HandleLogin(ctx, False)
            return

        try:
            await ctx.send("Sending proactive messages to all members...")

            # First ensure bot is installed for all members
            try:
                install_result = await CheckAndInstallForAllMembers(
                    ctx, user_state.token
                )
                if install_result['newInstalls'] > 0:
                    # Brief pause to allow installations to propagate
                    await asyncio.sleep(2)
            except Exception as install_err:
                logging.warning(f"Pre-send install check failed: {install_err}")

            result = await SendProactiveMessageToAll(ctx, user_state.token)

            message = f"**Messages Sent:** {result['sent']}"

            if result['errors']:
                message += f"\n\n**Errors:**\n" + "\n".join(result['errors'])

            await ctx.send(message)
        except Exception as e:
            logging.error(f"Error in send message: {e}")
        return

    # Check if user is responding to token confirmation
    if await HandleTokenConfirmation(ctx, text_lower, user_id, user_state):
        return

    # Handle login/signin commands or any other message (triggers login)
    is_explicit_login_command = text_lower in ("login", "signin")
    await HandleLogin(ctx, is_explicit_login_command)


if __name__ == "__main__":
    asyncio.run(app.start())
