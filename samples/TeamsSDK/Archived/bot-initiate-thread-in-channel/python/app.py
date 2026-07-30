# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime, timezone
from http import HTTPStatus
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import TeamsStartThreadInChannel
from config import DefaultConfig

# Configuration settings, loaded from the DefaultConfig class
CONFIG = DefaultConfig()

# The Bot Framework Adapter that handles communication between the bot and the Bot Framework.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


def log_error(error: Exception):
    """
    Logs an error to the console and prints the stack trace.
    
    This function is intended to be used for logging errors in the bot, 
    especially in development environments.

    """
    # Logs the error to the standard error stream and prints the traceback.
    print(f"Error: {error}", file=sys.stderr)
    traceback.print_exc()


async def on_error(context: TurnContext, error: Exception):
    """
    Handles unhandled errors during bot interactions.
    
    This function logs the error, sends a message to the user, and if the bot 
    is being tested in the Emulator, sends a trace activity with detailed error 
    information.
    
    """
    # Logs the error using the log_error function.
    log_error(error)

    # Send a generic error message to the user.
    await context.send_activity("The bot encountered an error or bug.")
    
    # If the bot is being tested in the Emulator, send a trace activity with the error details.
    if context.activity.channel_id == "emulator":
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.now(timezone.utc),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        # Send the trace activity, which will be visible in the Bot Framework Emulator.
        await context.send_activity(trace_activity)


# Register the error handler with the adapter
ADAPTER.on_turn_error = on_error

# The bot instance that will handle incoming activities.
bot = TeamsStartThreadInChannel()


async def messages(req: Request) -> Response:
    """
    Handles incoming bot messages.

    This function processes incoming requests to the '/api/messages' endpoint. It deserializes 
    the incoming request body into an Activity object, processes it using the Bot Framework 
    Adapter, and returns the appropriate response.

    """
    # Check if the Content-Type is 'application/json'. If not, return an error response.
    if "application/json" not in req.headers.get("Content-Type", ""):
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    # Process the incoming activity using the Bot Framework Adapter.
    response = await ADAPTER.process(req, bot)

    # If a response is returned, send it; otherwise, return a 200 OK status.
    return json_response(data=response.body, status=response.status) if response else Response(status=HTTPStatus.OK)


# Create and configure the web application with error middleware.
APP = web.Application(middlewares=[aiohttp_error_middleware])

# Define the route for the '/api/messages' endpoint, which listens for incoming POST requests.
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    web.run_app(APP, host="localhost", port=CONFIG.PORT)
