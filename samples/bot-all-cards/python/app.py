# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from pathlib import Path
from dotenv import load_dotenv
from microsoft.teams.apps import App, ActivityContext
from microsoft.teams.api import MessageActivity, MessageActivityInput, Attachment
from cards import cards

# Load environment variables from .env file
env_path = Path(__file__).parent / ".env"
if env_path.exists():
    load_dotenv(env_path)

# Also try to load .localConfigs (Teams Toolkit)
local_configs_path = Path(__file__).parent / ".localConfigs"
if local_configs_path.exists():
    load_dotenv(local_configs_path)

# Create simple in-memory storage (matches Node.js LocalStorage pattern)
storage = {}

# Create the Teams AI application with storage
app = App(
    storage=storage
)

# Message handler - processes all incoming messages from Teams
@app.on_message
async def on_message(context: ActivityContext[MessageActivity]):
    """Handle incoming message activities"""
    # Get the message text from the activity
    activity = context.activity
    text = (activity.text or "").strip()
    
    # Remove bot mentions (e.g., @BotName) from the message text
    if hasattr(activity, 'entities'):
        for entity in activity.entities or []:
            if entity.type == "mention":
                text = text.replace(f"<at>{entity.text}</at>", "").strip()
    
    # Send welcome message if user sends empty message or first interaction
    if not text:
        await context.send(
            "Welcome to Cards Bot. This bot will introduce you to different types of cards. "
            "Please select the cards from given options."
        )
        await send_suggested_cards(context)
        return
    
    # Define all supported card types
    suggested_cards = [
        'AdaptiveCard', 'HeroCard', 'ListCard', 'Office365',
        'CollectionCard', 'SignIn', 'OAuth', 'ThumbnailCard'
    ]
    
    # Process the selected card type and send the corresponding card
    if text in suggested_cards:
        # Send Adaptive Card (rich interactive card with JSON schema)
        if text == 'AdaptiveCard':
            message = MessageActivityInput(text="").add_card(cards.adaptive_Card())
            await context.send(message)
        
        # Send Hero Card (card with large image, title, and buttons)
        elif text == 'HeroCard':
            card = cards.hero_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send Office 365 Connector Card (card with sections, facts, and actions)
        elif text == 'Office365':
            card = cards.O365_connector_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send Thumbnail Card (compact card with small image and buttons)
        elif text == 'ThumbnailCard':
            card = cards.thumbnail_card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send SignIn Card (authentication card with sign-in button)
        elif text == 'SignIn':
            card = cards.signin_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send OAuth Card (authentication card with OAuth connection)
        elif text == 'OAuth':
            card = cards.oauth_Card('connection-name')
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send List Card (card with multiple list items and sections)
        elif text == 'ListCard':
            card = cards.list_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)
        
        # Send Collection Card (adaptive card for employee events)
        elif text == 'CollectionCard':
            message = MessageActivityInput(text="").add_card(cards.collection_Card())
            await context.send(message)
        
        # Confirm the card selection to the user
        await context.send(f"You have selected <b>{text}</b>")
    
    # Always send the card selection menu after processing the message
    await send_suggested_cards(context)


async def send_suggested_cards(context):
    """Send a hero card with buttons to select different card types"""
    # Create hero card content with all available card options as buttons
    hero_card_content = {
        'title': 'Please select a card from given options.',
        'buttons': [
            {'type': 'imBack', 'title': 'AdaptiveCard', 'value': 'AdaptiveCard'},
            {'type': 'imBack', 'title': 'HeroCard', 'value': 'HeroCard'},
            {'type': 'imBack', 'title': 'ListCard', 'value': 'ListCard'},
            {'type': 'imBack', 'title': 'Office365', 'value': 'Office365'},
            {'type': 'imBack', 'title': 'CollectionCard', 'value': 'CollectionCard'},
            {'type': 'imBack', 'title': 'SignIn', 'value': 'SignIn'},
            {'type': 'imBack', 'title': 'ThumbnailCard', 'value': 'ThumbnailCard'},
            {'type': 'imBack', 'title': 'OAuth', 'value': 'OAuth'}
        ]
    }
    # Create attachment and send the hero card with selection buttons
    attachment = Attachment(content_type='application/vnd.microsoft.card.hero', content=hero_card_content)
    message = MessageActivityInput(text="").add_attachments(attachment)
    await context.send(message)
