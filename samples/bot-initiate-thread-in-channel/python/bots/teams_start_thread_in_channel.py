# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import CardFactory, TurnContext, MessageFactory, BotFrameworkAdapter
from botbuilder.core.teams import TeamsActivityHandler, TeamsInfo, teams_get_channel_id
from botframework.connector.auth import MicrosoftAppCredentials
from botbuilder.schema import CardAction, HeroCard, Mention, ConversationParameters
from botbuilder.schema._connector_client_enums import ActionTypes


class TeamsStartThreadInChannel(TeamsActivityHandler):
    """
    A bot handler class for creating and managing threaded conversations within a Microsoft Teams channel.

    This class extends the `TeamsActivityHandler` to manage activities related to starting new threads
    within Teams channels. It contains methods to handle message activities and initiate conversations
    based on user interactions in the Teams channel.

    """

    def __init__(self, app_id: str):
        """
        Initializes the TeamsStartThreadInChannel bot handler.
        """
        self._app_id = app_id

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming message activities. This method is triggered when a user sends a message
        in the channel where the bot is installed.
        The bot will respond with a message and start a new thread in the current Teams channel.

        """
        # Retrieve the Teams channel ID from the incoming activity.
        teams_channel_id = teams_get_channel_id(turn_context.activity)

        # Create a new message that will be sent to the new thread.
        message = MessageFactory.text("This will be the start of a new thread")

        # Create a new conversation in the channel and send the message to start the thread.
        new_conversation = await self.teams_create_conversation(turn_context, teams_channel_id, message)

        # Continue the conversation and pass a callback function to handle the next step in the conversation.
        await turn_context.adapter.continue_conversation(
            new_conversation[0],  # Conversation reference
            self.continue_conversation_callback,  # Callback function to continue the conversation
            self._app_id  # App ID for authentication
        )

    async def teams_create_conversation(self, turn_context: TurnContext, teams_channel_id: str, message):
        """
        Creates a new conversation within the specified Teams channel and sends the provided message.

        This method initializes the conversation parameters, creates the conversation using the 
        connector client, and returns the conversation reference and activity ID.

        """
        # Prepare conversation parameters including channel information and the message to send.
        params = ConversationParameters(
            is_group=True,  # This conversation will be a group conversation (a thread).
            channel_data={"channel": {"id": teams_channel_id}},  # Channel ID where the thread should be created.
            activity=message  # The initial message for the new thread.
        )

        # Create a connector client to interact with the Teams service.
        connector_client = await turn_context.adapter.create_connector_client(turn_context.activity.service_url)

        # Create the conversation using the connector client.
        conversation_resource_response = await connector_client.conversations.create_conversation(params)

        # Get the conversation reference from the current turn context and update it with the new conversation ID.
        conversation_reference = TurnContext.get_conversation_reference(turn_context.activity)
        conversation_reference.conversation.id = conversation_resource_response.id

        # Return the conversation reference and the activity ID of the newly created conversation.
        return [conversation_reference, conversation_resource_response.activity_id]
    
    async def continue_conversation_callback(self, t):
        """
        This callback function is invoked to send a reply to the newly created thread.
        The bot sends a message to continue the conversation in the new thread that was started.
        """
        # Send a reply to the new thread created, indicating the first reply in the conversation.
        await t.send_activity(MessageFactory.text("This will be the first reply to my new thread"))
