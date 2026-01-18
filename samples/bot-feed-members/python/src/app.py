# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import sys
import traceback

from azure.identity import ManagedIdentityCredential
from microsoft.teams.api import (
    MessageActivity,
    ConversationUpdateActivity,
    MessageActivityInput,
    Attachment,
    HeroCard,
    CardAction,
    CardActionType
)
from microsoft.teams.apps import ActivityContext, App
from config import Config

config = Config()


def create_token_factory():
    """Create token factory for managed identity authentication."""
    def get_token(scopes, tenant_id=None):
        credential = ManagedIdentityCredential(client_id=config.APP_ID)
        if isinstance(scopes, str):
            scopes_list = [scopes]
        else:
            scopes_list = scopes
        token = credential.get_token(*scopes_list)
        return token.token
    return get_token


# Create the app with optional token authentication
app = App(
    token=create_token_factory() if config.APP_TYPE == "UserAssignedMsi" else None
)


@app.on_conversation_update
async def handle_conversation_update(ctx: ActivityContext[ConversationUpdateActivity]):
    """Handle when members are added to the conversation."""
    activity = ctx.activity
    
    # Check if this is a members added event
    if not activity.members_added or not isinstance(activity.members_added, list):
        return
    
    # Additional safety check for empty array
    if len(activity.members_added) == 0:
        return
    
    # Iterate over all new members added to the conversation
    for member in activity.members_added:
        try:
            # Skip if this is the bot itself being added
            if member.id == activity.recipient.id:
                continue
            
            try:
                # Get detailed information about the new member using Teams SDK V2
                conversation_id = activity.conversation.id
                # In Python Teams SDK V2, use members(conversation_id).get(member_id) to get individual member details
                member_details = await ctx.api.conversations.members(conversation_id).get(member.id)
                
                # Create an Adaptive Card to welcome the new member and display their details
                member_card = {
                    "type": "AdaptiveCard",
                    "body": [
                        {
                            "type": "TextBlock",
                            "text": f"Welcome to the team, {member_details.name}!",
                            "weight": "Bolder",
                            "size": "ExtraLarge"
                        },
                        {
                            "type": "TextBlock",
                            "text": "Here are your details:",
                            "weight": "Bolder",
                            "size": "Large",
                            "spacing": "Large"
                        },
                        {
                            "type": "FactSet",
                            "facts": [
                                {"title": "Name:", "value": member_details.name or "N/A"},
                                {"title": "Email:", "value": member_details.email or "N/A"},
                                {"title": "Given Name:", "value": member_details.given_name or "N/A"},
                                {"title": "Surname:", "value": member_details.surname or "N/A"},
                                {"title": "User Principal Name:", "value": member_details.user_principal_name or "N/A"},
                                {"title": "Tenant Id:", "value": member_details.tenant_id or "N/A"}
                            ],
                            "spacing": "ExtraLarge"
                        }
                    ],
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.4"
                }
                
                # Send the Adaptive Card as an activity to the user
                await ctx.send(MessageActivityInput(
                    attachments=[Attachment(
                        content_type="application/vnd.microsoft.card.adaptive",
                        content=member_card
                    )]
                ))
            except Exception as error:
                print(f"Error getting member details: {error}", file=sys.stderr)
                await ctx.send("I couldn't get your detailed information, but welcome to the team!")
        except Exception as error:
            print(f"Error processing member: {error}", file=sys.stderr)


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """Handle message activities."""
    try:
        # Get the message text and normalize it
        text = ctx.activity.text.strip().lower() if ctx.activity.text else ""
        
        # Remove bot mentions from the text
        # Note: Teams SDK V2 handles mention removal differently than Bot Framework
        # The text should already be cleaned, but we ensure it here
        
        # If the message contains 'list', list the members. Otherwise, send a welcome card.
        if 'list' in text:
            try:
                # Get conversation members using Teams SDK V2
                conversation_id = ctx.activity.conversation.id
                # In Python Teams SDK V2, use members(conversation_id).get_all() to get all members
                members = await ctx.api.conversations.members(conversation_id).get_all()
                await list_members_async(ctx, members)
            except Exception as error:
                print(f"Error getting members: {error}", file=sys.stderr)
                traceback.print_exc()
                await ctx.send("Sorry, I couldn't retrieve the member list. Please try again.")
        else:
            await card_activity_async(ctx)
    except Exception as error:
        print(f"Error in message handler: {error}", file=sys.stderr)
        traceback.print_exc()
        await ctx.send("The bot encountered an error or bug.")


async def list_members_async(ctx: ActivityContext, members):
    """
    Lists all members in the current Teams conversation using an Adaptive Card.
    
    Args:
        ctx: The activity context
        members: The list of conversation members
    """
    # Handle both array and object with members property
    member_array = members if isinstance(members, list) else (getattr(members, 'members', None) or [])
    
    # Construct the Adaptive Card JSON structure with the list of members
    adaptive_card_json = {
        "type": "AdaptiveCard",
        "body": [
            {
                "type": "TextBlock",
                "text": "List of Members",
                "weight": "Bolder",
                "size": "Medium"
            },
            {
                "type": "Container",
                "items": [
                    {"type": "TextBlock", "text": f"- {item.name}", "wrap": True}
                    for item in member_array
                ]
            }
        ],
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4"
    }
    
    # Send the Activity with the list of members to the user
    await ctx.send(MessageActivityInput(
        attachments=[Attachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=adaptive_card_json
        )]
    ))


async def card_activity_async(ctx: ActivityContext):
    """
    Sends a welcome card with an option to list all members.
    
    Args:
        ctx: The activity context
    """
    # Create a Hero Card with action button to list all members
    card = HeroCard(
        title="Welcome card",
        buttons=[
            CardAction(
                type=CardActionType.MESSAGE_BACK,
                title="List all members",
                text="List",
                value=None
            )
        ]
    )
    
    # Send the Hero Card as an activity to the user
    await ctx.send(MessageActivityInput(
        attachments=[Attachment(
            content_type="application/vnd.microsoft.card.hero",
            content=card
        )]
    ))


if __name__ == "__main__":
    try:
        asyncio.run(app.start())
    except Exception as error:
        print(f"Error starting app: {error}", file=sys.stderr)
        traceback.print_exc()
