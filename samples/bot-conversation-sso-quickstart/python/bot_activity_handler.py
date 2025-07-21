# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import (
    TurnContext,
    MessageFactory,
    CardFactory,
)
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import (
    Activity,
    Mention,
    ActionTypes,
    HeroCard,
    CardAction,
)
import html


class BotActivityHandler(TeamsActivityHandler):
    """
    BotActivityHandler class extends TeamsActivityHandler to handle Teams-specific activities.
    """

    def __init__(self):
        super().__init__()
        # Teams bots are Microsoft Bot Framework bots.
        # If a bot receives a message activity, the turn handler sees that incoming activity
        # and sends it to the on_message_activity handler.
        # Learn more: https://aka.ms/teams-bot-basics.
        #
        # NOTE: Ensure the bot endpoint that services incoming conversational bot queries is
        #       registered with Bot Framework.
        #       Learn more: https://aka.ms/teams-register-bot.

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming message activities.
        """
        # Remove recipient mention from the activity text
        TurnContext.remove_recipient_mention(turn_context.activity)
        
        text = (turn_context.activity.text or "").strip()
        
        if text.lower() == "hello":
            await self._mention_activity(turn_context)
        else:
            # By default for unknown activity sent by user show
            # a card with the available actions.
            await self._send_hero_card(turn_context)

    async def _mention_activity(self, turn_context: TurnContext):
        """
        Say hello and @ mention the current user.
        """
        # Create mention
        mention = Mention(
            mentioned=turn_context.activity.sender,
            text=f"<at>{html.escape(turn_context.activity.sender.name or 'User')}</at>",
            type="mention"
        )
        
        # Create reply activity with mention
        reply_text = f"Hi {mention.text}"
        reply_activity = MessageFactory.text(reply_text)
        reply_activity.entities = [mention]
        
        await turn_context.send_activity(reply_activity)

    async def _send_hero_card(self, turn_context: TurnContext):
        """
        Send a hero card with available actions.
        """
        # Create hero card
        card = HeroCard(
            title="Let's talk...",
            subtitle=None,
            text=None,
            images=None,
            buttons=[
                CardAction(
                    type=ActionTypes.message_back,
                    title="Say Hello",
                    value={"count": 0},
                    text="Hello"
                )
            ]
        )
        
        # Create card attachment
        card_attachment = CardFactory.hero_card(card)
        
        # Send card
        response = MessageFactory.attachment(card_attachment)
        await turn_context.send_activity(response)
