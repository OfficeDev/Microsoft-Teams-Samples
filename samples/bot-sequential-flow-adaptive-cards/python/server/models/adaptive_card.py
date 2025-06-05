# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import json
from botbuilder.core import MessageFactory, CardFactory, TurnContext
from botframework.connector.auth import MicrosoftAppCredentials
from botframework.connector import ConnectorClient
from server.services import incident_service
import os
import re
from botbuilder.schema import Activity, ActivityTypes
from botbuilder.schema.teams import TaskModuleContinueResponse, TaskModuleTaskInfo
from botbuilder.schema import InvokeResponse

categories = [
    {"name": "Software", "subcategory": ["Email", "OS", "Application"]},
    {
        "name": "Hardware",
        "subcategory": ["CPU", "Disk", "Keyboard", "Memory", "Monitor", "Mouse"],
    },
]

all_members = []
other_members = []


def incident_management_card(profile_name):
    return {
        "version": "1.0.0",
        "type": "AdaptiveCard",
        "body": [
            {"type": "TextBlock", "text": f"Hello {profile_name}"},
            {"type": "TextBlock", "text": "Starting Incident Management Workflow"},
        ],
    }

def invoke_response(card):
    return InvokeResponse(
        status=200,
        body={"type": "application/vnd.microsoft.card.adaptive", "value": card},
    )

def invoke_task_response(title, card_attachment):
    task_info = TaskModuleTaskInfo(
        title=title,
        height=250,
        width=400,
        card=card_attachment,
    )

    task_response = TaskModuleContinueResponse(value=task_info)

    # ✅ Return as proper InvokeResponse, NOT as a dict
    return InvokeResponse(
        status=200,
        body={
            "task": task_response.serialize()
        },  # 🚨 Make sure to call `.serialize()` here
    )

def invoke_incident_task_response(title, card_attachment):
    task_info = TaskModuleTaskInfo(
        title=title,
        height=460,
        width=600,
        card=card_attachment,
    )

    continue_response = TaskModuleContinueResponse(value=task_info)

    return InvokeResponse(
        status=200,
        body={"task": continue_response.serialize()},
    )

async def select_response_card(context, user, members):
    global all_members, other_members
    all_members = members

    user_aad_id = getattr(user, "aad_object_id", None)

    other_members = [tm for tm in all_members if tm.get("aadObjectId") != user_aad_id]

    action = context.activity.value.get("action", {})
    verb = action.get("verb", "")
    prefix = verb.split("_")[0] if verb else ""

    if verb and prefix == "choose":
        return await choose_category(categories)
    elif verb and prefix == "category":
        return await choose_sub_category(verb, categories)
    elif verb and prefix == "subcategory":
        return create_inc(verb, user, all_members)
    elif verb and prefix == "save":
        card = await save_inc(action, all_members)
        await update_card_in_teams(context, card)
        return card
    elif verb and prefix == "edit":
        return edit_inc(action, all_members)
    elif verb and prefix == "update":
        card = await update_inc(action, all_members)
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
    elif verb and prefix == "list":
        return await view_all_inc()
    else:
        return option_inc()

def option_inc():
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                        "verb": "choose_category",
                        "title": "Create New Incident",
                        "data": {"option": "Create New Incident"},
                    },
                    {
                        "type": "Action.Execute",
                        "verb": "list_inc",
                        "title": "List Incidents",
                        "data": {"option": "List Incidents"},
                    },
                ],
            },
        ],
        "type": "AdaptiveCard",
        "version": "1.4",
    }

def incident_list_card(choiceset):
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.0.0",
        "type": "AdaptiveCard",
        "body": [
            {
                "type": "TextBlock",
                "text": "Select a incident to send in chat",
                "size": "large",
                "weight": "bolder",
            },
            {
                "type": "Input.ChoiceSet",
                "id": "incidentId",
                "style": "expanded",
                "isMultiSelect": False,
                "value": "",
                "choices": choiceset,
                "wrap": True,
            },
        ],
        "actions": [
            {
                "type": "Action.Submit",
                "id": "submit",
                "title": "Send",
                "data": {"action": "incidentSelector"},
            }
        ],
    }

