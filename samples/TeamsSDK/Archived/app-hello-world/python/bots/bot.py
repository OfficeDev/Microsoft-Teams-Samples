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
    """
    A bot class that handles activities for Microsoft Teams, including messaging,
    member additions, and messaging extension queries.
    """
    def __init__(self, app_id: str, app_password: str):
        """
        Initializes the HelloWorldBot with the provided application ID and password.
        
        Args:
            app_id (str): The bot's application ID.
            app_password (str): The bot's password.
        """
        self._app_id = app_id
        self._app_password = app_password

    async def on_teams_members_added(  # pylint: disable=unused-argument
        self,
        teams_members_added: [TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        """
        Handles new members added to a team. Sends a welcome message to each member.

        Args:
            teams_members_added (list): A list of TeamsChannelAccount objects representing the new members.
            team_info (TeamInfo): Information about the team.
            turn_context (TurnContext): The context of the current turn of the conversation.
        """
        for member in teams_members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    f"Welcome to the App hello world sample {member.given_name} {member.surname}."
                )

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming messages. Responds by echoing the user's message.
        
        Args:
            turn_context (TurnContext): The context of the current turn of the conversation.
        
        Returns:
            None: No explicit return value, the bot sends an activity as a response.
        """
        # Remove recipient mention from the activity to avoid echoing the bot's name
        TurnContext.remove_recipient_mention(turn_context.activity)

        # Get the text from the incoming message
        text = (
            turn_context.activity.text.strip() if turn_context.activity.text else None
        )

        if text:
            await turn_context.send_activity(f"You said: {text}")
        else:
            return None

    async def on_teams_messaging_extension_query(self, context, query):
        """
        Handles messaging extension queries. Returns 5 random results with fake text and images.
        
        Args:
            context (TurnContext): The context of the current turn of the conversation.
            query (object): The query data from the messaging extension.
        
        Returns:
            MessagingExtensionResponse: A response containing generated fake results.
        """
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
                thumbnail_card = self.create_thumbnail_card(title, text, images)

                # Create a preview card and add the tap action
                preview_card = self.create_thumbnail_card(title, text, images)
                tap_action = CardAction(
                    type="invoke",
                    value={"title": title, "text": text, "images": images},
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
        """
        Handles the selection of an item from the messaging extension. Returns the selected item as a response.

        Args:
            turn_context (TurnContext): The context of the current turn of the conversation.
            query (dict): The query data containing the selected item information.
        
        Returns:
            MessagingExtensionResponse: A response containing the selected item as an attachment.
        """
        title = query.get("title")
        text = query.get("text")
        images = query.get("images", [])

        thumbnail_card = self.create_thumbnail_card(title, text, images)

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

    def create_thumbnail_card(self, title, text, images):
        """
        Creates a ThumbnailCard with the specified title, text, and images.
        
        Args:
            title (str): The title of the card.
            text (str): The body text of the card.
            images (list): A list of image URLs to include in the card.
        
        Returns:
            ThumbnailCard: The generated thumbnail card.
        """
        return ThumbnailCard(
            title=title,
            text=text,
            images=[CardImage(url=image) for image in images],
        )
