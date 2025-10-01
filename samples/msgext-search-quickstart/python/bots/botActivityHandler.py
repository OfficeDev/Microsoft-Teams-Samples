# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

import requests
from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import ThumbnailCard, CardAction, CardImage, InvokeResponse
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema.teams import (
    MessagingExtensionResponse,
    MessagingExtensionAttachment,
    MessagingExtensionResult,
)

# A bot activity handler that processes Teams messaging extensions and app link queries
class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()

    # Handle search-based messaging extension queries
    async def on_teams_messaging_extension_query(self, context, query):
        # Get the search query parameter
        search_query = query.parameters[0].value

        # Call the npm registry search API
        response = requests.get(
            "http://registry.npmjs.com/-/v1/search",
            params={"text": search_query, "size": 8},
        )
        response.raise_for_status()
        data = response.json()

        attachments = []

        # Build a list of cards for the search results
        for obj in data["objects"][:8]:
            package = obj.get("package", {})
            package_name = package.get("name", "Unknown Package")
            description = package.get("description", "No description available")
            homepage = package.get("links", {}).get("homepage", "https://www.npmjs.com")

            # Main card (full view when selected/inserted into conversation)
            thumbnail_card = ThumbnailCard(
                title=package_name,
                text=description,
                buttons=[
                    CardAction(
                        type="openUrl",
                        title="View on NPM",
                        value=f"https://www.npmjs.com/package/{package_name}",
                    ),
                    CardAction(
                        type="openUrl",
                        title="Homepage",
                        value=homepage,
                    ),
                ],
            )
            card_attachment = CardFactory.thumbnail_card(thumbnail_card)

            # Preview card (shown in search result list)
            preview_card = ThumbnailCard(title=package_name, text=description)
            preview_attachment = CardFactory.thumbnail_card(preview_card)
            preview_attachment.content.tap = CardAction(
                type="invoke",
                value={"title": package_name, "description": description},
            )

            # Messaging Extension attachment
            attachment = MessagingExtensionAttachment(
                content=card_attachment.content,
                content_type=card_attachment.content_type,
                preview=preview_attachment,
            )
            attachments.append(attachment)

        # Wrap the results in a MessagingExtensionResult
        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=attachments,
        )

        # Return InvokeResponse with the MessagingExtensionResponse
        return InvokeResponse(
            status=200,
            body=MessagingExtensionResponse(compose_extension=result),
        )

    # Handle the user's selection of an item from the search results
    async def on_teams_messaging_extension_select_item(
        self, turn_context: TurnContext, query
    ) -> InvokeResponse:
        # Build a card with the selected package details
        selected_card = ThumbnailCard(
            title=query.get("title"),
            text=query.get("description"),
        )
        selected_attachment = CardFactory.thumbnail_card(selected_card)

        me_attachment = MessagingExtensionAttachment(
            content=selected_attachment.content,
            content_type=selected_attachment.content_type,
        )

        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=[me_attachment],
        )

        return InvokeResponse(
            status=200,
            body=MessagingExtensionResponse(compose_extension=result),
        )

    # Handle app-based link unfurling (e.g., when a user pastes a link)
    async def on_teams_app_based_link_query(
        self, turn_context: TurnContext, query
    ) -> InvokeResponse:
        # Create a card with the unfurled link details
        link_card = ThumbnailCard(
            title="Thumbnail Card",
            text=query.url,
            images=[
                CardImage(
                    url="https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png"
                )
            ],
        )
        link_attachment = CardFactory.thumbnail_card(link_card)

        me_attachment = MessagingExtensionAttachment(
            content=link_attachment.content,
            content_type=link_attachment.content_type,
        )

        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=[me_attachment],
        )

        return InvokeResponse(
            status=200,
            body=MessagingExtensionResponse(compose_extension=result),
        )