async def choose_category(categories):
    actions = [
        {
            "type": "Action.Execute",
            "verb": f"category_{c['name']}",
            "title": c["name"],
            "data": {"option": c["name"]},
        }
        for c in categories
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
                "text": "Incident Management",
            },
            {
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": "Choose a Category"}],
            },
            {"type": "ActionSet", "actions": actions},
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

async def choose_sub_category(verb, categories):
    category = verb.split("_")[1]
    sub_categories = next(
        (c["subcategory"] for c in categories if c["name"] == category), []
    )
    actions = [
        {
            "type": "Action.Execute",
            "verb": f"subcategory_{category}_{sc}",
            "title": sc,
            "data": {"option": sc},
        }
        for sc in sub_categories
    ]
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.getenv("MicrosoftAppId"),
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
            },
            {
                "type": "RichTextBlock",
                "inlines": [{"type": "TextRun", "text": "Choose a Sub-Category"}],
            },
            {"type": "ActionSet", "actions": actions},
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "choose_category",
                "title": "Back",
                "data": {"info": "Back"},
            }
        ],
        "type": "AdaptiveCard",
        "version": "1.4",
    }

def create_inc(verb, user, other_members):
    # Extract category and subcategory from verb
    parts = verb.split("_")
    category = parts[1] if len(parts) > 1 else ""
    sub_category = parts[2] if len(parts) > 2 else ""

    # Prepare assignees list safely
    assignees = []
    for m in other_members:
        name = m.get("name", "")
        aad_id = m.get("aadObjectId", "")
        if name and aad_id:
            assignees.append({"title": name, "value": aad_id})

    # Debugging tip
    if not assignees:
        print(
            "⚠️ Warning: No assignees found in 'other_members'. List is empty or missing required fields."
        )

    # Safely extract user details
    created_by_name = getattr(user, "name", "Unknown")
    created_by_id = getattr(user, "aad_object_id", "")

    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.environ.get("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [{"type": "TextRun", "text": category}],
                            },
                        ],
                    },
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Sub-Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [{"type": "TextRun", "text": sub_category}],
                            },
                        ],
                    },
                ],
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
                "inlines": [{"type": "TextRun", "text": created_by_name, "wrap": True}],
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
            },
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "save_new_inc",
                "title": "Save",
                "data": {
                    "info": "save",
                    "category": category,
                    "sub_category": sub_category,
                    "inc_created_by": {
                        "name": created_by_name,
                        "aadObjectId": created_by_id,
                    },
                },
            },
            {
                "type": "Action.Execute",
                "verb": f"category_{category}",
                "title": "Back",
                "data": {"info": "Back"},
            },
        ],
    }

async def save_inc(action, all_members):
    inc = await incident_service.save_inc(action, all_members)

    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.environ.get("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {"info": "refresh", "incident": inc},  # Send as dict
            },
            "userIds": [m["aadObjectId"] for m in all_members],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "type": "RichTextBlock",
                "id": "inc_title",
                "inlines": [{"type": "TextRun", "text": inc.title}],
            },
            {
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [{"type": "TextRun", "text": inc.category}],
                            },
                        ],
                    },
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Sub-Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                        "horizontalAlignment": "Center",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": inc.sub_category,
                                        "horizontalAlignment": "Center",
                                    }
                                ],
                            },
                        ],
                    },
                ],
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
                    {"type": "TextRun", "text": inc.assigned_to["name"], "wrap": True}
                ],
            },
        ],
    }

