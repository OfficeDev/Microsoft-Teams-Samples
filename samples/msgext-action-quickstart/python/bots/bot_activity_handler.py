from botbuilder.core import ActivityHandler, TurnContext, CardFactory
from botbuilder.schema import HeroCard, InvokeResponse
from botbuilder.schema._models_py3 import CardImage


class BotActivityHandler(ActivityHandler):
    async def on_invoke_activity(self, turn_context: TurnContext):
        """
        Handle invoke activities (e.g., messaging extension actions).
        """
        activity = turn_context.activity
        if activity.name == "composeExtension/submitAction":
            action = activity.value
            if action.get("commandId") == "createCard":
                return await self.create_card_command(turn_context, action)
            elif action.get("commandId") == "shareMessage":
                return await self.share_message_command(turn_context, action)
            else:
                return self._create_invoke_response(
                    status=400, body={"error": "Command not implemented"}
                )
        elif activity.name == "composeExtension/queryLink":
            return await self.handle_query_link(turn_context, activity.value)
        return await super().on_invoke_activity(turn_context)
    
    async def handle_query_link(self, turn_context: TurnContext, query):
        """
        Handle unfurling links for messaging extensions.
        """
        attachment = CardFactory.thumbnail_card(
            title="Thumbnail Card",
            images=[query["url"]],
            buttons=[
                {
                    "type": "openUrl",
                    "title": "View Package",
                    "value": "https://www.npmjs.com/package/level-option-wrap",
                }
            ],
        )
        result = {
            "composeExtension": {
                "type": "result",
                "attachmentLayout": "list",
                "attachments": [attachment],
            }
        }
        return self._create_invoke_response(status=200, body=result)


    async def create_card_command(self, context: TurnContext, action: dict) -> InvokeResponse:
        """
        Create a card in response to the 'createCard' action.
        """
        data = action.get("data", {})
        hero_card = HeroCard(
            title=data.get("title", ""),
            text=data.get("text", ""),
            subtitle=data.get("subTitle", "")
        )

        # Create an attachment using CardFactory
        attachment = CardFactory.hero_card(hero_card)

        # Serialize the attachment for JSON serialization
        serialized_attachment = attachment.serialize()

        result = {
            "composeExtension": {
                "type": "result",
                "attachmentLayout": "list",
                "attachments": [serialized_attachment],
            }
        }
        return self._create_invoke_response(status=200, body=result)

    async def share_message_command(self, context: TurnContext, action: dict) -> InvokeResponse:
        """
        Share a message in response to the 'shareMessage' action.
        """
        user_name = "unknown"
        message_payload = action.get("messagePayload", {})
        if "from" in message_payload and "user" in message_payload["from"]:
            user_name = message_payload["from"]["user"].get("displayName", user_name)

        images = []
        include_image = action.get("data", {}).get("includeImage", "false")
        if include_image == "true":
            images = [CardImage(url="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU")]

        # Create the HeroCard
        hero_card = HeroCard(
            title=f"{user_name} originally sent this message:",
            text=message_payload.get("body", {}).get("content", ""),
            subtitle=f"({len(message_payload.get('attachments', []))} attachments not included)"
            if "attachments" in message_payload
            else None,
            images=images,
        )

        # Convert HeroCard to an attachment using CardFactory
        attachment = CardFactory.hero_card(hero_card)
        serialized_attachment = attachment.serialize()

        result = {
            "composeExtension": {
                "type": "result",
                "attachmentLayout": "list",
                "attachments": [serialized_attachment],
            }
        }

        # Return InvokeResponse with the result
        return InvokeResponse(status=200, body=result)
    
    def _create_invoke_response(self, status: int, body: dict) -> InvokeResponse:
        """
        Helper to create an InvokeResponse.
        """
        return InvokeResponse(status=status, body=body)