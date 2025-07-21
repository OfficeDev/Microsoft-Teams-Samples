# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext, MessageFactory
from botbuilder.schema import HeroCard, CardAction, ActionTypes, Mention, Attachment
from botbuilder.core.teams import TeamsActivityHandler
from html import escape

class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()

        # Registers an activity event handler for the message activity event.
        # This is triggered whenever the bot receives a message.
        self.on_message_activity = self.handle_message

    async def handle_message(self, turn_context: TurnContext):
        """
        Handles incoming messages sent to the bot.

        Args:
            turn_context (TurnContext): Provides context for the current turn of conversation.
        """
        # Removes any mention of the bot in the received message text (e.g., "@BotName Hello").
        turn_context.remove_recipient_mention(turn_context.activity)

        # Extract the user's message text and strip any extra spaces.
        user_message = turn_context.activity.text.strip()

        # If the user's message is "Hello", respond with a personalized mention.
        if user_message == "Hello":
            await self.mention_activity_async(turn_context)
        else:
            # If the user's message is something else, respond with a hero card.
            value = {"count": 0}  # Example value to be passed with the card action.
            card = HeroCard(
                title="Let's talk...",  # Title of the card.
                buttons=[
                    # Add a button to the card that sends a "Hello" message back to the bot.
                    CardAction(
                        type=ActionTypes.message_back,  # Button type: sends a message back.
                        title="Say Hello",  # Button text displayed to the user.
                        value=value,  # Payload sent when the button is clicked.
                        text="Hello",  # Message text sent when the button is clicked.
                    )
                ],
            )
            # Convert the hero card to an attachment that can be sent in the bot's response.
            attachment = Attachment(
                content_type="application/vnd.microsoft.card.hero",  # Indicates this is a hero card.
                content=card  # The actual hero card content.
            )
            # Send the hero card as the bot's response.
            await turn_context.send_activity(
                MessageFactory.attachment(attachment)
            )

    async def mention_activity_async(self, turn_context: TurnContext):
        """
        Sends a reply that mentions the user.

        Args:
            turn_context (TurnContext): Provides context for the current turn of conversation.
        """
        # Get the user's information (e.g., name, ID) from the incoming activity.
        user = turn_context.activity.from_property

        # Construct a mention text using the user's name, encoded for HTML.
        mention_text = f"<at>{escape(user.name)}</at>"
        # Create a mention entity with the user's details and the mention text.
        mention_entity = Mention(mentioned=user, text=mention_text, type="mention")

        # Create a text message that includes the mention text.
        reply_activity = MessageFactory.text(f"Hi {mention_text}")
        # Attach the mention entity to the message.
        reply_activity.entities = [mention_entity]

        # Send the message with the mention to the user.
        await turn_context.send_activity(reply_activity)

