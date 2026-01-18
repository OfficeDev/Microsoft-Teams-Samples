
import asyncio
import json
import os
from typing import Dict, List, Optional

from azure.identity import ManagedIdentityCredential
from microsoft.teams.api import (
    MessageActivity,
    TypingActivityInput,
    MessageActivityInput,
)
from microsoft.teams.apps import ActivityContext, App
from config import Config

config = Config()

# Storage for conversation references (for proactive messaging)
conversation_references: Dict[str, str] = {}

# Path to adaptive card template
ADAPTIVE_CARD_TEMPLATE = os.path.join(os.path.dirname(__file__), "resources", "UserMentionCardTemplate.json")


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


app = App(
    token=create_token_factory() if config.APP_TYPE == "UserAssignedMsi" else None
)


def create_hero_card(title: str, text: str, buttons: List[Dict], update_count: Optional[int] = None) -> Dict:
    """Create a Hero Card structure."""
    card = {
        "contentType": "application/vnd.microsoft.card.hero",
        "content": {
            "title": title,
            "text": text,
            "buttons": buttons
        }
    }
    return card


def get_welcome_card_buttons(update_value: Optional[Dict] = None) -> List[Dict]:
    """Get the buttons for the welcome/update card."""
    buttons = [
        {
            "type": "messageBack",
            "title": "Message all members",
            "text": "messageallmembers"
        },
        {
            "type": "messageBack",
            "title": "Who am I?",
            "text": "whoami"
        },
        {
            "type": "messageBack",
            "title": "Find me in Adaptive Card",
            "text": "mention me"
        },
        {
            "type": "messageBack",
            "title": "Delete card",
            "text": "deletecard"
        }
    ]
    
    # Add update card button
    update_button = {
        "type": "messageBack",
        "title": "Update Card",
        "text": "updatecardaction",
        "value": json.dumps(update_value if update_value else {"count": 0})
    }
    buttons.append(update_button)
    
    return buttons


async def send_welcome_card(ctx: ActivityContext[MessageActivity]) -> None:
    """Send the welcome Hero Card."""
    buttons = get_welcome_card_buttons()
    card = create_hero_card(
        title="Welcome Card",
        text="Click the buttons.",
        buttons=buttons
    )
    
    activity = MessageActivityInput(
        text="",
        attachments=[card]
    )
    await ctx.send(activity)


async def send_update_card(ctx: ActivityContext[MessageActivity]) -> None:
    """Send an updated Hero Card with incremented count."""
    # Get the value from the activity
    value = ctx.activity.value if hasattr(ctx.activity, 'value') and ctx.activity.value else {}
    
    # Parse value if it's a string
    if isinstance(value, str):
        try:
            value = json.loads(value)
        except json.JSONDecodeError:
            value = {}
    
    # Increment count
    count = value.get("count", 0) + 1
    new_value = {"count": count}
    
    buttons = get_welcome_card_buttons(new_value)
    
    # Remove the last button and re-add with updated value
    buttons = buttons[:-1]
    buttons.append({
        "type": "messageBack",
        "title": "Update Card",
        "text": "updatecardaction",
        "value": json.dumps(new_value)
    })
    
    card = create_hero_card(
        title="Updated card",
        text=f"Update count {count}",
        buttons=buttons
    )
    
    # Create the updated activity
    activity = MessageActivityInput(
        text="",
        attachments=[card]
    )
    
    # Update the existing activity
    if hasattr(ctx.activity, 'reply_to_id') and ctx.activity.reply_to_id:
        await ctx.api.conversations.activities(
            ctx.activity.conversation.id
        ).by_activity_id(ctx.activity.reply_to_id).put(activity)
    else:
        await ctx.send(activity)


async def mention_activity(ctx: ActivityContext[MessageActivity]) -> None:
    """Send a message that @mentions the user."""
    user = ctx.activity.from_
    mention_text = f"Hello <at>{user.name}</at>"
    
    activity = MessageActivityInput(text=mention_text).add_mention(account=user)
    await ctx.send(activity)


async def mention_adaptive_card_activity(ctx: ActivityContext[MessageActivity]) -> None:
    """Send an Adaptive Card with user mentions."""
    try:
        # Get member information
        conversation_id = ctx.activity.conversation.id
        user_id = ctx.activity.from_.id
        
        # Try to get member details from the API
        try:
            members = await ctx.api.conversations.members(conversation_id).get_all()
            member = None
            for m in members:
                if m.id == user_id:
                    member = m
                    break
            
            if not member:
                member = ctx.activity.from_
        except Exception:
            # Fallback to activity sender info
            member = ctx.activity.from_
        
        member_name = member.name if hasattr(member, 'name') and member.name else "User"
        member_upn = member.user_principal_name if hasattr(member, 'user_principal_name') and member.user_principal_name else member.id
        member_aad = member.aad_object_id if hasattr(member, 'aad_object_id') and member.aad_object_id else member.id
        
        # Load and process the adaptive card template
        with open(ADAPTIVE_CARD_TEMPLATE, "r", encoding="utf-8") as f:
            template = json.load(f)
        
        # Replace placeholders in the card body
        for body_item in template.get("body", []):
            if "text" in body_item:
                body_item["text"] = body_item["text"].replace("${userName}", member_name)
        
        # Replace placeholders in the msteams entities
        if "msteams" in template and "entities" in template["msteams"]:
            for entity in template["msteams"]["entities"]:
                if "text" in entity:
                    entity["text"] = entity["text"].replace("${userName}", member_name)
                if "mentioned" in entity:
                    entity["mentioned"]["id"] = entity["mentioned"]["id"].replace("${userUPN}", member_upn)
                    entity["mentioned"]["id"] = entity["mentioned"]["id"].replace("${userAAD}", member_aad)
                    entity["mentioned"]["name"] = entity["mentioned"]["name"].replace("${userName}", member_name)
        
        # Send the adaptive card
        adaptive_card_attachment = {
            "contentType": "application/vnd.microsoft.card.adaptive",
            "content": template
        }
        
        activity = MessageActivityInput(
            text="",
            attachments=[adaptive_card_attachment]
        )
        await ctx.send(activity)
        
    except Exception as e:
        if "MemberNotFoundInConversation" in str(e):
            await ctx.send("Member not found.")
        else:
            raise


