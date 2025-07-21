# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import Activity, InvokeResponse
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from server.services import incident_service
from server.models import adaptive_card

class BotSequentialFlowAdaptiveCard(TeamsActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_message_activity(self, turn_context: TurnContext):
        print("Running on Message Activity.")
        await self.start_inc_management(turn_context)

    async def on_invoke_activity(self, turn_context: TurnContext):
        print("Activity:", turn_context.activity.name)
        user = turn_context.activity.from_property
        activity_name = turn_context.activity.name
        activity_value = turn_context.activity.value
        action = activity_value.get("action") if activity_value else None

        if activity_name == "composeExtension/submitAction":
            choiceset = []
            incidents = await incident_service.get_all_inc()

            if activity_value.get("data", {}).get("msteams") is not None:
                for inc in incidents:
                    choice_data = {
                        "title": f"Incident title: {inc.title}, Created by: {inc.created_by.get('name')}",
                        "value": inc.id,
                    }
                    choiceset.append(choice_data)

                incident_card = CardFactory.adaptive_card(
                    await adaptive_card.incident_list_card(choiceset)
                )
                return adaptive_card.invoke_incident_task_response(
                    "Select incident", incident_card
                )

            incident_data = activity_value.get("data")
            incident = next(
                (inc for inc in incidents if inc.id == incident_data.get("incidentId")),
                None,
            )

            if incident:
                all_members = await TeamsInfo.get_members(turn_context)

                refresh_card = CardFactory.adaptive_card(
                    await adaptive_card.refresh_bot_card(incident, all_members)
                )
                await turn_context.send_activity(Activity(attachments=[refresh_card]))
                return InvokeResponse(
                    status=200, body={}
                )  # Empty body for submitAction

        elif activity_name == "composeExtension/fetchTask":
            try:
                choiceset = []
                all_members = [
                    member
                    for member in await TeamsInfo.get_members(turn_context)
                    if member.aad_object_id
                ]
                incidents = await incident_service.get_all_inc()

                if not incidents:
                    no_incident_card = CardFactory.adaptive_card(
                        {
                            "version": "1.0.0",
                            "type": "AdaptiveCard",
                            "body": [
                                {
                                    "type": "TextBlock",
                                    "text": "No incident found.",
                                    "size": "large",
                                    "weight": "bolder",
                                },
                                {
                                    "type": "TextBlock",
                                    "text": "Please create an incident using bot.",
                                    "size": "medium",
                                    "weight": "bolder",
                                },
                            ],
                        }
                    )

                    return adaptive_card.invoke_task_response(
                        "No Incident found", no_incident_card
                    )

                for inc in incidents:
                    created_by_name = ""

                    if isinstance(inc.created_by, dict):
                        created_by_name = inc.created_by.get("name", "Unknown")
                    elif hasattr(inc.created_by, "name"):
                        created_by_name = inc.created_by.name
                    elif isinstance(inc.created_by, str):
                        created_by_name = inc.created_by
                    else:
                        created_by_name = "Unknown"

                    choice_data = {
                        "title": f"Incident title: {inc.title}, Created by: {created_by_name}",
                        "value": inc.id,
                    }
                    choiceset.append(choice_data)

                incident_card = CardFactory.adaptive_card(
                    adaptive_card.incident_list_card(choiceset)
                )
                return adaptive_card.invoke_incident_task_response(
                    "Select incident", incident_card
                )

            except Exception as error:
                if getattr(error, "code", None) == "BotNotInConversationRoster":
                    install_card = CardFactory.adaptive_card(
                        {
                            "version": "1.0.0",
                            "type": "AdaptiveCard",
                            "body": [
                                {
                                    "type": "TextBlock",
                                    "text": "Looks like you haven't used bot in team/chat",
                                }
                            ],
                            "actions": [
                                {
                                    "type": "Action.Submit",
                                    "title": "Continue",
                                    "data": {"msteams": {"justInTimeInstall": True}},
                                }
                            ],
                        }
                    )

                    return adaptive_card.invoke_task_response(
                        "Bot is not installed", install_card
                    )

        elif activity_name == "adaptiveCard/action":
            print("Verb:", action.get("verb") if action else "N/A")

            try:
                members = await TeamsInfo.get_members(turn_context)
                print(f"Fetched {len(members)} members")

                all_members = []
                for member in members:
                    # print("Member:", vars(member))

                    # Try to get AAD Object ID from either attribute or additional_properties
                    aad_object_id = getattr(
                        member, "aad_object_id", None
                    ) or member.additional_properties.get("objectId")

                    if aad_object_id:
                        # print(
                        #     f"✅ Adding member: {member.name}, AAD Object ID: {aad_object_id}"
                        # )
                        all_members.append(
                            {"name": member.name, "aadObjectId": aad_object_id}
                        )

                    if not all_members:
                        print("⚠️ No members with valid AAD Object IDs found.")

                response_card = await adaptive_card.select_response_card(
                    turn_context, user, all_members
                )

                # DEBUG: Check if 'incident' field inside card is an object or dict
                # ✅ Convert Incident object to dict before returning the card
                incident_obj = (
                    response_card.get("refresh", {})
                    .get("action", {})
                    .get("data", {})
                    .get("incident")
                )
                if hasattr(incident_obj, "to_dict"):
                    response_card["refresh"]["action"]["data"][
                        "incident"
                    ] = incident_obj.to_dict()
                    print("✅ Converted Incident object to dict before returning card.")

                return adaptive_card.invoke_response(response_card)

            except Exception as e:
                print("❌ Error :", str(e))
                return adaptive_card.invoke_response(
                    {"type": "message", "text": "Error retrieving team members."}
                )

    async def start_inc_management(self, turn_context: TurnContext):
        await turn_context.send_activity(
            Activity(
                attachments=[CardFactory.adaptive_card(adaptive_card.option_inc())]
            )
        )
