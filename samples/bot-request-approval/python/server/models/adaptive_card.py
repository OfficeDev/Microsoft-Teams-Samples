# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory, TurnContext, CardFactory
from botbuilder.schema import (
    Activity,
    ActivityTypes,
    Attachment,
    InvokeResponse,
    AdaptiveCardInvokeResponse,
)
from botframework.connector.auth import MicrosoftAppCredentials
from botframework.connector import ConnectorClient
from server.services import task_service
from http import HTTPStatus
import os
import re
import json
from server.models.task import Task  # Import your Task model
from botframework.connector.aio import ConnectorClient


# Store members globally like in JS
all_members = []
other_members = []


def invoke_response(card: dict) -> InvokeResponse:
    # Construct the payload manually as a dict
    adaptive_card_invoke_response = {
        "statusCode": 200,
        "type": "application/vnd.microsoft.card.adaptive",
        "value": card,
    }

    return InvokeResponse(
        status=HTTPStatus.OK,
        body=adaptive_card_invoke_response,  # ✅ return plain dict, not the object
    )


def invoke_task_response(title, card):
    return {
        "status": HTTPStatus.OK,
        "body": {
            "task": {
                "type": "continue",
                "value": {"title": title, "card": card, "height": 250, "width": 400},
            }
        },
    }


async def select_response_card(context, user, members):
    # Filter out the current user
    other_members = [m for m in members if m.aad_object_id != user.aad_object_id]

    action = context.activity.value.get("action", {})
    verb = action.get("verb", "")
    prefix = verb.split("_")[0] if verb else ""

    if verb and prefix == "create":
        return await create_task(verb, user, other_members)

    elif verb and prefix == "save":
        card = await save_task(action, other_members)
        await update_card_in_teams(context, card)
        return card

    elif verb and prefix == "cancel":
        return cancel_inc(action)

    elif verb and prefix == "edit":
        return await edit_inc(action, other_members)

    elif verb and prefix == "update":
        card = await update_inc(action, other_members)
        await update_card_in_teams(context, card)
        return card

    elif verb and prefix == "view":
        card = await view_inc(action)
        await update_card_in_teams(context, card)
        return card

    elif verb and prefix == "status":
        card = await update_status_inc(action)
        await update_card_in_teams(context, card)
        return card

    elif verb and prefix == "refresh":
        return await refresh_inc(user, action)

    elif verb and prefix == "cancelledreq":
        card = await request_cancelled_inc(user, action)
        await update_card_in_teams(context, card)
        return card

    else:
        return option_inc()


def option_inc():
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": "Choose a Option"}],
            },
            {
                "type": "ActionSet",
                "actions": [
                    {
                        "type": "Action.Execute",
                        "verb": "create_task",
                        "title": "Create New Task",
                        "data": {"option": "Create New Task"},
                    }
                ],
            },
        ],
    }


async def create_task(verb, user, other_members):
    assignees = []

    for m in other_members:
        # Attempt to extract name and AAD Object ID safely
        name = getattr(m, "name", None)
        id = getattr(m, "id", None) or getattr(m, "id", None)

        # Defensive fallback: Check additional properties if needed
        if not name and hasattr(m, "_additional_properties"):
            name = m._additional_properties.get("name")
        if not id and hasattr(m, "_additional_properties"):
            id = m._additional_properties.get("id") or m._additional_properties.get(
                "id"
            )

        if name and id:
            assignees.append({"title": name, "value": id})

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {"type": "Input.Text", "placeholder": "Enter Title", "id": "inc_title"},
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "Input.Text",
                "placeholder": "Enter Description",
                "id": "inc_description",
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [{"type": "TextRun", "text": user.name, "wrap": True}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "Input.ChoiceSet",
                "choices": assignees,
                "placeholder": "Select Assignee",
                "id": "inc_assigned_to",
                "style": "compact",
            },
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "save_task",
                "title": "Save",
                "data": {
                    "info": "save",
                    "inc_created_by": {
                        "name": user.name,
                        "aadObjectId": getattr(user, "aad_object_id", "unknown"),
                    },
                },
            },
            {
                "type": "Action.Execute",
                "verb": "other",
                "title": "Back",
                "data": {"info": "Back"},
            },
        ],
    }

    return card