async def get_member_info(ctx: ActivityContext[MessageActivity]) -> None:
    """Get and display information about the current user."""
    try:
        conversation_id = ctx.activity.conversation.id
        user_id = ctx.activity.from_.id
        
        # Try to get member details from the API
        try:
            members = await ctx.api.conversations.members(conversation_id).get_all()
            member = None
            for m in members:
                if m.id == user_id:
                    member = m
                    break
            
            if member:
                await ctx.send(f"You are: {member.name}")
            else:
                await ctx.send(f"You are: {ctx.activity.from_.name}")
        except Exception:
            # Fallback to activity sender info
            await ctx.send(f"You are: {ctx.activity.from_.name}")
            
    except Exception as e:
        if "MemberNotFoundInConversation" in str(e):
            await ctx.send("Member not found.")
        else:
            raise


async def message_all_members(ctx: ActivityContext[MessageActivity]) -> None:
    """Send a message to all members in the conversation."""
    try:
        conversation_id = ctx.activity.conversation.id
        
        # Store conversation reference for the sender
        user_id = ctx.activity.from_.id
        conversation_references[user_id] = conversation_id
        
        # Get all members
        try:
            members = await ctx.api.conversations.members(conversation_id).get_all()
            
            for member in members:
                # Skip the bot itself
                if hasattr(ctx.activity, 'recipient') and member.id == ctx.activity.recipient.id:
                    continue
                
                member_conv_id = conversation_references.get(member.id)
                if member_conv_id:
                    # Send proactive message using stored conversation reference
                    activity = MessageActivityInput(
                        text=f"Hello {member.name}. I'm a Teams conversation bot."
                    )
                    await app.send(member_conv_id, activity)
            
            await ctx.send("All messages have been sent")
        except Exception as e:
            await ctx.send(f"Unable to message all members: {str(e)}")
            
    except Exception as e:
        await ctx.send(f"Error: {str(e)}")


async def delete_card_activity(ctx: ActivityContext[MessageActivity]) -> None:
    """Delete the card that triggered this action."""
    try:
        if hasattr(ctx.activity, 'reply_to_id') and ctx.activity.reply_to_id:
            await ctx.api.conversations.activities(
                ctx.activity.conversation.id
            ).by_activity_id(ctx.activity.reply_to_id).delete()
        else:
            await ctx.send("No card to delete.")
    except Exception as e:
        await ctx.send(f"Could not delete the card: {str(e)}")


def remove_recipient_mention(text: str, recipient_name: Optional[str] = None) -> str:
    """Remove bot mention from the text."""
    if not text:
        return ""
    
    import re
    # Remove <at>...</at> tags
    cleaned = re.sub(r'<at>.*?</at>', '', text)
    # Remove extra whitespace
    cleaned = ' '.join(cleaned.split())
    return cleaned.strip()


# Note: on_members_added is not available in Teams AI SDK Python.
# Welcome messages can be handled via on_install_add or within message handlers.
# The bot will welcome users when they first interact with it.


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle incoming message activities."""
    await ctx.reply(TypingActivityInput())
    
    # Store conversation reference for proactive messaging
    user_id = ctx.activity.from_.id
    conversation_references[user_id] = ctx.activity.conversation.id
    
    # Get the text and clean it (remove bot mention)
    raw_text = ctx.activity.text or ""
    recipient_name = ctx.activity.recipient.name if hasattr(ctx.activity, 'recipient') and ctx.activity.recipient else None
    text = remove_recipient_mention(raw_text, recipient_name).lower()
    
    # Handle different commands
    if "mention me" in text:
        await mention_adaptive_card_activity(ctx)
        return
    
    if "mention" in text:
        await mention_activity(ctx)
        return
    
    if "update" in text or "updatecardaction" in text:
        await send_update_card(ctx)
        return
    
    if "message" in text or "messageallmembers" in text:
        await message_all_members(ctx)
        return
    
    if "who" in text or "whoami" in text:
        await get_member_info(ctx)
        return
    
    if "delete" in text or "deletecard" in text:
        await delete_card_activity(ctx)
        return
    
    # Default: send welcome card
    await send_welcome_card(ctx)


if __name__ == "__main__":
    asyncio.run(app.start())
