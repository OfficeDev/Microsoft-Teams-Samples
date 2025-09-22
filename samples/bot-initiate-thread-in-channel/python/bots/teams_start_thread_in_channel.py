# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import logging
from botbuilder.core import CardFactory, TurnContext, MessageFactory
from botbuilder.core.teams import TeamsActivityHandler, TeamsInfo, teams_get_channel_id
from botframework.connector.auth import MicrosoftAppCredentials
from botbuilder.schema import ConversationParameters

# Configure logging
logging.basicConfig(level=logging.INFO)


class TeamsStartThreadInChannel(TeamsActivityHandler):
    def __init__(self):
        self._app_id = os.environ.get("MicrosoftAppId")

    async def on_message_activity(self, turn_context: TurnContext):
        text = turn_context.activity.text.strip().lower()

        if "listchannels" in text:
            await self.list_team_channels(turn_context)
        elif "threadchannel" in text:
            # Retrieve the Teams channel ID from the incoming activity.
            teams_channel_id = teams_get_channel_id(turn_context.activity)
            # Create a new message that will be sent to the new thread.
            message = MessageFactory.text("This will be the start of a new thread")
            await self.teams_create_conversation(
                turn_context, teams_channel_id, message
            )
        elif "getteammember" in text:
            await self.get_team_member(turn_context)
        elif "getpagedteammembers" in text:
            await self.get_paged_team_members(turn_context)
        else:
            await turn_context.send_activity(
                "I didn't understand that command. Please try again."
            )

    async def teams_create_conversation(
        self, turn_context: TurnContext, teams_channel_id: str, message
    ):
        """
        Creates a new conversation within the specified Teams channel and sends the provided message.

        This method initializes the conversation parameters, creates the conversation using the
        connector client, and returns the conversation reference and activity ID.

        """
        # Prepare conversation parameters including channel information and the message to send.
        params = ConversationParameters(
            is_group=True,  # This conversation will be a group conversation (a thread).
            channel_data={
                "channel": {"id": teams_channel_id}
            },  # Channel ID where the thread should be created.
            activity=message,  # The initial message for the new thread.
        )

        # Get the connector client from the turn context (available with CloudAdapter).
        connector_client = turn_context.turn_state.get("ConnectorClient")

        # Create the conversation using the connector client.
        conversation_resource_response = (
            await connector_client.conversations.create_conversation(params)
        )

        # Get the conversation reference from the current turn context and update it with the new conversation ID.
        conversation_reference = TurnContext.get_conversation_reference(
            turn_context.activity
        )
        conversation_reference.conversation.id = conversation_resource_response.id

        # Return the conversation reference and the activity ID of the newly created conversation.
        return [conversation_reference, conversation_resource_response.activity_id]

    async def continue_conversation_callback(self, t):
        """
        This callback function is invoked to send a reply to the newly created thread.
        The bot sends a message to continue the conversation in the new thread that was started.
        """
        # Send a reply to the new thread created, indicating the first reply in the conversation.
        await t.send_activity(
            MessageFactory.text("This will be the first reply to my new thread")
        )

    async def list_team_channels(self, turn_context: TurnContext):
        try:
            team_id = turn_context.activity.channel_data.get("team").get("id")
            channels = await TeamsInfo.get_team_channels(turn_context, team_id)

            if not channels:
                await turn_context.send_activity("No channels found in this team.")
                return

            message = "**List of Channels:**\n"
            for channel in channels:
                channel_name = channel.name if channel.name else "General"
                message += f"- {channel_name}\n"

            await turn_context.send_activity(MessageFactory.text(message))
        except Exception as e:
            logging.error(f"Error listing channels: {e}")
            await turn_context.send_activity(f"Error listing channels: {str(e)}")

    async def get_team_member(self, turn_context: TurnContext):
        try:
            aad_object_id = turn_context.activity.from_property.aad_object_id
            team_id = turn_context.activity.channel_data.get("team").get("id")
            team_member = await TeamsInfo.get_team_member(
                turn_context, team_id, aad_object_id
            )

            if not team_member:
                await turn_context.send_activity("Team member not found.")
                return

            message = f"**User Information:**\n- Name: {team_member.name}\n- Email: {team_member.email or 'N/A'}"
            await turn_context.send_activity(MessageFactory.text(message))
        except Exception as e:
            logging.error(f"Error getting team member: {e}")
            await turn_context.send_activity(f"Error retrieving team member: {str(e)}")

    async def get_paged_team_members(self, turn_context: TurnContext):
        try:
            team_id = turn_context.activity.channel_data.get("team").get("id")
            members = []
            continuation_token = None

            while True:
                current_page = await TeamsInfo.get_paged_team_members(
                    turn_context, team_id, continuation_token
                )
                members.extend(current_page.members)
                continuation_token = current_page.continuation_token

                if not continuation_token:
                    break

            if not members:
                await turn_context.send_activity("No team members found.")
                return

            message = "**Team Members:**\n" + "\n".join(
                f"- {member.name} ({member.email or 'N/A'})" for member in members
            )
            await turn_context.send_activity(MessageFactory.text(message))
        except Exception as e:
            logging.error(f"Error retrieving team members: {e}")
            await turn_context.send_activity(f"Error retrieving team members: {str(e)}")