def edit_inc(action, other_members):
    inc = action["data"]["incident"]
    assigned_to = inc.get("assigned_to") or {}
    created_by = inc.get("created_by") or {}

    # Prepare choices for assignee dropdown
    assignees = [{"title": m["name"], "value": m["aadObjectId"]} for m in other_members]

    # Find current assignee value
    assignee = next(
        (m for m in assignees if m["value"] == assigned_to.get("aadObjectId")), None
    )

    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.environ.get("MicrosoftAppId"),
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
            },
            # Title Input
            {
                "type": "TextBlock",
                "text": "Title",
                "weight": "Bolder",
                "italic": True,
                "size": "medium",
            },
            {
                "type": "Input.Text",
                "id": "title",
                "placeholder": "Enter Title",
                "value": inc.get("title", ""),
            },
            # Category (non-editable display)
            {
                "type": "TextBlock",
                "text": "Category",
                "weight": "Bolder",
                "italic": True,
                "size": "medium",
                "separator": True,
            },
            {"type": "TextBlock", "text": inc.get("category", ""), "wrap": True},
            {
                "type": "TextBlock",
                "text": "Created By",
                "weight": "Bolder",
                "italic": True,
                "size": "medium",
            },
            {
                "type": "TextBlock",
                "id": "createdByNameDisplay",
                "text": created_by.get("name", ""),
                "wrap": True,
            },
            {
                "type": "Input.Text",
                "id": "createdByName",
                "value": created_by.get("name", ""),
                "isVisible": False,
            },
            # Assigned To ChoiceSet
            {
                "type": "TextBlock",
                "text": "Assigned To",
                "weight": "Bolder",
                "italic": True,
                "size": "medium",
            },
            {
                "type": "Input.ChoiceSet",
                "id": "assignedToId",
                "style": "compact",
                "choices": assignees,
                "value": assignee["value"] if assignee else "",
                "placeholder": "Select Assignee",
            },
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "verb": "update_inc",
                "title": "Update",
                "data": {"info": "update", "incident": inc},
            },
            {
                "type": "Action.Execute",
                "verb": "view_inc",
                "title": "Back",
                "data": {"info": "back", "incident": inc},
            },
        ],
    }

def sanitize_for_json(data):
    # Recursively convert complex objects to JSON-serializable formats
    if isinstance(data, dict):
        return {k: sanitize_for_json(v) for k, v in data.items()}
    elif isinstance(data, list):
        return [sanitize_for_json(item) for item in data]
    elif hasattr(data, "__dict__"):
        return sanitize_for_json(vars(data))
    elif isinstance(data, (str, int, float, bool)) or data is None:
        return data
    else:
        return str(data)  # fallback to string

def flatten_incident_for_data(inc):
    return {
        "incidentId": inc.get("id", ""),
        "title": inc.get("title", ""),
        "category": inc.get("category", ""),
        "subCategory": inc.get("subCategory", ""),
        "createdBy": inc.get("createdBy", {}).get("name", ""),
        "assignedTo": inc.get("assignedTo", {}).get("name", ""),
    }

async def update_inc(action, all_members):
    data = action.get("data", {})

    # Extract inputs from submitted data
    title = data.get("inc_title", "")
    assigned_to_aad = data.get("inc_assigned_to", "")
    created_by_name = data.get("created_by_name", "")
    sub_category = data.get("sub_category", "")

    # Original incident payload or empty
    incident_payload = data.get("incident", {})

    # Update incident payload with new data
    incident_payload["title"] = title
    incident_payload["subCategory"] = sub_category

    # Preserve createdBy
    if not incident_payload.get("createdBy") or not incident_payload["createdBy"].get(
        "name"
    ):
        incident_payload["createdBy"] = {"name": created_by_name}

    # Find assigned member details by AAD ID
    assigned_to_member = next(
        (m for m in all_members if m.get("aadObjectId") == assigned_to_aad), None
    )
    if assigned_to_member:
        incident_payload["assignedTo"] = {
            "aadObjectId": assigned_to_member["aadObjectId"],
            "name": assigned_to_member["name"],
        }
    else:
        incident_payload["assignedTo"] = {}

    # Call your update service to save the incident
    action["data"]["incident"] = incident_payload
    inc = await incident_service.update_inc(action, all_members)

    # Sanitize and flatten for response card
    inc_dict = sanitize_for_json(inc)
    flat_data = flatten_incident_for_data(inc_dict)

    user_ids = [
        m["id"] if isinstance(m, dict) and "id" in m else getattr(m, "id", None)
        for m in all_members
        if m is not None and (("id" in m and m["id"]) or getattr(m, "id", None))
    ]

    # Return updated incident view card
    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {
                    "info": "refresh",
                    **flat_data,
                },
            },
            "userIds": user_ids,
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "inlines": [
                    {"type": "TextRun", "text": inc_dict.get("title", "Untitled")}
                ],
            },
            {
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
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
                                        "text": inc_dict.get("category", "N/A"),
                                    }
                                ],
                            },
                        ],
                    },
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Sub-Category",
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
                                        "text": inc_dict.get("sub_category", "N/A"),
                                    }
                                ],
                            },
                        ],
                    },
                ],
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
                        "text": inc_dict.get("created_by", {}).get("name", "Unknown"),
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
                        "text": inc_dict.get("assigned_to", {}).get(
                            "name", "Unassigned"
                        ),
                        "wrap": True,
                    }
                ],
            },
        ],
    }

    return card

