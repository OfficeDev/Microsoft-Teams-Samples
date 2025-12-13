import os
import json
import logging
from typing import List
from botbuilder.core import TurnContext, CardFactory
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from botbuilder.schema import Attachment, Activity
from botbuilder.schema.teams import MeetingNotificationBase
from botbuilder.schema.teams._models_py3 import TargetedMeetingNotification
from botframework.connector import ConnectorClient
from botframework.connector.auth import MicrosoftAppCredentials


class TeamsBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        self.base_url = os.environ.get("BaseUrl")
        self.app_id = os.environ.get("MicrosoftAppId")
        self.secret = os.environ.get("MicrosoftAppPassword")

    async def on_message_activity(self, turn_context: TurnContext):
        channel_data = turn_context.activity.channel_data
        members = []

        try:
            meeting_id = channel_data.get("meeting", {}).get("id")
            tenant_id = channel_data.get("tenant", {}).get("id")

            if turn_context.activity.value is None:
                TurnContext.remove_recipient_mention(turn_context.activity)
                user_text = turn_context.activity.text.strip()

                if user_text.lower() == "sendnotification":
                    paged_members = await TeamsInfo.get_paged_members(turn_context)

                    for member in paged_members.members:
                        members.append(
                            {
                                "id": member.id,
                                "name": f"{member.given_name or ''} {member.surname or ''}".strip(),
                            }
                        )

                    if members:
                        card = self.create_members_adaptive_card(members)
                        await turn_context.send_activity(Activity(attachments=[card]))
                    else:
                        await turn_context.send_activity(
                            "No members are currently in the meeting."
                        )
                else:
                    await turn_context.send_activity(
                        "Please type `SendNotification` to send in-meeting notifications."
                    )

            elif turn_context.activity.value.get("Type") == "StageViewNotification":
                selected_members = turn_context.activity.value.get("Choice", "").split(
                    ","
                )
                await self.stage_view(turn_context, meeting_id, selected_members)

            elif turn_context.activity.value.get("Type") == "AppIconBadging":
                selected_members = turn_context.activity.value.get("Choice", "").split(
                    ","
                )
                await self.visual_indicator(turn_context, meeting_id, selected_members)

        except Exception as e:
            await turn_context.send_activity(
                "An error occurred while processing your request."
            )
            logging.error(f"Error in on_message_activity: {e}")

    async def stage_view(
        self, context: TurnContext, meeting_id: str, selected_members: list[str]
    ):
        notification_info = {
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
            # Wrap dict in Activity to support serialization
            await TeamsInfo.send_meeting_notification(
                context, Activity(value=notification_info), meeting_id
            )
        except Exception as e:
            print(f"Error sending stage view notification: {e}")

    async def visual_indicator(
        self, context: TurnContext, meeting_id: str, selected_members: list[str]
    ):
        from botbuilder.schema.teams._models_py3 import TargetedMeetingNotification

        notification = TargetedMeetingNotification(
            additional_properties={
                "recipients": [{"userId": member_id} for member_id in selected_members],
                "surfaces": [{"surface": "meetingTabIcon"}],
            }
        )

        try:
            await TeamsInfo.send_meeting_notification(context, notification, meeting_id)
        except Exception as e:
            print(f"Error sending visual indicator: {e}")

    def create_members_adaptive_card(self, members: List[dict]) -> Attachment:
        choices = [
            {"title": member["name"], "value": member["id"]} for member in members
        ]
        choices_json = json.dumps(choices, indent=2)
        final_card_json = adaptive_card_template.replace("__CHOICES__", choices_json)
        card_payload = json.loads(final_card_json)
        return CardFactory.adaptive_card(card_payload)


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
