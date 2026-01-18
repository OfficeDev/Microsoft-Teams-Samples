# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio

from azure.identity import ManagedIdentityCredential
from microsoft.teams.api import MessageActivity, TypingActivityInput, MessageActivityInput
from microsoft.teams.apps import ActivityContext, App
from microsoft.teams.cards import AdaptiveCard
from config import Config

config = Config()


def create_token_factory():
    """Create a token factory for UserAssignedMsi authentication."""
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


def create_hero_card() -> AdaptiveCard:
    """
    Creates a Hero Card-style Adaptive Card with a "Say Hello" button.
    Since Teams AI SDK uses Adaptive Cards, we create an equivalent card
    that mimics the Hero Card functionality.
    """
    card = AdaptiveCard.model_validate(
        {
            "type": "AdaptiveCard",
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Let's talk...",
                    "size": "Large",
                    "weight": "Bolder",
                    "wrap": True
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Say Hello",
                    "data": {
                        "msteams": {
                            "type": "messageBack",
                            "displayText": "Hello",
                            "text": "Hello",
                            "value": {"count": 0}
                        }
                    }
                }
            ]
        }
    )
    return card


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """
    Handles incoming messages sent to the bot.
    
    - If the user sends "Hello", responds with a personalized mention.
    - Otherwise, responds with a Hero Card containing a "Say Hello" button.
    """
    await ctx.reply(TypingActivityInput())
    
    # Get the message text and strip any extra spaces
    # The Teams AI SDK automatically removes bot mentions
    user_message = ctx.activity.text.strip() if ctx.activity.text else ""
    
    # If the user's message is "Hello", respond with a personalized mention
    if user_message == "Hello":
        await mention_user(ctx)
    else:
        # If the user's message is something else, respond with a hero card
        card = create_hero_card()
        message = MessageActivityInput(text="").add_card(card)
        await ctx.send(message)


async def mention_user(ctx: ActivityContext[MessageActivity]):
    """
    Sends a reply that mentions the user.
    
    Args:
        ctx: The activity context for the current turn of conversation.
    """
    # Get the user's information from the incoming activity
    user = ctx.activity.from_
    
    # Send a message that mentions the user using the Teams AI SDK
    await ctx.send(MessageActivityInput(text=f"Hi").add_mention(account=user))


if __name__ == "__main__":
    asyncio.run(app.start())
