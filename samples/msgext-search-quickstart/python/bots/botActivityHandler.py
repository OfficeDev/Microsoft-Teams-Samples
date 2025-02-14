# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import requests
from botbuilder.core import TurnContext, CardFactory
from botbuilder.schema import ThumbnailCard, CardImage, CardAction
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
        # Extract the user's search query from the request
        search_query = query.parameters[0].value

        # Call the NPM search API with the user's search query and limit results to 8
        response = requests.get(
            "http://registry.npmjs.com/-/v1/search",
            params={"text": search_query, "size": 8},  # Limit results to top 8
        )
        response.raise_for_status()  # Raise an error if the API call fails
        data = response.json()  # Parse the JSON response from the API

        # Prepare messaging attachments for the top 5 search results
        attachments = []
        for obj in data["objects"][:8]:  # Iterate through the first 5 search results
            package = obj.get("package", {})
            package_name = package.get("name", "Unknown Package")  # Fallback if name is missing
            description = package.get("description", "No description available")  # Fallback for missing description
            homepage = package.get("links", {}).get("homepage", "https://www.npmjs.com")  # Default link

            # Create a ThumbnailCard for each package with buttons for NPM and homepage links
            thumbnail_card = ThumbnailCard(
                title=package_name,  # Package name as card title
                text=description,  # Package description
                buttons=[
                    # Button to view the package on NPM
                    CardAction(
                        type="openUrl",
                        title="View on NPM",
                        value=f"https://www.npmjs.com/package/{package_name}",
                    ),
                    # Button to visit the package's homepage
                    CardAction(
                        type="openUrl",
                        title="Homepage",
                        value=homepage,
                    ),
                ],
            )

            # Create a preview card with title and description for display in the list
            preview_card = ThumbnailCard(
                title=package_name,
                text=description,
            )
            preview_attachment = CardFactory.thumbnail_card(preview_card)

            # Add a tap action to the preview card for handling clicks
            preview_attachment.content.tap = CardAction(
                type="invoke",  # Invoke action triggers a bot command
                value={"title": package_name, "description": description},
            )

            # Create a MessagingExtensionAttachment with the main card and preview
            attachment = MessagingExtensionAttachment(
                content=thumbnail_card,
                content_type=CardFactory.content_types.thumbnail_card,
                preview=preview_attachment,
            )

            # Add the attachment to the list
            attachments.append(attachment)

        # Return a MessagingExtensionResponse with the list of attachments
        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",  # Indicates this is a list of results
                attachment_layout="list",  # Layout style for results
                attachments=attachments,
            )
        )

    # Handle the user's selection of an item from the search results
    async def on_teams_messaging_extension_select_item(
        self, turn_context: TurnContext, query
    ) -> MessagingExtensionResponse:
        # Create a ThumbnailCard using the title and description provided in the query
        thumbnail_card = ThumbnailCard(
            title=query.get("title"),  # Extract title from the query
            text=query.get("description"),  # Extract description from the query
        )

        # Create a MessagingExtensionAttachment for the selected item
        attachment = MessagingExtensionAttachment(
            content_type=CardFactory.content_types.thumbnail_card,
            content=thumbnail_card,
        )

        # Return the selected item as a MessagingExtensionResponse
        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",  # Indicates this is a single result
                attachment_layout="list",  # Use list layout
                attachments=[attachment],  # Include the single attachment
            )
        )

    # Handle app-based link unfurling (e.g., when a user pastes a link)
    async def on_teams_app_based_link_query(self, turn_context: TurnContext, query):
        """
        Handles unfurling links when a user pastes them in a conversation.
        """
        # Create a ThumbnailCard for the unfurled link
        attachment = ThumbnailCard(
            title="Thumbnail Card",  # Title for the card
            text=query.url,  # Display the URL that was pasted
            images=[
                # Add an image for the card (example image from GitHub)
                CardImage(
                    url="https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png"
                )
            ],
        ).to_attachment()

        # Prepare a MessagingExtensionResult with the unfurled link
        result = MessagingExtensionResult(
            type="result",  # Indicates this is a result
            attachment_layout="list",  # Use list layout
            attachments=[attachment],  # Include the attachment
        )

        # Return the response with the unfurled link
        return MessagingExtensionResponse(compose_extension=result)
