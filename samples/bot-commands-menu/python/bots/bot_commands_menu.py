from botbuilder.core import ActivityHandler, TurnContext, CardFactory
from botbuilder.schema import Activity

# Import JSON templates for Adaptive Cards
import json
with open("resources/flightsDetails.json", "r") as flights_file:
    FlightsDetailsCardTemplate = json.load(flights_file)

with open("resources/searchHotels.json", "r") as hotels_file:
    SearchHotelsCardTemplate = json.load(hotels_file)

class TeamsCommandsMenuBot(ActivityHandler):
    """
    TeamsCommandsMenuBot is a bot that handles commands such as searching flights and hotels. 
    It processes incoming messages and sends adaptive cards based on user input.
    """

    def __init__(self):
        """
        Initializes the TeamsCommandsMenuBot class.
        Sets up the message activity handler to respond to user commands.
        """
        super(TeamsCommandsMenuBot, self).__init__()

        async def on_message_activity(context: TurnContext):
            """
            Handles incoming message activities. It processes text commands and sends appropriate responses.
            Supports commands like 'search flights', 'search hotels', 'help', and 'best time to fly'.
            
            Args:
                context (TurnContext): The context of the incoming activity.
            """
            # Remove mentions from the message text
            activity = TurnContext.remove_mention_text(context.activity, context.activity.recipient.id)

            if activity:
                if context.activity.text:
                    text = context.activity.text.strip().lower()

                    if 'search flights' in text:
                        await self.search_flights_reader_card_async(context)
                    elif 'search hotels' in text:
                        await self.search_hotels_reader_card_async(context)
                    elif 'help' in text:
                        await context.send_activity("Displays this help message.")
                    elif 'best time to fly' in text:
                        await context.send_activity("Best time to fly to London for a 5-day trip is summer.")

            elif context.activity.value:
                # Extract hotel search details from the activity's value
                value = context.activity.value
                response = (
                    f"Hotel search details are: {value.get('checkinDate')}, \n"
                    f"Task CheckoutDate: {value.get('checkoutDate')}, \n"
                    f"Location: {value.get('location')}, \n"
                    f"NumberOfGuests: {value.get('numberOfGuests')}"
                )
                await context.send_activity(response)

            # Ensure the next bot handler is called
            await super(TeamsCommandsMenuBot, self).on_message_activity(context)

        self.on_message_activity = on_message_activity

    async def search_flights_reader_card_async(self, context: TurnContext):
        """
        Sends the flight details Adaptive Card to the user.
        
        Args:
            context (TurnContext): The context of the incoming activity.
        """
        await context.send_activity(
            Activity(
                attachments=[CardFactory.adaptive_card(FlightsDetailsCardTemplate)]
            )
        )

    async def search_hotels_reader_card_async(self, context: TurnContext):
        """
        Sends the hotel search Adaptive Card to the user.
        
        Args:
            context (TurnContext): The context of the incoming activity.
        """
        await context.send_activity(
            Activity(
                attachments=[CardFactory.adaptive_card(SearchHotelsCardTemplate)]
            )
        )
