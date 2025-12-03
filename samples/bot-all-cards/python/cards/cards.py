# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from microsoft.teams.cards import AdaptiveCard
from typing import Dict, Any


# Creates an Adaptive Card with rich content including images, text blocks, and fact sets
def adaptive_Card() -> AdaptiveCard:
    return AdaptiveCard.model_validate(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Publish Adaptive Card schema",
                            "weight": "bolder",
                            "size": "medium",
                        },
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {
                                    "type": "Column",
                                    "width": "auto",
                                    "items": [
                                        {
                                            "type": "Image",
                                            "url": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                                            "size": "small",
                                            "style": "person",
                                        }
                                    ],
                                },
                                {
                                    "type": "Column",
                                    "width": "stretch",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "Teams",
                                            "weight": "bolder",
                                            "wrap": True,
                                        },
                                        {
                                            "type": "TextBlock",
                                            "spacing": "none",
                                            "text": "Created {{DATE(2023-01-12T06:08:39Z, SHORT)}}",
                                            "isSubtle": True,
                                            "wrap": True,
                                        },
                                    ],
                                },
                            ],
                        },
                    ],
                },
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.",
                            "wrap": True,
                        },
                        {
                            "type": "FactSet",
                            "facts": [
                                {"title": "Board:", "value": "Adaptive Card"},
                                {"title": "List:", "value": "Backlog"},
                                {"title": "Assigned to:", "value": "Teams"},
                            ],
                        },
                    ],
                },
            ],
        }
    )


# Creates a Collection Card (Adaptive Card) for employee events and notifications
def collection_Card() -> AdaptiveCard:
    return AdaptiveCard.model_validate(
        {
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "size": "extraLarge",
                            "weight": "bolder",
                            "text": "Welcome to Employee Connect",
                            "height": "stretch",
                        },
                        {
                            "type": "TextBlock",
                            "size": "medium",
                            "weight": "bolder",
                            "text": "Add events to your calendar",
                            "height": "stretch",
                        },
                        {
                            "type": "TextBlock",
                            "weight": "bolder",
                            "text": "The bot can send \r\rnotification to remind \r\ryou about the latest \r\revents and trainings.",
                            "wrap": True,
                            "height": "stretch",
                        },
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {"type": "Column", "items": [], "height": "stretch"}
                            ],
                        },
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {"type": "Column", "items": [], "height": "stretch"}
                            ],
                        },
                    ],
                }
            ],
            "actions": [{"type": "Action.Submit", "title": "Let's get started"}],
        }
    )


# Creates a List Card with multiple items including files, links, and people
def list_Card() -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.teams.card.list",
        "content": {
            "title": "Card title",
            "items": [
                {
                    "type": "file",
                    "id": "https://contoso.sharepoint.com/teams/new/Shared%20Documents/Report.xlsx",
                    "title": "Report",
                    "subtitle": "teams > new > design",
                    "tap": {
                        "type": "imBack",
                        "value": "editOnline https://contoso.sharepoint.com/teams/new/Shared%20Documents/Report.xlsx",
                    },
                },
                {
                    "type": "resultItem",
                    "icon": "https://cdn2.iconfinder.com/data/icons/social-icons-33/128/Trello-128.png",
                    "title": "Trello title",
                    "subtitle": "A Trello subtitle",
                    "tap": {"type": "openUrl", "value": "http://trello.com"},
                },
                {"type": "section", "title": "Manager"},
                {
                    "type": "person",
                    "id": "JohnDoe@contoso.com",
                    "title": "John Doe",
                    "subtitle": "Manager",
                    "tap": {"type": "imBack", "value": "whois JohnDoe@contoso.com"},
                },
            ],
            "buttons": [{"type": "imBack", "title": "Select", "value": "whois"}],
        },
    }


# Creates an Office 365 Connector Card with sections, facts, images, and actions
def O365_connector_Card() -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.teams.card.o365connector",
        "content": {
            "@type": "MessageCard",
            "@context": "http://schema.org/extensions",
            "summary": "Office 365 Connector Card",
            "title": "Project Tango",
            "sections": [
                {
                    "activityTitle": "Office 365 Connector Card",
                    "activitySubtitle": "On Project Tango",
                    "activityText": "Here are the designs",
                    "activityImage": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                },
                {
                    "title": "Details",
                    "facts": [
                        {"name": "Labels", "value": "Designs, redlines"},
                        {"name": "Due date", "value": "May 28, 2023"},
                        {
                            "name": "Attachments",
                            "value": "[icon.png](https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png)",
                        },
                    ],
                },
                {
                    "title": "Images",
                    "images": [
                        {
                            "image": "https://picsum.photos/400/300?random=1"
                        },
                        {
                            "image": "https://picsum.photos/400/300?random=2"
                        },
                        {
                            "image": "https://picsum.photos/400/300?random=3"
                        },
                    ],
                },
            ],
            "potentialAction": [
                {
                    "@type": "ViewAction",
                    "name": "View in Microsoft",
                    "target": ["https://learn.microsoft.com/"],
                }
            ],
        },
    }


# Creates a Thumbnail Card with a compact layout, small image, and action buttons
def thumbnail_card() -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.card.thumbnail",
        "content": {
            "title": "Thumbnail Card",
            "subtitle": "Tale of a robot who dared to love",
            "text": (
                "Bender Bending Rodríguez is a main character in the animated "
                "television series Futurama. He was created by series creators "
                "Matt Groening and David X. Cohen, and is voiced by John DiMaggio."
            ),
            "images": [
                {
                    "url": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                    "alt": "Bender Rodríguez",
                }
            ],
            "buttons": [
                {
                    "type": "openUrl",
                    "title": "Learn more",
                    "value": "https://en.wikipedia.org/wiki/Bender_(Futurama)",
                }
            ],
        },
    }


# Creates a Hero Card with a large image, title, and action buttons
def hero_Card() -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.card.hero",
        "content": {
            "title": "Teams SDK Hero Card",
            "images": [
                {
                    "url": "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
                }
            ],
            "buttons": [
                {
                    "type": "openUrl",
                    "title": "Get started",
                    "value": "https://docs.microsoft.com/en-us/azure/bot-service/",
                }
            ],
        },
    }


# Creates a SignIn Card for user authentication with a sign-in button
def signin_Card() -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.card.signin",
        "content": {
            "text": "Teams SDK SignIn Card",
            "buttons": [
                {
                    "type": "signin",
                    "title": "Sign In",
                    "value": "https://login.microsoftonline.com",
                }
            ],
        },
    }


# Creates an OAuth Card for authentication with a specified connection name
def oauth_Card(connection_name: str) -> Dict[str, Any]:
    return {
        "contentType": "application/vnd.microsoft.card.oauth",
        "content": {
            "text": "Teams SDK OAuth Card",
            "connectionName": connection_name,
            "buttons": [
                {
                    "type": "signin",
                    "title": "Sign In",
                    "value": "https://login.microsoftonline.com",
                }
            ],
        },
    }