async def view_inc(action, all_members):
    incident = action["data"]["incident"]
    return await update_inc({"data": {"incident": incident}}, all_members)

async def update_status_inc(action):
    inc = await incident_service.update_status_inc(action)

    # Convert to dict (if not already)
    if not isinstance(inc, dict):
        inc = inc.to_dict()

    baseUrl = os.getenv("BaseUrl")
    # fallback icon if status is unknown
    status_icon = {
        "Approved": baseUrl + "/Images/Status_Icons/approved.png",
        "Rejected": baseUrl + "/Images/Status_Icons/rejected.png",
    }.get(inc.get("status"), baseUrl + "/Images/Status_Icons/pending.png")

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {"type": "TextRun", "text": inc["category"]}
                                ],
                            },
                        ],
                    },
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Sub-Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {"type": "TextRun", "text": inc["sub_category"]}
                                ],
                            },
                        ],
                    },
                ],
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
                    {"type": "TextRun", "text": inc["created_by"]["name"], "wrap": True}
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
                        "text": inc["assigned_to"]["name"],
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
                "type": "Image",
                "id": "inc_status",
                "url": status_icon,
                "altText": inc["status"],
                "width": "64px",
                "height": "64px",
            },
        ],
    }

    # Optional: validate JSON before returning
    try:
        json.dumps(card)
    except Exception as e:
        print("❌ Card is invalid JSON:", e)
        raise

    return card

async def update_card_in_teams(context: TurnContext, card: dict):
    service_url = context.activity.service_url
    credentials = MicrosoftAppCredentials(
        app_id=os.environ["MicrosoftAppId"], password=os.environ["MicrosoftAppPassword"]
    )
    connector_client = ConnectorClient(credentials, base_url=service_url)
    conversation_id = context.activity.conversation.id
    activity_id = context.activity.reply_to_id

    card_attachment = CardFactory.adaptive_card(card)

    updated_activity = Activity(
        type=ActivityTypes.message, attachments=[card_attachment]
    )

    connector_client.conversations.update_activity(
        conversation_id=conversation_id,
        activity_id=activity_id,
        activity=updated_activity,
    )

async def refresh_inc(user, action):
    inc = action["data"]["incident"]
    actions = []

    created_by = inc.get("created_by", {})
    assigned_to = inc.get("assigned_to", {})

    user_aad_id = getattr(user, "aad_object_id", None)

    if user_aad_id == created_by.get("aadObjectId"):
        actions = [
            {
                "type": "Action.Execute",
                "verb": "edit_inc",
                "title": "Edit",
                "data": {"info": "edit", "incident": inc},
            }
        ]
    elif user_aad_id == assigned_to.get("aadObjectId"):
        actions = [
            {
                "type": "Action.Execute",
                "verb": "status_approve_inc",
                "title": "Approve",
                "data": {"info": "approve", "status": "Approved", "incident": inc},
            },
            {
                "type": "Action.Execute",
                "verb": "status_reject_inc",
                "title": "Reject",
                "data": {"info": "reject", "status": "Rejected", "incident": inc},
            },
        ]

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.environ["MicrosoftAppId"],
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
                                        "weight": "Bolder",
                                        "italic": True,
                                        "size": "medium",
                                    }
                                ],
                            },
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {"type": "TextRun", "text": inc.get("category", "")}
                                ],
                            },
                        ],
                    },
                ],
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
                        "text": created_by.get("name", ""),
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
                        "text": assigned_to.get("name", ""),
                        "wrap": True,
                    }
                ],
            },
        ],
        "actions": actions,
    }

    return card

def resolve_user_name(user_id, members):
    for m in members:
        if m.id == user_id:
            return m.name or m.aad_object_id or "Unknown"
    return user_id or "Unknown"

