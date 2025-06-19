# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from dotenv import load_dotenv
from botbuilder.core import TurnContext, ActivityHandler, ConversationState, MemoryStorage
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import ConversationReference

# Load environment variables
load_dotenv(dotenv_path=os.path.join(os.path.dirname(__file__), "../env/.env.local"))

class TeamsBot(TeamsActivityHandler):
    def __init__(self, conversation_references: dict):
        super().__init__()
        self.conversation_references = conversation_references

    async def on_conversation_update_activity(self, turn_context: TurnContext):
        self._add_conversation_reference(turn_context.activity)
        await super().on_conversation_update_activity(turn_context)

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                welcome_message = (
                    f"Welcome to the Proactive Bot sample. Navigate to "
                    f"{os.getenv('PROVISIONOUTPUT__BOTOUTPUT__SITEENDPOINT')}/api/notify "
                    "to proactively message everyone who has previously messaged this bot."
                )
                await turn_context.send_activity(welcome_message)

    async def on_message_activity(self, turn_context: TurnContext):
        self._add_conversation_reference(turn_context.activity)
        await turn_context.send_activity(
            f"You sent '{turn_context.activity.text}'. Navigate to "
            f"{os.getenv('PROVISIONOUTPUT__BOTOUTPUT__SITEENDPOINT')}/api/notify "
            "to proactively message everyone who has previously messaged this bot."
        )

    def _add_conversation_reference(self, activity):
        conversation_reference = TurnContext.get_conversation_reference(activity)
        self.conversation_references[
            conversation_reference.conversation.id
        ] = conversation_reference
