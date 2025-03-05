# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core import TurnContext, CardFactory
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
        """
        Handles incoming message activities from the user.

        Args:
            turn_context: Context object for the current turn of the conversation.
        
        Behavior:
            - Removes the mention text (if any) from the activity.
            - Sends an adaptive card depending on the conversation type:
                - Personal scope: Sends a personal adaptive card.
                - Channel scope: Sends a channel/group adaptive card.
            - If the activity contains values (from a form submission), sends a summary of the submitted data.
        """
        activity = self.remove_mention_text(turn_context.activity)

        if activity.text:
            # Handle text messages
            if activity.conversation.conversation_type == "personal":
                # Personal conversation: Send a personal scope adaptive card
                user_card = cards.adaptive_card_for_personal_scope()
                await turn_context.send_activity(Activity(attachments=[user_card]))
            else:
                # Group or channel conversation: Send a channel scope adaptive card
                user_card = cards.adaptive_card_for_channel_scope()
                await turn_context.send_activity(Activity(attachments=[user_card]))
        elif turn_context.activity.value:
            # Handle form submissions (activity.value contains submitted data)
            task_details = turn_context.activity.value
            await turn_context.send_activity(
                f"Task title: {task_details['taskTitle']},\n"
                f"Task description: {task_details['taskDescription']},\n"
                f"Task assigned to: {task_details['userId']}"
            )

    def remove_mention_text(self, activity: Activity):
        """
        Removes mention text (e.g., bot name) from the activity text.

        Args:
            activity: The activity object containing the text and mention entities.
        
        Returns:
            The activity object with the mention text removed.
        
        Behavior:
            - Checks if the activity contains mention entities.
            - Extracts the text of the mention and removes it from the activity's text.
            - Strips extra spaces after removing the mention.
        """
        if activity.entities and activity.entities[0].type == "mention":
            # Check if the first entity is a mention
            mentioned_entity = activity.entities[0]
            if hasattr(mentioned_entity, "mentioned") and mentioned_entity.mentioned.name:
                # Extract the mention text (either 'text' or 'mentioned.name')
                mention_text = mentioned_entity.text if hasattr(mentioned_entity, "text") else mentioned_entity.mentioned.name
                updated_text = activity.text.replace(mention_text, "").strip()
                activity.text = updated_text
        return activity