async def refresh_bot_card(inc, all_members):
    # Normalize incident dict
    if not isinstance(inc, dict):
        inc = vars(inc)

    created_by_name = inc.get("created_by", {}).get("name", "Unknown")
    assigned_to_name = inc.get("assigned_to", {}).get("name", "Unassigned")

    card = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "appId": os.environ["MicrosoftAppId"],
        "type": "AdaptiveCard",
        "version": "1.4",
        "refresh": {
            "action": {
                "type": "Action.Execute",
                "title": "Refresh",
                "verb": "refresh_edit_status",
                "data": {
                    "info": "refresh",
                    "incident": inc,  # Ensure it's serializable!
                },
            },
            "userIds": [
                member.get("id") if isinstance(member, dict) else member.id
                for member in all_members
            ],
        },
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incident Management",
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
                "inlines": [
                    {"type": "TextRun", "text": inc.get("title", ""), "wrap": True}
                ],
            },
            {
                "type": "ColumnSet",
                "columns": [
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Category",
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
                                        "text": inc.get("category", ""),
                                        "wrap": True,
                                    }
                                ],
                            },
                        ],
                    },
                    {
                        "type": "Column",
                        "width": "auto",
                        "items": [
                            {
                                "type": "RichTextBlock",
                                "inlines": [
                                    {
                                        "type": "TextRun",
                                        "text": "Sub-Category",
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
                                        "text": inc.get("subCategory", ""),
                                        "wrap": True,
                                    }
                                ],
                            },
                        ],
                    },
                ],
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
                "inlines": [{"type": "TextRun", "text": created_by_name, "wrap": True}],
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
                    {"type": "TextRun", "text": assigned_to_name, "wrap": True}
                ],
            },
        ],
    }

    return card

# Helper function
def to_title_case(s: str) -> str:
    return re.sub(r"\b\w", lambda m: m.group().upper(), s)

# Main function
async def view_all_inc():

    def get_name_safe(obj, default):
        if isinstance(obj, dict):
            return obj.get("name", default)
        elif hasattr(obj, "name"):
            return obj.name
        return default

    incs = await incident_service.get_all_inc()
    inc_cards = []

    if not incs:
        inc_cards = [
            {
                "type": "RichTextBlock",
                "inlines": [
                    {
                        "type": "TextRun",
                        "text": "No Incidents Available",
                        "italic": True,
                        "size": "medium",
                        "color": "Attention",
                    }
                ],
            }
        ]
    else:
        for inc in incs:
            title = getattr(inc, "title", "Untitled")
            created_by_name = get_name_safe(getattr(inc, "created_by", None), "Unknown")
            assigned_to_name = get_name_safe(
                getattr(inc, "assigned_to", None), "Unassigned"
            )
            category = getattr(inc, "category", "N/A")
            sub_category = getattr(inc, "sub_category", "N/A")
            status = getattr(inc, "status", "Unknown")

            status_color = (
                "Good"
                if status == "Approved"
                else "Attention" if status == "Rejected" else "Accent"
            )

            inc_cards.append(
                {
                    "type": "Container",
                    "separator": True,
                    "items": [
                        {
                            "type": "RichTextBlock",
                            "inlines": [
                                {
                                    "type": "TextRun",
                                    "text": title,
                                    "weight": "Bolder",
                                    "italic": True,
                                    "size": "medium",
                                }
                            ],
                            "separator": True,
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
                                            "inlines": [
                                                {
                                                    "type": "TextRun",
                                                    "text": created_by_name,
                                                    "size": "Small",
                                                }
                                            ],
                                            "spacing": "Small",
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
                                            "inlines": [
                                                {
                                                    "type": "TextRun",
                                                    "text": assigned_to_name,
                                                    "size": "Small",
                                                }
                                            ],
                                            "spacing": "Small",
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
                                                    "text": "Category",
                                                    "weight": "Bolder",
                                                    "italic": True,
                                                    "size": "medium",
                                                }
                                            ],
                                        },
                                        {
                                            "type": "RichTextBlock",
                                            "inlines": [
                                                {"type": "TextRun", "text": category}
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
                                                    "text": "Sub Category",
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
                                                    "text": sub_category,
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
                                                    "text": status,
                                                    "color": status_color,
                                                }
                                            ],
                                        },
                                    ],
                                },
                            ],
                        },
                    ],
                }
            )

    card = {
        "type": "AdaptiveCard",
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Incidents List",
            },
            {
                "type": "Container",
                "separator": True,
                "items": inc_cards,
            },
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

async def get_all_categories():
    return [{"name": "Hardware"}, {"name": "Software"}, {"name": "Network"}]