async def save_task(action, other_members):
    all_members = other_members
    inc = await task_service.save_task(action, all_members)

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {"info": "refresh", "task": inc.to_dict()},
            },
            "userIds": [member.id for member in all_members],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.title}],
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.description}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {"type": "TextRun", "text": inc.created_by["name"], "wrap": True}
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_assigned_to",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": (inc.assigned_to["name"]),
                        "wrap": True,
                    }
                ],
            },
        ],
    }

    return card


async def cancel_inc(action):
    inc = action["data"]["task"]

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "cancelledreq_edit_status",
                "data": {"info": "refresh", "status": "Cancelled", "task": inc},
            },
            "userIds": [member.id for member in all_members],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc["title"]}],
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc["description"]}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {"type": "TextRun", "text": inc["createdBy"]["name"], "wrap": True}
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_assigned_to",
                "inlines": [
                    {"type": "TextRun", "text": inc["assignedTo"]["name"], "wrap": True}
                ],
            },
        ],
    }

    return card


async def edit_inc(action, other_members):
    inc = action["data"]["task"]

    assignees = [{"title": member.name, "value": member.id} for member in other_members]

    assigned_to_id = inc.get("assignedTo", {}).get("aadObjectId")

    assignee = next((a for a in assignees if a["value"] == assigned_to_id), None)

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "Input.Text",
                "placeholder": "Enter Title",
                "id": "inc_title",
                "value": inc["title"],
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "Input.Text",
                "placeholder": "Enter Description",
                "id": "inc_description",
                "value": inc["description"],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {"type": "TextRun", "text": inc["createdBy"]["name"], "wrap": True}
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "Input.ChoiceSet",
                "choices": assignees,
                "placeholder": "Select Assignee",
                "id": "inc_assigned_to",
                "value": assignee["value"] if assignee else "",
            },
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "update_inc",
                "title": "Update",
                "data": {
                    "info": "update",
                    "inc_created_by": inc["createdBy"],
                    "task": inc,
                },
            },
            {
                "type": "Action.Execute",
                "verb": "view_inc",
                "title": "Back",
                "data": {"info": "Back", "task": inc},
            },
        ],
    }

    return card


async def update_inc(action, all_members):
    inc = await task_service.update_inc(action, all_members)

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {"info": "refresh", "task": inc.to_dict()},
            },
            "userIds": [member.id for member in all_members],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.title}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.description}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {"type": "TextRun", "text": inc.created_by["name"], "wrap": True}
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_assigned_to",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": getattr(
                            inc.assigned_to,
                            "name",
                            (
                                inc.assigned_to.get("name")
                                if isinstance(inc.assigned_to, dict)
                                else "Unassigned"
                            ),
                        ),
                        "wrap": True,
                    }
                ],
            },
        ],
    }

    return card


async def view_inc(action):
    inc = action["data"]["task"]

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {"info": "refresh", "task": inc},
            },
            "userIds": [member.id for member in all_members],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc["title"]}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_created_by",
                "type": "RichTextBlock",
                "inlines": [
                    {"type": "TextRun", "text": inc["createdBy"]["name"], "wrap": True}
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_assigned_to",
                "type": "RichTextBlock",
                "inlines": [
                    {"type": "TextRun", "text": inc["assignedTo"]["name"], "wrap": True}
                ],
            },
        ],
    }

    return card


