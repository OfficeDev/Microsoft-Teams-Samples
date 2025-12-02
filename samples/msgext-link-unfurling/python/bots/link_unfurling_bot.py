# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.hosting.teams import TeamsActivityHandler
from microsoft_agents.activity.teams import (
    MessagingExtensionResponse,
    MessagingExtensionAttachment,
    MessagingExtensionResult,
)

class LinkUnfurlingBot(TeamsActivityHandler):
    async def on_teams_app_based_link_query(
        self, turn_context: TurnContext, query
    ):
        # query is a dict in Agent SDK
        url = query.get("url", "No URL provided")
        
        thumbnail_card = {
            "contentType": "application/vnd.microsoft.card.thumbnail",
            "content": {
                "title": "Thumbnail Card",
                "text": url,
                "images": [
                    {
                        "url": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png"
                    }
                ],
            },
        }

        attachments = MessagingExtensionAttachment(
            content_type="application/vnd.microsoft.card.thumbnail",
            content=thumbnail_card["content"],
            content_url=""  # Required field in Agent SDK
        )

        result = MessagingExtensionResult(
            attachment_layout="list", type="result", attachments=[attachments]
        )

        return MessagingExtensionResponse(compose_extension=result)

    async def on_teams_messaging_extension_query(
        self, turn_context: TurnContext, query
    ):
        # These commandIds are defined in the Teams App Manifest.
        command_id = query.get("commandId") if isinstance(query, dict) else getattr(query, "command_id", None)
        
        if command_id != "searchQuery":
            raise NotImplementedError(f"Invalid CommandId: {command_id}")

        hero_card = {
            "contentType": "application/vnd.microsoft.card.hero",
            "content": {
                "title": "This is a Link Unfurling Sample",
                "subtitle": "It will unfurl links from *.BotFramework.com",
                "text": "This sample demonstrates how to handle link unfurling in Teams. Please review the readme for more information.",
            },
        }

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                attachment_layout="list",
                type="result",
                attachments=[
                    MessagingExtensionAttachment(
                        content=hero_card["content"],
                        content_type="application/vnd.microsoft.card.hero",
                        content_url="",  # Required field in Agent SDK
                        preview=hero_card,
                    )
                ],
            )
        )
