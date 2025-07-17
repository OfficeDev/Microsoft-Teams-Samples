# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from dotenv import load_dotenv
from botbuilder.core import TurnContext
from botbuilder.core.teams import TeamsActivityHandler

# Load environment variables from a .env.local file
from pathlib import Path
load_dotenv(dotenv_path=Path(__file__).resolve().parents[1] / "env" / ".env.local")

class BotProactiveMessageTeamsFx(TeamsActivityHandler):
    def __init__(self, conversation_references: dict):
        super().__init__()
        self.conversation_references = conversation_references

    async def on_conversation_update_activity(self, turn_context: TurnContext):
        self._add_conversation_reference(turn_context.activity)
        await super().on_conversation_update_activity(turn_context)

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                site_endpoint = os.getenv("BaseUrl")
                welcome_message = (
                    f"Welcome to the Proactive Bot sample. Navigate to {site_endpoint}/api/notify "
                    f"to proactively message everyone who has previously messaged this bot."
                )
                await turn_context.send_activity(welcome_message)

        await super().on_members_added_activity(members_added, turn_context)

    async def on_message_activity(self, turn_context: TurnContext):
        self._add_conversation_reference(turn_context.activity)
        site_endpoint = os.getenv("BaseUrl")
        await turn_context.send_activity(
            f"You sent '{turn_context.activity.text}'. Navigate to {site_endpoint}/api/notify "
            f"to proactively message everyone who has previously messaged this bot."
        )
        await super().on_message_activity(turn_context)

    def _add_conversation_reference(self, activity):
        conversation_reference = TurnContext.get_conversation_reference(activity)
        self.conversation_references[conversation_reference.conversation.id] = conversation_reference
