# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from http import HTTPStatus

from botbuilder.core import CardFactory, InvokeResponse, TurnContext
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import HeroCard
from botbuilder.schema._models_py3 import CardImage
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionActionResponse,
    MessagingExtensionAttachment,
    MessagingExtensionResult,
)

SHARED_IMAGE_URL = (
    "https://encrypted-tbn0.gstatic.com/images?"
    "q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU"
)


class BotActivityHandler(TeamsActivityHandler):
    """Handles messaging extension action commands."""

    async def on_invoke_activity(self, turn_context: TurnContext) -> InvokeResponse:
        # Handle composeExtension/submitAction here directly so the response
        # model is returned to the CloudAdapter without being pre-serialized
        # by TeamsActivityHandler._create_invoke_response (which would lead to
        # double-serialization in CloudAdapter.process).
        if turn_context.activity.name == "composeExtension/submitAction":
            action = MessagingExtensionAction().deserialize(
                turn_context.activity.value
            )
            response = await self.on_teams_messaging_extension_submit_action(
                turn_context, action
            )
            return InvokeResponse(status=int(HTTPStatus.OK), body=response)

        return await super().on_invoke_activity(turn_context)

    async def on_teams_messaging_extension_submit_action(
        self,
        turn_context: TurnContext,
        action: MessagingExtensionAction,
    ) -> MessagingExtensionActionResponse:
        if action.command_id == "createCard":
            return self._create_card_command(action)
        if action.command_id == "shareMessage":
            return self._share_message_command(action)
        return MessagingExtensionActionResponse()

    def _create_card_command(
        self, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        data = action.data or {}
        card = CardFactory.hero_card(
            HeroCard(
                title=data.get("title", ""),
                subtitle=data.get("subTitle", ""),
                text=data.get("text", ""),
            )
        )
        return self._build_response(card)

    def _share_message_command(
        self, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        message_payload = action.message_payload

        user_name = "unknown"
        if (
            message_payload
            and message_payload.from_property
            and message_payload.from_property.user
        ):
            user_name = (
                message_payload.from_property.user.display_name or user_name
            )

        include_image = (action.data or {}).get("includeImage") == "true"
        images = [CardImage(url=SHARED_IMAGE_URL)] if include_image else []

        attachments_count = (
            len(message_payload.attachments)
            if message_payload and message_payload.attachments
            else 0
        )
        body_content = ""
        if message_payload and message_payload.body:
            body_content = message_payload.body.content or ""

        card = CardFactory.hero_card(
            HeroCard(
                title=f"{user_name} originally sent this message:",
                text=body_content,
                subtitle=f"({attachments_count} attachments not included)",
                images=images,
            )
        )
        return self._build_response(card)

    @staticmethod
    def _build_response(card) -> MessagingExtensionActionResponse:
        attachment = MessagingExtensionAttachment(
            content=card.content,
            content_type=card.content_type,
            preview=card,
        )
        return MessagingExtensionActionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",
                attachment_layout="list",
                attachments=[attachment],
            )
        )
