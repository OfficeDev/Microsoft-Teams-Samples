# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
import uuid
from datetime import datetime
from http import HTTPStatus
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    TurnContext,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import BotActivityHandler
from config import DefaultConfig
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication


CONFIG = DefaultConfig()

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # This check writes out errors to console log .vs. app insights.
    # NOTE: In production environment, you should consider logging this to Azure
    #       application insights.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    await context.send_activity("The bot encountered an error or bug. Please check the logs for more details.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
    # Send a trace activity if we're talking to the Bot Framework Emulator
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
        # Send a trace activity, which will be displayed in Bot Framework Emulator
        await context.send_activity(trace_activity)

ADAPTER.on_turn_error = on_error

# If the channel is the Emulator, and authentication is not in use, the AppId will be null.
# We generate a random AppId for this case only. This is not required for production, since
# the AppId will have a value.
#APP_ID = SETTINGS.app_id if SETTINGS.app_id else uuid.uuid4()

# Create the Bot
BOT = BotActivityHandler()

# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    # Main bot message handler.
    return await ADAPTER.process(req, BOT)

# Listen for incoming requests on /api/messages.
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
