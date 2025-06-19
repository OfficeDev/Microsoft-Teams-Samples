import json
from botbuilder.core import TurnContext, MessageFactory
from botbuilder.schema import Activity, Attachment, InvokeResponse
from botbuilder.core.teams import TeamsActivityHandler

class BotActivityHandler(TeamsActivityHandler):
    ALL_USERS_CARD_TYPE = "All Users"
    ME_CARD_TYPE = "Me"

    def __init__(self, card_factory):
        if not card_factory:
            raise ValueError("card_factory cannot be None")
        self.card_factory = card_factory

    async def on_message_activity(self, turn_context: TurnContext):
        # Send initial card.
        initial_card = self.card_factory.get_select_card_type_card()
        await turn_context.send_activity(
            MessageFactory.attachment(initial_card)
        )

    async def on_invoke_activity(self, turn_context: TurnContext):
        activity = turn_context.activity

        if activity.name == "adaptiveCard/action":
            action_data = json.loads(json.dumps(activity.value))  # ensure dict format

            verb = action_data.get("action", {}).get("verb")
            data = action_data.get("action", {}).get("data", {})
            data["refreshCount"] = int(data.get("refreshCount", 0)) + 1

            if verb == "me":
                card = self.card_factory.get_auto_refresh_for_specific_user_base_card(
                    activity.from_property.id, self.ME_CARD_TYPE
                )
                await turn_context.send_activity(MessageFactory.attachment(card))

            elif verb == "allusers":
                card = self.card_factory.get_auto_refresh_for_all_users_base_card(
                    self.ALL_USERS_CARD_TYPE
                )
                await turn_context.send_activity(MessageFactory.attachment(card))

            elif verb == "UpdateBaseCard":
                card = self.card_factory.get_final_base_card(action_data)
                updated_activity = MessageFactory.attachment(card)
                updated_activity.id = activity.reply_to_id
                await turn_context.update_activity(updated_activity)

            elif verb == "RefreshUserSpecificView":
                card = self.card_factory.get_updated_card_for_user(
                    activity.from_property.id, action_data
                )
                return self.prepare_invoke_response(card)

        # Default response
        return InvokeResponse(
            status=200,
            body={
                "type": "application/vnd.microsoft.activity.message",
                "value": "Success!"
            }
        )

    @staticmethod
    def prepare_invoke_response(card: Attachment) -> InvokeResponse:
        return InvokeResponse(
            status=200,
            body={
                "type": card.content_type,
                "value": card.content
            }
        )