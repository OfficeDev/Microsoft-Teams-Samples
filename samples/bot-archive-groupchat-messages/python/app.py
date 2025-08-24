# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

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
    TurnContext,
    UserState,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes

from bots import TeamsBot
import logging
import traceback

# Create the loop and Flask app
from config import DefaultConfig
from dialogs import MainDialog

CONFIG = DefaultConfig()

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)


# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # This check writes out errors to console log .vs. app insights.
    # NOTE: In production environment, you should consider logging this to Azure
    #       application insights. See https://aka.ms/bottelemetry for telemetry
    #       configuration instructions.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a trace activity, which will be displayed in Bot Framework Emulator
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
    
    # Uncomment below commented line for local debugging.
    await context.send_activity(f"Sorry, it looks like something went wrong. Exception Caught: {error}")

    # Clear out state
    await CONVERSATION_STATE.delete(context)


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
    # Deserialize incoming request to Activity object
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers["Authorization"] if "Authorization" in req.headers else ""

    # Log incoming activity type and name
    logging.info(f"Incoming activity type: {activity.type}")
    logging.info(f"Activity name: {getattr(activity, 'name', None)}")

    try:
        # Process activity with the bot adapter and bot logic
        response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)

        # Handle special Invoke response case
        if isinstance(response, InvokeResponse):
            if isinstance(response.body, dict):
                return json_response(status=response.status, data=response.body)
            else:
                return Response(status=response.status)

        return Response(status=HTTPStatus.OK)

    except Exception as e:
        # Log unexpected errors during activity processing
        logging.error(f"Exception in processing activity: {e}")
        traceback.print_exc()
        return Response(
            status=HTTPStatus.INTERNAL_SERVER_ERROR,
            text="Internal server error while processing the activity."
        )


# Create aiohttp app and register message route
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

# Run aiohttp web server
if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
