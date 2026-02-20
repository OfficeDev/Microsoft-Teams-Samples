"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import logging

from azure.core.credentials import AccessToken, TokenCredential
from msgraph import GraphServiceClient
from msgraph.generated.models.chat_type import ChatType

from microsoft_teams.api import MessageActivity
from microsoft_teams.apps import ActivityContext


class TokenCredentialFromString(TokenCredential):
    """Custom token credential that uses an existing access token."""

    def __init__(self, access_token: str):
        self._token = access_token

    def get_token(self, *scopes, **kwargs) -> AccessToken:
        return AccessToken(self._token, 0)


def get_graph_client(token: str) -> GraphServiceClient:
    """Create a Graph client from an access token string."""
    credential = TokenCredentialFromString(token)
    return GraphServiceClient(credentials=credential)


async def handle_chats_command(ctx: ActivityContext[MessageActivity], token: str) -> None:
    """
    Handles the 'chats' command - lists group chats where the user is a member.
    Uses delegated permissions (Chat.Read) via the user's Graph client.
    """
    # Verify this is a personal scope
    conversation = ctx.activity.conversation
    if conversation and getattr(conversation, 'conversation_type', None) != "personal":
        await ctx.send("This command is only available in personal chat with the bot.")
        return

    await ctx.send("Fetching your group chats...")

    try:
        graph_client = get_graph_client(token)

        # Get all chats for the user
        chats_response = await graph_client.me.chats.get()

        if not chats_response or not chats_response.value or len(chats_response.value) == 0:
            await ctx.send("You don't have any chats.")
            return

        # Filter to only group chats
        group_chats = [
            c for c in chats_response.value
            if c.chat_type == ChatType.Group
        ]

        if len(group_chats) == 0:
            await ctx.send("You are not a member of any group chats.")
            return

        message = f"You are a member of {len(group_chats)} group chat(s):\n\n"

        for chat in group_chats:
            chat_name = chat.topic if chat.topic else "Unnamed Group Chat"
            message += f"- {chat_name}\n"

        await ctx.send(message)

    except Exception:
        await ctx.send("Failed to fetch group chats. Please ensure you have the required permissions.")
