from botbuilder.core import ActivityHandler, TurnContext, MessageFactory
from botbuilder.schema import SuggestedActions, CardAction, ActionTypes


class SuggestedActionsBot(ActivityHandler):
    def __init__(self):
        super(SuggestedActionsBot, self).__init__()

        # Register the on_members_added handler
        async def on_members_added(members_added, context: TurnContext):
            await self.handle_members_added(members_added, context)

        self.on_members_added_activity = on_members_added

        # Register the on_message handler
        async def on_message(context: TurnContext):
            await self.handle_message_activity(context)

        self.on_message_activity = on_message

    async def handle_members_added(self, members_added, context: TurnContext):
        """
        Handles the event when new members are added to the conversation.
        :param members_added: List of members added to the conversation.
        :param context: The TurnContext object containing information about the turn.
        """
        for member in members_added:
            # Avoid sending the welcome message to the bot itself.
            if member.id != context.activity.recipient.id:
                welcome_message = (
                    "Welcome to the bot! Let's get started with some suggestions."
                )
                await context.send_activity(welcome_message)
                await self.send_suggested_actions(context)

    async def handle_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming messages and responds based on user input.
        :param turn_context: The TurnContext object containing information about the turn.
        """
        user_message = turn_context.activity.text

        # Define valid options
        valid_colors = ["Red", "Blue", "Yellow"]

        if user_message in valid_colors:
            await turn_context.send_activity(f"I agree, {user_message} is a great color!")
        else:
            await turn_context.send_activity("Please choose a valid color.")

        # Send suggested actions for the next turn
        await self.send_suggested_actions(turn_context)

    async def send_suggested_actions(self, turn_context: TurnContext):
        """
        Sends a message with suggested actions to the user.
        :param turn_context: The TurnContext object containing information about the turn.
        """
        actions = SuggestedActions(
            actions=[
                CardAction(type=ActionTypes.im_back, title="Red", value="Red"),
                CardAction(type=ActionTypes.im_back, title="Blue", value="Blue"),
                CardAction(
                    type="Action.Compose",
                    title="@Facilitator",
                    value={
                        "type": "Teams.chatMessage",
                        "data": {
                            "body": {
                                "additionalData": {},
                                "backingStore": {
                                    "returnOnlyChangedValues": False,
                                    "initializationCompleted": True
                                },
                                "content": "<at id=\"0\">Facilitator</at>"
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
                                            "displayName": "Facilitator",
                                            "id": "28:8e55a7b1-6766-4f0a-8610-ecacfe3d569a"
                                        }
                                    },
                                    "mentionText": "Facilitator"
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
        reply = MessageFactory.text("What is your favorite color?")
        reply.suggested_actions = actions
        await turn_context.send_activity(reply)