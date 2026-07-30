# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core import TurnContext
from adaptive_cards import cards

class TeamsBot(TeamsActivityHandler):
    def __init__(self):
        # Constructor for the TeamsBot class. Initializes the parent class (TeamsActivityHandler)
        super().__init__()

    async def on_teams_members_added(
        self,
        teams_members_added: [TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        """
        Handles the event when new members are added to a team.
        
        Args:
            teams_members_added: List of members added to the team.
            team_info: Information about the team where the members were added.
            turn_context: Context object for the current turn of the conversation.
        
        Behavior:
            Sends a welcome message to newly added members, excluding the bot itself.
        """
        for member in teams_members_added:
            # Check if the added member is not the bot itself
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    f"Hello and welcome! With this sample you can see the functionality of people-picker in adaptive card"
                )

    async def on_message_activity(self, turn_context: TurnContext):
        TurnContext.remove_recipient_mention(turn_context.activity)

        if turn_context.activity.text:
            if turn_context.activity.conversation.conversation_type == "personal":
                user_card = cards.adaptive_card_for_personal_scope()
                await turn_context.send_activity(Activity(attachments=[user_card]))
            else:
                user_card = cards.adaptive_card_for_channel_scope()
                await turn_context.send_activity(Activity(attachments=[user_card]))
        elif turn_context.activity.value:
            task_details = turn_context.activity.value
            await turn_context.send_activity(
                f"Task title: {task_details['taskTitle']},\n"
                f"Task description: {task_details['taskDescription']},\n"
                f"Task assigned to: {task_details['userId']}"
            )

