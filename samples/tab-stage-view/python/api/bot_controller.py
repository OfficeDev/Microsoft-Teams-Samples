# api/bot_controller.py

import sys
import traceback
from http import HTTPStatus
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    TurnContext,
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import Activity
from bots.bot_activity_handler import BotActivityHandler
import os

from config import DefaultConfig
CONFIG = DefaultConfig()
# Adapter settings using environment variables
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# Error handler
async def on_error(context: TurnContext, error: Exception):
    print(f"\n[on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue, please fix the bot source code.")

    # Send a trace activity (for Emulator)
    await context.send_trace_activity(
        name="OnTurnError Trace",
        value=str(error),
        value_type="https://www.botframework.com/schemas/error",
        label="TurnError"
    )

ADAPTER.on_turn_error = on_error

# Instantiate the bot
bot = BotActivityHandler()

# Main request handler for /api/messages
async def messages(request: Request) -> Response:
    
    return await ADAPTER.process(request, bot)
