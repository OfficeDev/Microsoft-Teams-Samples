# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    TurnContext
)
from botbuilder.integration.aiohttp import (
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes

from bots import TeamsCommandsMenuBot
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

async def on_error(context: TurnContext, error: Exception):
    """Handles errors encountered during bot operation."""
    # Log error details for debugging.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Inform the user about the error in the bot.
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue running this bot, please fix the source code.")

    # Send detailed trace activity if running in Bot Framework Emulator.
    if context.activity.channel_id == "emulator":
        # Create a trace activity that contains the error object
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        await context.send_activity(trace_activity)

# Set up error handling for the adapter.
ADAPTER.on_turn_error = on_error

# Initialize the Bot.
teams_commands_menu_bot = TeamsCommandsMenuBot()

async def handle_incoming_messages(req: Request) -> Response:
    """Processes incoming messages and routes them to the bot."""
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    # Process the activity using the adapter and bot.
    response = await ADAPTER.process_activity(activity, auth_header, teams_commands_menu_bot.on_turn)
    if response:
        return json_response(data=response.body, status=response.status)
    
    return Response(status=HTTPStatus.OK)

# Create and configure the web application to listen for bot messages.
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", handle_incoming_messages)

if __name__ == "__main__":
    try:
        # Run the web server.
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        # Handle any errors during server startup.
        raise error
