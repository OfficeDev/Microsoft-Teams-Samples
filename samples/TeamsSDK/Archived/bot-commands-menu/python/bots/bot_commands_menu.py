import json
from pathlib import Path

from botbuilder.core import ActivityHandler, CardFactory, MessageFactory, TurnContext

# Load Adaptive Card templates relative to the project root.
_RESOURCES_DIR = Path(__file__).resolve().parent.parent / "resources"

with open(_RESOURCES_DIR / "flightsDetails.json", "r") as f:
    FLIGHTS_DETAILS_CARD = json.load(f)

with open(_RESOURCES_DIR / "searchHotels.json", "r") as f:
    SEARCH_HOTELS_CARD = json.load(f)


class TeamsCommandsMenuBot(ActivityHandler):
    """Handles commands such as searching flights and hotels."""

    async def on_message_activity(self, turn_context: TurnContext):
        # Strip bot mentions (relevant in group/channel scopes).
        TurnContext.remove_mention_text(turn_context.activity, turn_context.activity.recipient.id)

        if turn_context.activity.text:
            text = turn_context.activity.text.strip().lower()

            if "search flights" in text:
                await self._send_flights_card(turn_context)
            elif "search hotels" in text:
                await self._send_hotels_card(turn_context)
            elif "help" in text:
                await turn_context.send_activity("Displays this help message.")
            elif "best time to fly" in text:
                await turn_context.send_activity("Best time to fly to London for a 5-day trip is summer.")

        elif turn_context.activity.value:
            value = turn_context.activity.value
            response = (
                f"Hotel search details are: {value.get('checkinDate')}, \n"
                f"Task CheckoutDate: {value.get('checkoutDate')}, \n"
                f"Location: {value.get('location')}, \n"
                f"NumberOfGuests: {value.get('numberOfGuests')}"
            )
            await turn_context.send_activity(response)

    async def _send_flights_card(self, turn_context: TurnContext):
        card = CardFactory.adaptive_card(FLIGHTS_DETAILS_CARD)
        await turn_context.send_activity(MessageFactory.attachment(card))

    async def _send_hotels_card(self, turn_context: TurnContext):
        card = CardFactory.adaptive_card(SEARCH_HOTELS_CARD)
        await turn_context.send_activity(MessageFactory.attachment(card))