async def update_status_inc(action):
    inc = await task_service.update_status_inc(action)

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.title}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.description}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_created_by",
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": (
                            inc.created_by ["name"]
                            if isinstance(inc.created_by , dict)
                            and "name" in inc.created_by 
                            else (
                                str(inc.created_by )
                                if inc.created_by 
                                else "Unassigned"
                            )
                        ),
                        "wrap": True,
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_assigned_to",
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": (
                            inc.assigned_to["name"]
                            if isinstance(inc.assigned_to, dict)
                            and "name" in inc.assigned_to
                            else (
                                str(inc.assigned_to)
                                if inc.assigned_to
                                else "Unassigned"
                            )
                        ),
                        "wrap": True,
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Status",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": inc.status,
                        "color": (
                            "Good"
                            if inc.status == "Approved"
                            else "Attention" if inc.status == "Rejected" else "Accent"
                        ),
                    }
                ],
            },
        ],
    }

    return card


async def update_card_in_teams(context: TurnContext, card: dict):
    service_url = context.activity.service_url
    credentials = MicrosoftAppCredentials(
        app_id=os.getenv("MicrosoftAppId"), password=os.getenv("MicrosoftAppPassword")
    )

    connector_client = ConnectorClient(
        credentials, base_url=service_url
    )  # ✅ async client

    conversation_id = context.activity.conversation.id
    activity_id = context.activity.reply_to_id

    card_attachment = CardFactory.adaptive_card(card)
    reply_activity = MessageFactory.attachment(card_attachment)
    reply_activity.id = activity_id

    # ✅ Now safe to await
    await connector_client.conversations.update_activity(
        conversation_id, activity_id, reply_activity
    )


async def refresh_inc(user, action):
    inc_raw = action["data"].get("task")
    inc = {}

    if isinstance(inc_raw, Task) or (
        hasattr(inc_raw, "to_dict") and callable(inc_raw.to_dict)
    ):
        try:
            inc = inc_raw.to_dict()
        except Exception as e:
            print(f"[ERROR] Task serialization failed: {e}")
            return {"type": "message", "text": "Unable to render task data."}
    elif isinstance(inc_raw, dict):
        inc = inc_raw
    elif isinstance(inc_raw, str):
        try:
            inc = json.loads(inc_raw)
        except Exception as e:
            print(f"[ERROR] Failed to parse task string: {e}")
            return {"type": "message", "text": "Invalid task data."}
    else:
        return {"type": "message", "text": "Invalid task format."}

    actions = []
    created_by = inc.get("createdBy", {})
    assigned_to = inc.get("assignedTo", {})

    creator_id = created_by.get("aadObjectId") or created_by.get("id")
    assignee_id = assigned_to.get("aadObjectId") or assigned_to.get("id")
    user_aad_id = getattr(user, "aad_object_id", "").strip()
    user_id = getattr(user, "id", "").strip()

    if user_aad_id == creator_id:
        actions = [
            {
                "type": "Action.Execute",
                "verb": "edit_inc",
                "title": "Edit",
                "data": {"info": "edit", "task": inc},
            },
            {
                "type": "Action.Execute",
                "verb": "cancelledreq",
                "title": "Cancel",
                "data": {"info": "cancel", "status": "Cancelled", "task": inc},
            },
        ]
    elif user_id == assignee_id or user_aad_id == assignee_id:
        actions = [
            {
                "type": "Action.Execute",
                "verb": "status_approve_inc",
                "title": "Approve",
                "data": {"info": "approve", "status": "Approved", "task": inc},
            },
            {
                "type": "Action.Execute",
                "verb": "status_reject_inc",
                "title": "Reject",
                "data": {"info": "reject", "status": "Rejected", "task": inc},
            },
        ]

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.get("title", "")}],
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.get("description", "")}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": created_by.get("name", "Unknown"),
                        "wrap": True,
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_assigned_to",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": assigned_to.get("name", "Unassigned"),
                        "wrap": True,
                    }
                ],
            },
        ],
        "actions": actions,
    }

    return card


