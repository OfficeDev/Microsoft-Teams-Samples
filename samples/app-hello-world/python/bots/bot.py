# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext, CardFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from faker import Faker
from botbuilder.schema.teams import (
    MessagingExtensionResponse,
    MessagingExtensionAttachment,
    MessagingExtensionResult,
)
from botbuilder.schema import (
    ThumbnailCard,
    CardImage,
    CardAction,
)

class HelloWorldBot(TeamsActivityHandler):
    def __init__(self, app_id: str, app_password: str):
        self._app_id = app_id
        self._app_password = app_password

    async def on_teams_members_added(  # pylint: disable=unused-argument
        self,
        teams_members_added: [TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        for member in teams_members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    f"Welcome to the App hello world sample { member.given_name } { member.surname }. "
                )

    async def on_message_activity(self, turn_context: TurnContext):
         # Remove recipient mention from the activity
        TurnContext.remove_recipient_mention(turn_context.activity)

        # Check if the activity contains a text message
        text = turn_context.activity.text.strip() if turn_context.activity.text else None

        if text:
        # Send a response if text is present
            await turn_context.send_activity(f"You said: {text}")
        else:
        # Return None if no text message is provided
            return None

    async def on_teams_messaging_extension_query(self, context, query):
        faker = Faker()

        title = query.command_id
        random_image_url = "https://loremflickr.com/200/200"

        if query.command_id == "getRandomText":
            attachments = []

        # Generate 5 results with fake text and fake images
        for i in range(5):
            text = faker.paragraph()
            images = [f"{random_image_url}?random={i}"]

            # Create a thumbnail card using ThumbnailCard
            thumbnail_card = ThumbnailCard(
                title=title,
                text=text,
                images=[CardImage(url=image) for image in images],
            )

            # Use CardFactory to create the card attachment
            card_attachment = CardFactory.thumbnail_card(thumbnail_card)

            # Create a CardAction for the 'tap' attribute
            tap_action = CardAction(
                type="invoke",
                value={"title": title, "text": text, "images": images},
            )

            # Create a preview card and add the tap action
            preview_card = ThumbnailCard(
                title=title,
                text=text,
                images=[CardImage(url=image) for image in images],
            )
            preview_attachment = CardFactory.thumbnail_card(preview_card)
            preview_attachment.content.tap = tap_action

            # Combine the thumbnail card and the preview
            attachment = MessagingExtensionAttachment(
                content=thumbnail_card,
                content_type=CardFactory.content_types.thumbnail_card,
                preview=preview_attachment,
            )
            attachments.append(attachment)

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",
                attachment_layout="list",
                attachments=attachments,
            )
        )


    async def on_teams_messaging_extension_select_item(
    self, turn_context: TurnContext, query
    ) -> MessagingExtensionResponse:
        title = query.get("title")
        text = query.get("text")
        images = query.get("images", [])

        thumbnail_card = ThumbnailCard(
                title=title,
                text=text,
                images=[CardImage(url=image) for image in images],
            )
        
        # Create a MessagingExtensionAttachment
        attachment = MessagingExtensionAttachment(
            content_type=CardFactory.content_types.thumbnail_card,
            content=thumbnail_card,
        )

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result", attachment_layout="list", attachments=[attachment]
            )
        )
