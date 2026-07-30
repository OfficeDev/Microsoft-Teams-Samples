#Copyright (c) Microsoft Corporation. All rights reserved.
#Licensed under the MIT License.

import os
from botbuilder.core import TurnContext, CardFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from models.task_module_response import TaskModuleResponseFactory
from botbuilder.schema.teams import TaskModuleTaskInfo

conversation_data_references = {}

class AppCheckInBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        self.base_url = os.getenv("BaseUrl", "")

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Hello and welcome! With this sample you can check in your location (use command 'checkin') "
                    "and view your checked in location (use command 'viewcheckin')."
                )

    async def on_message_activity(self, turn_context: TurnContext):
        text = turn_context.activity.text.strip().lower()

        if text == "viewcheckin":
            current_data = conversation_data_references.get("userDetails")
            if not current_data:
                await turn_context.send_activity("No last check in found")
            else:
                attachments = [
                    CardFactory.adaptive_card(self.adaptive_card_for_user_last_checkin(user))
                    for user in current_data
                ]
                await turn_context.send_activity(Activity(attachments=attachments))
        else:
            card = CardFactory.adaptive_card(self.adaptive_card_for_task_module())
            await turn_context.send_activity(Activity(attachments=[card]))

    async def on_teams_task_module_fetch(self, turn_context: TurnContext, task_module_request):
        data = task_module_request.data
        
        if data.get("id") == "checkin":
            task_info = TaskModuleTaskInfo(
            url=f"{self.base_url}/CheckIn",
            fallback_url=f"{self.base_url}/CheckIn",
            height=350,
            width=350,
            title="Check in details"
        )
        elif data.get("id") == "viewLocation":
            lat = data.get("latitude")
            lon = data.get("longitude")
            task_info = TaskModuleTaskInfo(
                url=f"{self.base_url}/ViewLocation?latitude={lat}&longitude={lon}",
                fallback_url=f"{self.base_url}/ViewLocation?latitude={lat}&longitude={lon}",
                height=350,
                width=350,
                title="View location"
            )
        return TaskModuleResponseFactory.to_task_module_response(task_info)

    async def on_teams_task_module_submit(self, turn_context: TurnContext, task_module_request):
        data = task_module_request.data
        activity = turn_context.activity

        new_checkin_details = {
            "userId": activity.from_property.aad_object_id,
            "userName": activity.from_property.name,
            "latitude": data.get("latitude"),
            "longitude": data.get("longitude"),
            "time": str(activity.local_timestamp),
        }

        await self.save_user_details(new_checkin_details)

        card = CardFactory.adaptive_card(
            self.adaptive_card_for_user_location_details(
                new_checkin_details["userName"],
                new_checkin_details["time"],
                new_checkin_details["latitude"],
                new_checkin_details["longitude"]
            )
        )
        await turn_context.send_activity(Activity(attachments=[card]))
        return TaskModuleResponseFactory.create_response("Check-in submitted!")


    async def save_user_details(self, new_details):
        user_id = new_details["userId"]
        current_data = conversation_data_references.get("userDetails", [])
        current_data = [d for d in current_data if d["userId"] != user_id]
        current_data.append(new_details)
        conversation_data_references["userDetails"] = current_data

    def adaptive_card_for_task_module(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": "Please click on check in"
                },
                {
                    "type": "ActionSet",
                    "actions": [
                        {
                            "type": "Action.Submit",
                            "title": "Check in",
                            "data": {
                                "msteams": {
                                    "type": "task/fetch"
                                },
                                "id": "checkin"
                            }
                        }
                    ]
                }
            ]
        }

    def adaptive_card_for_user_last_checkin(self, user):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": f"User name: {user['userName']}"
                },
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "wrap": True,
                    "text": f"Check in time: {user['time']}"
                },
                {
                    "type": "ActionSet",
                    "actions": [
                        {
                            "type": "Action.Submit",
                            "title": "View Location",
                            "data": {
                                "msteams": {
                                    "type": "task/fetch"
                                },
                                "id": "viewLocation",
                                "latitude": user['latitude'],
                                "longitude": user['longitude']
                            }
                        }
                    ]
                }
            ]
        }

    def adaptive_card_for_user_location_details(self, user_name, time, lat, lon):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": f"Username is: {user_name}"
                },
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "wrap": True,
                    "text": f"Check in time: {time}"
                },
                {
                    "type": "ActionSet",
                    "actions": [
                        {
                            "type": "Action.Submit",
                            "title": "View Location",
                            "data": {
                                "msteams": {
                                    "type": "task/fetch"
                                },
                                "id": "viewLocation",
                                "latitude": lat,
                                "longitude": lon
                            }
                        }
                    ]
                }
            ]
        }
