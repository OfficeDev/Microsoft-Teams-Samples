# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import ActivityTypes, Activity
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from server.models import (
    adaptive_card
)

class TeamsBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        self.base_url = os.getenv("BaseUrl")  # Get base URL from environment variables

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        # Greet new users when they are added to the conversation
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Hello and welcome! With this sample you can send task requests to your manager, and your manager can approve/reject the request."
                )

    async def on_message_activity(self, turn_context: TurnContext):
        channel_data = turn_context.activity.channel_data  # Channel metadata as dict

        # Suppress Teams schema warnings by accessing known attributes
        source = channel_data.get("source", None)  # Accessing 'source' to avoid warning
        legacy = channel_data.get("legacy", None)  # Accessing 'legacy' to avoid warning

        await self.start_task_management(turn_context)  # Show options card to user

    async def on_invoke_activity(self, turn_context: TurnContext):
        activity = turn_context.activity
        user = activity.from_property  # Current user object

        # Handle Adaptive Card submit actions
        if activity.name == "adaptiveCard/action":
            all_members = await TeamsInfo.get_members(turn_context)  # Get team members
            card = await adaptive_card.select_response_card(
                turn_context, user, all_members  # Build response card based on user
            )
            return adaptive_card.invoke_response(card)  # Return card as response

    async def start_task_management(self, turn_context: TurnContext):
        card = adaptive_card.option_inc()  # Load initial option card
        await turn_context.send_activity(
            Activity(
                type=ActivityTypes.message,
                attachments=[CardFactory.adaptive_card(card)],  # Send Adaptive Card
            )
        )

