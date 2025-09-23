import sys
import traceback
from datetime import datetime
from http import HTTPStatus
from botbuilder.schema import InvokeResponse
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    BotFrameworkAdapter,
    BotFrameworkAdapterSettings,
    ConversationState,
    MemoryStorage,
    UserState,
    TurnContext
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes

from bots.teams_bot import TeamsBot
import logging

from config import DefaultConfig
from dialogs.main_dialog import MainDialog

CONFIG = DefaultConfig()


SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # Handles uncaught exceptions during turn execution
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Inform user that an error occurred
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )

    # Send a trace activity if using Bot Framework Emulator
    if context.activity.channel_id == "emulator":
        # Create a trace activity for debugging
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        # Send trace activity to emulator
        await context.send_activity(trace_activity)


# Assign the global error handler to the adapter
ADAPTER.on_turn_error = on_error

# Create MemoryStorage and state management objects
MEMORY = MemoryStorage()
USER_STATE = UserState(MEMORY)
CONVERSATION_STATE = ConversationState(MEMORY)

# Create dialog instance
DIALOG = MainDialog(CONFIG.CONNECTION_NAME)

# Create the main bot instance
BOT = TeamsBot(CONVERSATION_STATE, USER_STATE, DIALOG)

# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    # Deserialize imcoming request to Activity object
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)
    
    activity = Activity().deserialize(body)
    auth_header = req.headers["Authorization"] if "Authorization" in req.headers else ""

    # Log incoming activity type and name
    logging.info(f"Received activity of type: {activity.type}")
    logging.info(f"Activity name: {getattr(activity, 'name', None)}")

    try:
        # Process acitivity with the bot adapter and bot login
        response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)

        # Handle special Invoke response case
        if isinstance(response, InvokeResponse):
            if isinstance(response.body, dict):
                return json_response(status=response.status, data=response.body)
            else:
                return json_response(status=response.status)

        return json_response(status=HTTPStatus.OK)

    except Exception as e:
        # Log unexpected errors during activity processing
        logging.error(f"Error processing activity: {e}")
        traceback.print_exc()
        return Response(
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            text="An error occurred while processing the activity."
        )
    

APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

# Run aiohttp web server
if __name__ == "__main__":
    try:
        print(f"\nTag Mention Bot listening to http://localhost:{CONFIG.PORT}")
        print('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator')
        print('\nTo talk to your bot, open the emulator select "Open Bot"')
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error