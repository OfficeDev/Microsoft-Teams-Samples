# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import CardFactory
from botbuilder.schema import (
    Attachment,
    HeroCard,
    CardImage,
    CardAction,
    ActionTypes,
    SigninCard,
    OAuthCard
)
import os


def adaptive_Card() -> Attachment:
    return CardFactory.adaptive_card(
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


def collection_Card() -> Attachment:
    return CardFactory.adaptive_card(
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


def list_Card() -> Attachment:
    return Attachment(
        content_type="application/vnd.microsoft.teams.card.list",
        content={
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
    )


def o365_Connector_Card() -> Attachment:
    return Attachment(
        content_type="application/vnd.microsoft.teams.card.o365connector",
        content={
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
                            "image": "http://connectorsdemo.azurewebsites.net/images/MicrosoftSurface_024_Cafe_OH-06315_VS_R1c.jpg"
                        },
                        {
                            "image": "http://connectorsdemo.azurewebsites.net/images/WIN12_Scene_01.jpg"
                        },
                        {
                            "image": "http://connectorsdemo.azurewebsites.net/images/WIN12_Anthony_02.jpg"
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
    )


def thumbnail_Card() -> Attachment:
    return Attachment(
        content_type="application/vnd.microsoft.card.thumbnail",
        content={
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
    )


def hero_Card():
    return CardFactory.hero_card(
        HeroCard(
            title="BotFramework Hero Card",
            images=[
                CardImage(
                    url="https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
                )
            ],
            buttons=[
                CardAction(
                    type=ActionTypes.open_url,
                    title="Get started",
                    value="https://docs.microsoft.com/en-us/azure/bot-service/",
                )
            ],
        )
    )


def signin_Card():
    return CardFactory.signin_card(
        SigninCard(
            text="BotFramework SignIn Card",
            buttons=[
                CardAction(
                    type=ActionTypes.signin,
                    title="Sign In",
                    value="https://login.microsoftonline.com",
                )
            ],
        )
    )

def oauth_Card(connection_name: str):
    return CardFactory.oauth_card(
        connection_name, "Sign In", "Please sign in to continue."
    )
