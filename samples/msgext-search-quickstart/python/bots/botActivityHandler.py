# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

import requests
from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.hosting.teams import TeamsActivityHandler
from microsoft_agents.activity.teams import (
    MessagingExtensionResponse,
    MessagingExtensionResult,
    MessagingExtensionAttachment,
)

class BotActivityHandler(TeamsActivityHandler):
    """Bot activity handler for Teams messaging extensions and app link queries."""

    def __init__(self):
        super().__init__()

    async def on_teams_messaging_extension_query(self, context, query):
        """Handle search-based messaging extension queries."""
        # Extract search query from parameters (supports both dict and object formats)
        search_query = ""
        if isinstance(query, dict):
            parameters = query.get("parameters", [])
            search_query = parameters[0].get("value", "") if parameters else ""
        else:
            search_query = query.parameters[0].value if hasattr(query, "parameters") and query.parameters else ""

        # Use default search term if not provided or too short (npm API requires min 2 chars)
        if not search_query or len(search_query) < 2:
            search_query = "react"

        try:
            # Call the npm registry search API
            response = requests.get(
                "http://registry.npmjs.com/-/v1/search",
                params={"text": search_query, "size": 8},
                timeout=10,
            )
            response.raise_for_status()
            data = response.json()
        except Exception as e:
            # Return error card on API failure
            error_card = {
                "contentType": "application/vnd.microsoft.card.hero",
                "content": {
                    "title": "Search Error",
                    "text": "Unable to search npm packages. Please try again with a different query (minimum 2 characters).",
                },
            }
            return MessagingExtensionResponse(
                compose_extension=MessagingExtensionResult(
                    type="result",
                    attachment_layout="list",
                    attachments=[
                        MessagingExtensionAttachment(
                            content=error_card["content"],
                            content_type="application/vnd.microsoft.card.hero",
                            content_url="",
                            preview=error_card,
                        )
                    ],
                )
            )

        attachments = []

        # Build result cards for search results
        for obj in data["objects"][:8]:
            package = obj.get("package", {})
            package_name = package.get("name", "Unknown Package")
            description = package.get("description", "No description available")
            homepage = package.get("links", {}).get("homepage", "https://www.npmjs.com")

            # Create hero card (better Outlook compatibility)
            hero_card = {
                "contentType": "application/vnd.microsoft.card.hero",
                "content": {
                    "title": package_name,
                    "text": description,
                    "buttons": [
                        {
                            "type": "openUrl",
                            "title": "View on NPM",
                            "value": f"https://www.npmjs.com/package/{package_name}",
                        },
                        {
                            "type": "openUrl",
                            "title": "Homepage",
                            "value": homepage,
                        },
                    ],
                },
            }

            # Create messaging extension attachment
            attachment = MessagingExtensionAttachment(
                content=hero_card["content"],
                content_type="application/vnd.microsoft.card.hero",
                content_url="",  # Required field in Agent SDK
                preview=hero_card,
            )
            attachments.append(attachment)

        # Return messaging extension response
        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",
                attachment_layout="list",
                attachments=attachments,
            )
        )

    async def on_teams_messaging_extension_select_item(self, turn_context: TurnContext, query) -> MessagingExtensionResponse:
        """Handle user's selection of an item from search results."""
        # Create card with selected package details
        selected_card = {
            "contentType": "application/vnd.microsoft.card.thumbnail",
            "content": {
                "title": query.get("title"),
                "text": query.get("description"),
            },
        }

        me_attachment = MessagingExtensionAttachment(
            content=selected_card["content"],
            content_type="application/vnd.microsoft.card.thumbnail",
            content_url="",  # Required field in Agent SDK
        )

        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=[me_attachment],
        )

        return MessagingExtensionResponse(compose_extension=result)

    async def on_teams_app_based_link_query(self, turn_context: TurnContext, query) -> MessagingExtensionResponse:
        """Handle app-based link unfurling when user pastes a link."""
        # Extract URL from query (supports both dict and object formats)
        url = query.get("url", "No URL provided") if isinstance(query, dict) else getattr(query, "url", "No URL provided")

        # Create card with unfurled link details
        link_card = {
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

        me_attachment = MessagingExtensionAttachment(
            content=link_card["content"],
            content_type="application/vnd.microsoft.card.thumbnail",
            content_url="",  # Required field in Agent SDK
        )

        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=[me_attachment],
        )

        return MessagingExtensionResponse(compose_extension=result)
