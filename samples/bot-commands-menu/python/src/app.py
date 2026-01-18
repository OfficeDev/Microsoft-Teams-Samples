# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import json
import os
import sys
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.common import LocalStorage
from microsoft_teams.api import MessageActivity, MessageActivityInput, Attachment
from azure.identity import ManagedIdentityCredential
from config import Config

config = Config()

# Load card templates
with open("../resources/flightsDetails.json", "r") as f:
    flights_details_card = json.load(f)

with open("../resources/searchHotels.json", "r") as f:
    search_hotels_card = json.load(f)

# Create storage for conversation history
storage = LocalStorage()


def strip_mentions_text(activity):
    """Strip mentions from activity text."""
    text = activity.text or ""
    # Simple mention stripping - Teams SDK v2 provides this utility
    if activity.entities:
        for entity in activity.entities:
            if entity.type == "mention":
                mention_text = entity.text
                if mention_text:  # Check if mention_text is not None
                    text = text.replace(mention_text, "").strip()
    return text


def create_token_factory():
    """Create token factory for managed identity authentication."""
    async def token_factory(scope, tenant_id):
        credential = ManagedIdentityCredential(
            client_id=config.APP_ID
        )
        scopes = scope if isinstance(scope, list) else [scope]
        token_response = credential.get_token(*scopes, tenant_id=tenant_id)
        return token_response.token
    return token_factory


# Configure authentication using TokenCredentials
token_credentials = {
    "client_id": config.APP_ID,
    "token": create_token_factory(),
}

credential_options = (
    token_credentials
    if config.APP_TYPE == "UserAssignedMsi"
    else {}
)

# Create the app with storage
app = App(
    **credential_options,
    storage=storage
)


def get_conversation_state(conversation_id):
    """Get or create conversation state."""
    state = storage.get(conversation_id)
    if not state:
        state = {"count": 0}
        storage.set(conversation_id, state)
    return state


@app.on_message
async def on_message(context: ActivityContext[MessageActivity]):
    """Handle incoming messages."""
    activity = context.activity
    text = strip_mentions_text(activity)

    # Handle different commands
    if "search flights" in text.lower():
        attachment = Attachment(
            content=flights_details_card,
            content_type="application/vnd.microsoft.card.adaptive"
        )
        message = MessageActivityInput(attachments=[attachment])
        await context.send(message)
        return

    if "search hotels" in text.lower():
        attachment = Attachment(
            content=search_hotels_card,
            content_type="application/vnd.microsoft.card.adaptive"
        )
        message = MessageActivityInput(attachments=[attachment])
        await context.send(message)
        return

    if "help" in text.lower():
        await context.send("Displays this help message.")
        return

    if "best time to fly" in text.lower():
        await context.send("Best time to fly to London for a 5-day trip is summer.")
        return

    # Handle hotel search details from activity value (form submission)
    if activity.value:
        value = activity.value
        response = f"""Hotel search details:
Check-in Date: {value.get('checkinDate')},
Checkout Date: {value.get('checkoutDate')},
Location: {value.get('location')},
Number of Guests: {value.get('numberOfGuests')}"""
        await context.send(response)
        return

    if text == "/reset":
        storage.delete(activity.conversation.id)
        await context.send("Ok I've deleted the current conversation state.")
        return

    if text == "/count":
        state = get_conversation_state(activity.conversation.id)
        await context.send(f"The count is {state['count']}")
        return

    if text == "/diag":
        await context.send(json.dumps(activity.__dict__, default=str))
        return

    if text == "/state":
        state = get_conversation_state(activity.conversation.id)
        await context.send(json.dumps(state))
        return

    if text == "/runtime":
        runtime = {
            "pythonversion": sys.version,
            "sdkversion": "2.0.0",  # Teams AI v2
        }
        await context.send(json.dumps(runtime))
        return

    # Default echo behavior
    state = get_conversation_state(activity.conversation.id)
    state["count"] += 1
    storage.set(activity.conversation.id, state)
    await context.send(f"[{state['count']}] you said: {text}")


async def main():
    """Start the application."""
    await app.start()
    print(f"\nBot started, app listening to port 3978")


if __name__ == "__main__":
    asyncio.run(main())
