from botbuilder.core import ActivityHandler, TurnContext, MessageFactory
from botbuilder.schema import SuggestedActions, CardAction, ActionTypes
from config import DefaultConfig

CONFIG = DefaultConfig()


class SuggestedActionsBot(ActivityHandler):
    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        """
        Handles the event when new members are added to the conversation.
        :param members_added: List of members added to the conversation.
        :param context: The TurnContext object containing information about the turn.
        """
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Welcome to the bot! Let's get started with some suggestions."
                )
                await self._send_suggested_actions(turn_context)

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming messages and responds based on user input.
        :param turn_context: The TurnContext object containing information about the turn.
        """
        text = turn_context.activity.text

        match text:
            case "Hello":
                await turn_context.send_activity("Hello! How can I assist you today?")
            case "Welcome":
                await turn_context.send_activity("Welcome! How can I assist you today?")
            case _:
                await turn_context.send_activity(
                    "I didn't understand that. Please choose one of the suggested actions."
                )

        await self._send_suggested_actions(turn_context)

    async def _send_suggested_actions(self, turn_context: TurnContext):
        """
        Sends a message with suggested actions to the user.
        :param turn_context: The TurnContext object containing information about the turn.
        """
        actions = SuggestedActions(
            actions=[
                CardAction(type=ActionTypes.im_back, title="Hello", value="Hello"),
                CardAction(type=ActionTypes.im_back, title="Welcome", value="Welcome"),
                CardAction(
                    type="Action.Compose",
                    title="@SuggestedActionsBot",
                    value={
                        "type": "Teams.chatMessage",
                        "data": {
                            "body": {
                                "additionalData": {},
                                "backingStore": {
                                    "returnOnlyChangedValues": False,
                                    "initializationCompleted": True
                                },
                                "content": "<at id=\"0\">SuggestedActionsBot</at>"
                            },
                            "mentions": [
                                {
                                    "additionalData": {},
                                    "backingStore": {
                                        "returnOnlyChangedValues": False,
                                        "initializationCompleted": False
                                    },
                                    "id": 0,
                                    "mentioned": {
                                        "additionalData": {},
                                        "backingStore": {
                                            "returnOnlyChangedValues": False,
                                            "initializationCompleted": False
                                        },
                                        "odataType": "#microsoft.graph.chatMessageMentionedIdentitySet",
                                        "user": {
                                            "additionalData": {},
                                            "backingStore": {
                                                "returnOnlyChangedValues": False,
                                                "initializationCompleted": False
                                            },
                                            "displayName": "Suggested Actions Bot",
                                            "id": "28:" + CONFIG.APP_ID,
                                        }
                                    },
                                    "mentionText": "Suggested Actions Bot"
                                }
                            ],
                            "additionalData": {},
                            "backingStore": {
                                "returnOnlyChangedValues": False,
                                "initializationCompleted": True
                            }
                        }
                    }
                )
            ]
        )
        reply = MessageFactory.text("Choose one of the action from the suggested actions")
        reply.suggested_actions = actions
        await turn_context.send_activity(reply)