async def request_cancelled_inc(user, action):
    inc = await task_service.update_status_inc(action)

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Task Management",
            },
            {
                "type": "RichTextBlock",
                "separator": True,
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Title",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_title",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.title}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Description",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "id": "inc_description",
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": inc.description}],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Created By",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_created_by",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": (
                            inc.created_by["name"]
                            if isinstance(inc.created_by, dict)
                            else getattr(inc.created_by, "name", "Unknown")
                        ),
                        "wrap": True,
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Assigned To",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "id": "inc_assigned_to",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": (
                            inc.assigned_to["name"]
                            if isinstance(inc.assigned_to, dict)
                            else getattr(inc.assigned_to, "name", "Unknown")
                        ),
                        "wrap": True,
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "Status",
                        "weight": "Bolder",
                        "italic": True,
                        "size": "medium",
                    }
                ],
            },
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": inc.status,
                        "color": (
                            "Good"
                            if inc.status == "Approved"
                            else "Attention" if inc.status == "Rejected" else "Accent"
                        ),
                    }
                ],
            },
        ],
    }

    return card


async def view_all_inc():
    tasks = await task_service.get_all_inc()
    task_cards = []

    if not tasks:
        task_cards = [
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "No Tasks Available",
                        "italic": True,
                        "size": "medium",
                        "color": "Attention",
                    }
                ],
            }
        ]
    else:
        for inc in tasks:
            task_card = {
                "type": "Container",
                "separator": True,
                "items": [
                    {
                        "type": "RichTextBlock",
                        "separator": True,
                        "inlines": [
                            {
                                "type": "TextRun",
                                "text": inc["title"],
                                "weight": "Bolder",
                                "italic": True,
                                "size": "medium",
                            }
                        ],
                    },
                    {
                        "type": "RichTextBlock",
                        "inlines": [
                            {
                                "type": "TextRun",
                                "text": inc["description"],
                                "weight": "Bolder",
                                "italic": True,
                                "size": "medium",
                            }
                        ],
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "width": "stretch",
                                "items": [
                                    {
                                        "type": "RichTextBlock",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": "Created By",
                                                "weight": "Bolder",
                                                "italic": True,
                                                "size": "medium",
                                            }
                                        ],
                                    },
                                    {
                                        "type": "RichTextBlock",
                                        "spacing": "Small",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": inc["createdBy"]["name"],
                                                "size": "Small",
                                            }
                                        ],
                                    },
                                ],
                            },
                            {
                                "type": "Column",
                                "width": "stretch",
                                "items": [
                                    {
                                        "type": "RichTextBlock",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": "Assigned To",
                                                "weight": "Bolder",
                                                "italic": True,
                                                "size": "medium",
                                            }
                                        ],
                                    },
                                    {
                                        "type": "RichTextBlock",
                                        "spacing": "Small",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": inc["assignedTo"]["name"],
                                                "size": "Small",
                                            }
                                        ],
                                    },
                                ],
                            },
                        ],
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "width": "stretch",
                                "items": [
                                    {
                                        "type": "RichTextBlock",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": "Status",
                                                "weight": "Bolder",
                                                "italic": True,
                                                "size": "medium",
                                            }
                                        ],
                                    },
                                    {
                                        "type": "RichTextBlock",
                                        "inlines": [
                                            {
                                                "type": "TextRun",
                                                "text": inc["status"],
                                                "color": (
                                                    "Good"
                                                    if inc["status"] == "Approved"
                                                    else (
                                                        "Attention"
                                                        if inc["status"] == "Rejected"
                                                        else "Accent"
                                                    )
                                                ),
                                            }
                                        ],
                                    },
                                ],
                            }
                        ],
                    },
                ],
            }
            task_cards.append(task_card)

    card = {
        "type": "AdaptiveCard",
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Tasks List",
            },
            {"type": "Container", "separator": True, "items": task_cards},
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "option_inc",
                "title": "Back",
                "data": {"info": "Back"},
            }
        ],
    }

    return card


def to_title_case(text):
    return re.sub(r"\b\w", lambda match: match.group(0).upper(), text)
