import os
import json
from botbuilder.core import TurnContext, CardFactory, MessageFactory
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from botbuilder.schema import Attachment, Activity
import logging
import requests
from bots.oauth2 import get_access_token


class TeamsBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        self.base_url = os.environ.get("BaseUrl")
        self.app_id = os.environ.get("MicrosoftAppId")
        self.secret = os.environ.get("MicrosoftAppPassword")

    async def on_message_activity(self, turn_context: TurnContext):
        members = []

        try:
            # Extract meeting and participant details from the message context
            meeting_id = turn_context.activity.channel_data["meeting"]["id"]
            tenant_id = turn_context.activity.channel_data["tenant"]["id"]
            participant_id = turn_context.activity.from_property.aad_object_id
            
            # Fetch the access token
            client_id = self.app_id  # Add your client_id
            client_secret = self.secret  # Add your client_secret
            tenant_id_oauth = tenant_id  # Add your tenant_id for OAuth

            # Get the access token
            access_token = get_access_token(client_id, client_secret, tenant_id_oauth)

            if not access_token:
                await turn_context.send_activity(
                    "Failed to fetch access token. Please check your credentials."
                )
                return

            # Fetch the meeting participant details using TeamsInfo SDK
            participant = await TeamsInfo.get_meeting_participant(
                turn_context, meeting_id, participant_id, tenant_id
            )

            # Log the participant details (You can use this info for further logic)
            if participant and participant.user:
                logging.info(
                    f"Participant ID: {participant.user.id}, Name: {participant.user.name}"
                )
                members.append(
                    {"id": participant.user.id, "name": participant.user.name}
                )
            else:
                logging.warning(f"Participant not found or is not in the meeting.")

            # Optionally send a response or notification to the user
            if members:
                await turn_context.send_activity(f"Participants: {members}")
            else:
                await turn_context.send_activity(
                    "No participants found in the meeting."
                )

        except Exception as e:
            logging.error(f"Error in on_message_activity: {e}")
            await turn_context.send_activity(
                "An error occurred while fetching participant details."
            )

        if turn_context.activity.value is None:
            TurnContext.remove_recipient_mention(turn_context.activity)
            user_text = (
                turn_context.activity.text.strip() if turn_context.activity.text else ""
            )

            if user_text == "SendNotification":
                try:
                    paged_members_result = await TeamsInfo.get_paged_members(
                        turn_context
                    )
                    meeting_members = paged_members_result.members or []
                    # tenant_id = turn_context.activity.channel_data.get(
                    #     "tenant", {}
                    # ).get("id")

                    for member in meeting_members:
                        try:
                            participant_detail = (
                                await TeamsInfo.get_meeting_participant(
                                    turn_context, meeting_id, participant_id, tenant_id
                                )
                            )

                            if participant_detail:
                                members.append(
                                    {
                                        "id": participant_detail.user.id,
                                        "name": participant_detail.user.name,
                                    }
                                )

                        except Exception as error:
                            logging.error(
                                f"Failed to get meeting participant for member {member.name}: {error}"
                            )
                            continue

                    if members:
                        card = self.create_members_adaptive_card(members)
                        await turn_context.send_activity(Activity(attachments=[card]))
                    else:
                        await turn_context.send_activity(
                            "No members are currently in the meeting."
                        )

                except Exception as err:
                    logging.error("[SendNotification Error]: %s", err)
                    await turn_context.send_activity(
                        "An error occurred while sending notifications."
                    )

            else:
                await turn_context.send_activity(
                    "Please type SendNotification to send In-meeting notifications."
                )

        elif turn_context.activity.value.get("Type") == "StageViewNotification":
            selected_members = turn_context.activity.value.get("Choice", "").split(",")
            await self.stage_view(turn_context, meeting_id, selected_members)

        elif turn_context.activity.value.get("Type") == "AppIconBadging":
            selected_members = turn_context.activity.value.get("Choice", "").split(",")
            await self.visual_indicator(turn_context, meeting_id, selected_members)

    async def stage_view(
        self, context: TurnContext, meeting_id: str, selected_members: list[str]
    ):
        notification_information = {
            "type": "targetedMeetingNotification",
            "value": {
                "recipients": selected_members,
                "surfaces": [
                    {
                        "surface": "meetingStage",
                        "contentType": "task",
                        "content": {
                            "value": {
                                "height": "300",
                                "width": "400",
                                "title": "Targeted meeting Notification",
                                "url": f"{self.base_url}/tab",
                            }
                        },
                    }
                ],
            },
        }

        try:
            await TeamsInfo.send_meeting_notification(
                context, notification_information, meeting_id
            )
        except Exception as e:
            print(f"Error sending stage view notification: {e}")

    async def visual_indicator(
        context: TurnContext, meeting_id: str, selected_members: list[str]
    ):
        notification_information = {
            "type": "targetedMeetingNotification",
            "value": {
                "recipients": selected_members,
                "surfaces": [{"surface": "meetingTabIcon"}],
            },
        }

        try:
            await TeamsInfo.send_meeting_notification(
                context, notification_information, meeting_id
            )
        except Exception as e:
            print(f"Error sending visual indicator: {e}")

    def create_members_adaptive_card(members: list[dict]) -> Attachment:
        # Build list of choices
        choices = [
            {"title": member["name"], "value": member["id"]} for member in members
        ]

        # Serialize choices to JSON
        choices_json = json.dumps(choices, indent=2)

        # Replace placeholder with JSON string
        final_card_json = adaptive_card_template.replace("__CHOICES__", choices_json)

        # Parse string to dict
        card_payload = json.loads(final_card_json)

        # Return adaptive card
        return CardFactory.adaptive_card(card_payload)

    def get_meeting_participants(access_token, meeting_id):
        url = f"https://graph.microsoft.com/v1.0/me/onlineMeetings/{meeting_id}/participants"

        headers = {
            "Authorization": f"Bearer {access_token}",
            "Content-Type": "application/json",
        }

        response = requests.get(url, headers=headers)

        if response.status_code == 200:
            participants = response.json().get("value", [])
            return participants  # List of participants
        else:
            print(
                f"Error fetching meeting participants: {response.status_code} - {response.text}"
            )
        return None


adaptive_card_template = """
{
  "type": "AdaptiveCard",
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "version": "1.5",
  "body": [
    {
      "type": "TextBlock",
      "text": "Following people are in meeting. Please select members to send notification.",
      "wrap": true
    },
    {
      "type": "Input.ChoiceSet",
      "id": "Choice",
      "style": "expanded",
      "isMultiSelect": true,
      "wrap": true,
      "choices": __CHOICES__
    }
  ],
  "actions": [
    {
      "type": "Action.Submit",
      "id": "submit",
      "title": "Stage Notification",
      "data": {
        "Type": "StageViewNotification"
      }
    },
    {
      "type": "Action.Submit",
      "id": "submit",
      "title": "App Icon Badging",
      "data": {
        "Type": "AppIconBadging"
      }
    }
  ]
}
"""
