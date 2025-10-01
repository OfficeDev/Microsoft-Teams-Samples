# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core.teams import TeamsActivityHandler, TeamsInfo
import json

# Class to handle meeting context in a Microsoft Teams bot
class MeetingContextApp(TeamsActivityHandler):

    # Event triggered when new members are added to the team
    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        for member in teams_members_added:
            # Avoid greeting the bot itself
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity("Hello and welcome!")
                await turn_context.send_activity(
                    "Please use one of these two commands : <b>Participant Context</b> and <b>Meeting Context</b> <br>Thank you"
                )

    # Event triggered when a message is received
    async def on_message_activity(self, turn_context: TurnContext):
        # Extract meeting and participant details from the message context
        meeting_id = turn_context.activity.channel_data["meeting"]["id"]
        tenant_id = turn_context.activity.channel_data["tenant"]["id"]
        participant_id = turn_context.activity.from_property.aad_object_id

        # Remove bot mentions and extract the user message
        text = TurnContext.remove_recipient_mention(turn_context.activity).strip()

        # Respond based on the user's command
        if "Participant Context" in text:
            # Retrieve participant details
            participant = await TeamsInfo.get_meeting_participant(
                turn_context, meeting_id, participant_id, tenant_id
            )
            formatted_string = self.format_object(participant)
            await turn_context.send_activity(formatted_string)
        elif "Meeting Context" in text:
            # Retrieve meeting details
            meeting_info = await TeamsInfo.get_meeting_info(turn_context)
            formatted_string = self.format_object(meeting_info)
            await turn_context.send_activity(json.dumps(formatted_string))
        else:
            # Default response for unrecognized commands
            await turn_context.send_activity(
                "Please use one of these two commands : <b>Participant Context</b> and <b>Meeting Context</b> <br>Thank you"
            )

    # Helper function to format objects into readable HTML format
    def format_object(self, obj):
        if obj is None:
            return "Error: Object is None"

        # Convert object to dictionary if possible
        if not isinstance(obj, dict):
            obj = obj.__dict__ if hasattr(obj, "__dict__") else {}
        
        formatted_string = ""
        for key, value in obj.items():
            block = f"<b>{key}:</b> <br>"
            store_temporary_formatted_string = ""

            # Handle nested dictionaries
            if isinstance(value, dict):
                for sub_key, sub_value in value.items():
                    store_temporary_formatted_string += (
                        f" <b> &nbsp;&nbsp;{sub_key}:</b> {sub_value}<br/>"
                    )
                formatted_string += block + store_temporary_formatted_string

        return formatted_string
