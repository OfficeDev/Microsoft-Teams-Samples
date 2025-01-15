# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import ActivityHandler, MessageFactory,TurnContext

# This class represents a bot that handles message activities and reactions in Microsoft Teams.
class MessageReactionBot(ActivityHandler):
    def __init__(self, app_id, app_password, activity_log):
        """
        Initialize the bot with app credentials and an activity log for storing activity data.
        :param app_id: The Microsoft app ID for the bot.
        :param app_password: The Microsoft app password for the bot.
        :param activity_log: An object to manage logging of activity IDs and their associated data.
        """
        super().__init__()
        self.app_id = app_id
        self.app_password = app_password
        self._log = activity_log  # Reference to an activity log instance for storing and retrieving activities.
        
    async def on_message_activity(self, turn_context:TurnContext, next_handler=None):
        # Remove recipient mention from the activity
        TurnContext.remove_recipient_mention(turn_context.activity)
        text = turn_context.activity.text
        
        """
        Handle incoming message activities.
        :param turn_context: Provides information about the current turn of the conversation.
        :param next_handler: Optional handler to call after the current method.
        """
        # Respond to the user with an echo of their message and log the activity ID.
        await self.send_message_and_log_activity_id(turn_context, f"echo: {text}")   

    async def on_reactions_added(self, reactions_added, turn_context):
        """
        Handle reactions added to messages.
        :param reactions_added: A list of reactions that were added.
        :param turn_context: Provides information about the current turn of the conversation.
        """
        for reaction in reactions_added:
            # Look up the activity in the log using the replyToId (ID of the message being reacted to).
            activity = await self._log.find(turn_context.activity.reply_to_id)

            if not activity:
                # If the activity was not found, notify the user and log the event.
                await self.send_message_and_log_activity_id(
                    turn_context,
                    f"Activity {turn_context.activity.reply_to_id} not found in the log."
                )
            else:
                # If the activity is found, acknowledge the reaction and include the activity's text.
                await self.send_message_and_log_activity_id(
                    turn_context,
                    f" added '{reaction.type}' regarding '{activity.text}'"
                )

    async def on_reactions_removed(self, reactions_removed, turn_context):
        """
        Handle reactions removed from messages.
        :param reactions_removed: A list of reactions that were removed.
        :param turn_context: Provides information about the current turn of the conversation.
        """
        for reaction in reactions_removed:
            # Look up the activity in the log using the replyToId.
            activity = await self._log.find(turn_context.activity.reply_to_id)
            if not activity:
                # If the activity was not found, notify the user.
                await self.send_message_and_log_activity_id(
                    turn_context, 
                    f"Activity {turn_context.activity.reply_to_id} not found in the log."
                )
            else:
                # If the activity is found, acknowledge the removal of the reaction.
                await self.send_message_and_log_activity_id(
                    turn_context, 
                    f"You removed '{reaction.type}' regarding '{activity.text}'"
                )

    async def send_message_and_log_activity_id(self, turn_context, text):
        """
        Send a message to the user and log the activity ID.
        :param turn_context: Provides information about the current turn of the conversation.
        :param text: The message text to send to the user.
        """
        # Create a text message activity.
        reply_activity = MessageFactory.text(text)
        # Send the activity to the user and capture the response containing the activity ID.
        resource_response = await turn_context.send_activity(reply_activity)
        # Log the activity ID and its associated text in the activity log.
        await self._log.append(resource_response.id, reply_activity)
