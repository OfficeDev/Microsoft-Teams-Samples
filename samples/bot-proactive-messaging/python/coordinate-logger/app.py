# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
Coordinate Logger Bot - Logs conversation coordinates for proactive messaging.

This bot captures conversation references when installed in:
1. User (1-on-1) conversations
2. Channel conversations and threads

The logged coordinates (Service URL, Tenant ID, Conversation ID) are printed
to the console and can be used with the proactive-cmd tool to send messages.
"""

import asyncio
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.api import MessageActivity, InstalledActivity, ConversationUpdateActivity
from config import DefaultConfig

# Load configuration
CONFIG = DefaultConfig()

# Create the Teams AI application
app = App(
    storage={},
    client_id=CONFIG.CLIENT_ID,
    client_secret=CONFIG.CLIENT_SECRET
)


def log_conversation_coordinates(activity, event_type: str):
    """Log the conversation coordinates needed for proactive messaging."""
    conversation = activity.conversation
    
    # Determine conversation type
    conversation_type = get_conversation_type(conversation)
    
    print("\n" + "=" * 80)
    print("# Terminal version command")
    print_proactive_command(activity, conversation_type)
    print("=" * 80 + "\n")


def get_conversation_type(conversation) -> str:
    """Determine the type of conversation."""
    if not conversation:
        return "Unknown"
    
    # Check conversation type
    if hasattr(conversation, 'conversation_type'):
        if conversation.conversation_type == 'personal':
            return "User (1-on-1)"
        elif conversation.conversation_type == 'channel':
            return "Channel"
        elif conversation.conversation_type == 'groupChat':
            return "Group Chat"
    
    # Fallback: check conversation ID pattern
    if conversation.id:
        if ':' in conversation.id and 'channel' in conversation.id.lower():
            return "Channel"
        elif conversation.id.startswith('a:'):
            return "User (1-on-1)"
    
    return "Unknown"


def print_proactive_command(activity, conversation_type: str):
    """Print the command to send a proactive message using proactive-cmd."""
    conversation_id = activity.conversation.id
    tenant_id = activity.conversation.tenant_id
    service_url = activity.service_url.rstrip('/')
    
    # Get actual bot credentials from config
    bot_id = CONFIG.CLIENT_ID
    bot_password = CONFIG.CLIENT_SECRET
    
    # Escape special characters for command line
    conversation_id_safe = conversation_id.replace('"', '""')
    
    # 1-on-1 command
    print(f'1. ---> python app.py sendUserMessage --app-id="{bot_id}" --app-password="{bot_password}" --tenant-id="{tenant_id}" --service-url="{service_url}" --conversation-id="{conversation_id_safe}" --message="Send Message To User (1-on-1)!"')
    print()
    # Channel command
    print(f'2. ---> python app.py sendChannelThread --app-id="{bot_id}" --app-password="{bot_password}" --tenant-id="{tenant_id}" --service-url="{service_url}" --conversation-id="{conversation_id_safe}" --message="Send Message To Channel Thread!"')


@app.on_message
async def on_message(context: ActivityContext[MessageActivity]):
    """Capture conversation ID when any message is received."""
    log_conversation_coordinates(context.activity, "Message Received")


if __name__ == "__main__":
    asyncio.run(app.start(port=CONFIG.PORT))
