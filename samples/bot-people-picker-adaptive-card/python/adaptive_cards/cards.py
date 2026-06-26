# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import CardFactory
from botbuilder.schema import Attachment


def adaptive_card_for_personal_scope() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": "Task title",
                },
                {"type": "Input.Text", "placeholder": "Task title", "id": "taskTitle"},
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "text": "Task description",
                    "weight": "Bolder",
                },
                {
                    "type": "Input.Text",
                    "weight": "Bolder",
                    "placeholder": "Task description",
                    "id": "taskDescription",
                },
                {
                    "columns": [
                        {
                            "width": "auto",
                            "items": [
                                {
                                    "text": "Select the member to assign the task:",
                                    "wrap": True,
                                    "height": "stretch",
                                    "type": "TextBlock",
                                    "weight": "Bolder",
                                }
                            ],
                            "type": "Column",
                        }
                    ],
                    "type": "ColumnSet",
                },
                {
                    "columns": [
                        {
                            "width": "stretch",
                            "items": [
                                {
                                    "choices": [],
                                    "isMultiSelect": False,
                                    "style": "filtered",
                                    "choices.data": {
                                        "type": "Data.Query",
                                        "dataset": "graph.microsoft.com/users",
                                    },
                                    "id": "userId",
                                    "type": "Input.ChoiceSet",
                                }
                            ],
                            "type": "Column",
                        }
                    ],
                    "type": "ColumnSet",
                },
            ],
            "actions": [
                {"type": "Action.Submit", "id": "submitdynamic", "title": "Assign"}
            ],
        }
    )


def adaptive_card_for_channel_scope() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": "Task title",
                },
                {
                    "type": "Input.Text",
                    "placeholder": "Task title",
                    "id": "taskTitle",
                },
                {
                    "type": "TextBlock",
                    "size": "Medium",
                    "weight": "Bolder",
                    "text": "Task description",
                },
                {
                    "type": "Input.Text",
                    "placeholder": "Task description",
                    "id": "taskDescription",
                },
                {
                    "columns": [
                        {
                            "width": "auto",
                            "items": [
                                {
                                    "text": "Select the member to assign the task: ",
                                    "wrap": True,
                                    "height": "stretch",
                                    "type": "TextBlock",
                                    "weight": "Bolder",
                                }
                            ],
                            "type": "Column",
                        }
                    ],
                    "type": "ColumnSet",
                },
                {
                    "columns": [
                        {
                            "width": "stretch",
                            "items": [
                                {
                                    "choices": [],
                                    "isMultiSelect": True,
                                    "style": "filtered",
                                    "choices.data": {
                                        "type": "Data.Query",
                                        "dataset": "graph.microsoft.com/users?scope=currentContext",
                                    },
                                    "id": "userId",
                                    "type": "Input.ChoiceSet",
                                }
                            ],
                            "type": "Column",
                        }
                    ],
                    "type": "ColumnSet",
                },
            ],
            "actions": [
                {"type": "Action.Submit", "id": "submitdynamic", "title": "Assign"}
            ],
        }
    )
