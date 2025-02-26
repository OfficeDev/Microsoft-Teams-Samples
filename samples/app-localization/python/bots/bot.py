from botbuilder.core import TurnContext, MessageFactory
from botbuilder.core.teams import TeamsActivityHandler
from services.language_service import get_translated_res

class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        # Teams bots are Microsoft Bot Framework bots.
        # If a bot receives a message activity, the turn handler sees that incoming activity
        # and sends it to the on_message_activity handler.
        # Learn more: https://aka.ms/teams-bot-basics

        # Registers an activity event handler for the message event, emitted for every incoming message activity.

    async def on_message_activity(self, turn_context: TurnContext):
        # Removes recipient mention from the activity text
        TurnContext.remove_recipient_mention(turn_context.activity)

        # Get the locale from the incoming activity
        locale = turn_context.activity.locale

        # Get the translated response based on the locale
        text = get_translated_res(locale).get("welcome", "Welcome")

        # Reply with the translated text
        await self.reply_activity_async(turn_context, text)

    async def reply_activity_async(self, turn_context: TurnContext, text: str):
        reply_activity = MessageFactory.text(text)
        await turn_context.send_activity(reply_activity)
