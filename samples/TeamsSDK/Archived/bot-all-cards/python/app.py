# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from pathlib import Path
from dotenv import load_dotenv
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.api import MessageActivity, MessageActivityInput, Attachment
from cards import cards

# Load environment variables from .env file
env_path = Path(__file__).parent / ".env"
if env_path.exists():
    load_dotenv(env_path)

# Also try to load .localConfigs (Teams Toolkit)
local_configs_path = Path(__file__).parent / ".localConfigs"
if local_configs_path.exists():
    load_dotenv(local_configs_path)

app = App()

# All supported card types
CARD_TYPES = [
    'AdaptiveCard', 'HeroCard', 'ListCard', 'Office365',
    'CollectionCard', 'SignIn', 'ThumbnailCard', 'OAuth'
]


@app.on_message
async def on_message(context: ActivityContext[MessageActivity]):
    activity = context.activity
    text = (activity.text or "").strip()

    # Remove bot mentions (e.g., @BotName) from the message text
    for entity in activity.entities or []:
        if entity.type == "mention":
            text = text.replace(f"<at>{entity.text}</at>", "").strip()

    if not text:
        await context.send(
            "Welcome to Cards Bot. This bot will introduce you to different types of cards. "
            "Please select the cards from given options."
        )
        await send_suggested_cards(context)
        return

    # Process the selected card type and send the corresponding card
    if text in CARD_TYPES:
        if text == 'AdaptiveCard':
            message = MessageActivityInput(text="").add_card(cards.adaptive_Card())
            await context.send(message)

        elif text == 'HeroCard':
            card = cards.hero_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'Office365':
            card = cards.O365_connector_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'ThumbnailCard':
            card = cards.thumbnail_card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'SignIn':
            card = cards.signin_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'OAuth':
            card = cards.oauth_Card('connection-name')
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'ListCard':
            card = cards.list_Card()
            attachment = Attachment(content_type=card['contentType'], content=card['content'])
            message = MessageActivityInput(text="").add_attachments(attachment)
            await context.send(message)

        elif text == 'CollectionCard':
            message = MessageActivityInput(text="").add_card(cards.collection_Card())
            await context.send(message)

        await context.send(f"You have selected <b>{text}</b>")

    await send_suggested_cards(context)


async def send_suggested_cards(context):
    hero_card_content = {
        'title': 'Please select a card from given options.',
        'buttons': [
            {'type': 'imBack', 'title': card_type, 'value': card_type}
            for card_type in CARD_TYPES
        ]
    }
    attachment = Attachment(content_type='application/vnd.microsoft.card.hero', content=hero_card_content)
    message = MessageActivityInput(text="").add_attachments(attachment)
    await context.send(message)
