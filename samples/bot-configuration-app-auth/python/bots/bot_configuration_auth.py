# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import ActivityHandler, InvokeResponse
from botbuilder.schema import Activity, Attachment
from http import HTTPStatus
import base64

class TeamsBot(ActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_members_added_activity(self, members_added, turn_context):
        """
        Handles the event when new members are added to the conversation.
        Sends a welcome message with an adaptive card.
        """
        image_path = 'Images/configbutton.png'
        with open(image_path, 'rb') as image_file:
            image_base64 = base64.b64encode(image_file.read()).decode('utf-8')

        adaptive_card = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card.",
                    "wrap": True,
                    "size": "large",
                    "weight": "bolder"
                },
                {
                    "type": "Image",
                    "url": f"data:image/jpeg;base64,{image_base64}",
                    "size": "auto"
                }
            ],
            "fallbackText": "This card requires Adaptive Card support."
        }

        card_attachment = Attachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=adaptive_card
        )

        activity = Activity(
            attachments=[card_attachment],
            text=""
        )

        await turn_context.send_activity(activity)

    async def on_message_activity(self, turn_context):
        """
        Handles incoming message activities.
        """
        await super().on_message_activity(turn_context)

    async def on_invoke_activity(self, turn_context):
        """
        Handles invoke activities for bot configuration.
        """
        if turn_context.activity.name == "config/fetch":
            return await self.handle_teams_config_fetch(turn_context)
        elif turn_context.activity.name == "config/submit":
            return await self.handle_teams_config_submit(turn_context)
        else:
            return InvokeResponse(status=HTTPStatus.NOT_IMPLEMENTED)

    async def handle_teams_config_fetch(self, turn_context):
        """
        Handles the configuration fetch request.
        """
        response = {
            "config": {
                "type": "auth",
                "suggestedActions": {
                    "actions": [
                        {
                            "type": "openUrl",
                            "value": "https://example.com/auth",
                            "title": "Sign in to this app"
                        }
                    ]
                },
            },
        }
        return InvokeResponse(status=HTTPStatus.OK, body=response)

    async def handle_teams_config_submit(self, turn_context):
        """
        Handles the configuration submit request.
        """
        response = {
            "config": {
                "type": "message",
                "value": "You have chosen to finish setting up bot",
            },
        }
        return InvokeResponse(status=HTTPStatus.OK, body=response)