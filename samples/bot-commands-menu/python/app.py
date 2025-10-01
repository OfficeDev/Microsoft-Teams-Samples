# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import (
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
)
from botbuilder.schema import Activity, ActivityTypes

from bots import TeamsCommandsMenuBot
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create the Bot Framework Authentication to be used with the Bot Adapter.
bot_framework_authentication = ConfigurationBotFrameworkAuthentication(CONFIG)

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
ADAPTER = CloudAdapter(bot_framework_authentication)

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

@web.middleware
async def error_middleware(request, handler):
    """Error handling middleware."""
    try:
        return await handler(request)
    except Exception as e:
        print(f"Error handling request: {e}")
        traceback.print_exc()
        return Response(status=HTTPStatus.INTERNAL_SERVER_ERROR)

async def messages(req: Request) -> Response:
    """Main bot message handler."""
    body = await req.text()
    auth_header = req.headers.get("Authorization", "")

    try:
        response = await ADAPTER.process_activity(body, auth_header, teams_commands_menu_bot.on_turn)
        if response:
            return json_response(data=response.body, status=response.status)
        return Response(status=HTTPStatus.OK)
    except Exception as e:
        print(f"Error processing activity: {e}")
        traceback.print_exc()
        return Response(status=HTTPStatus.INTERNAL_SERVER_ERROR)

# Create and configure the web application to listen for bot messages.
APP = web.Application(middlewares=[error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        # Run the web server.
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        # Handle any errors during server startup.
        raise error
