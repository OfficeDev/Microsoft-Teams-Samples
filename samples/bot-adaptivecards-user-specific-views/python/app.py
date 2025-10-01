# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
import uuid
from datetime import datetime
from http import HTTPStatus
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    TurnContext,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes

from config import DefaultConfig
from bots.teams_conversation_bot import BotActivityHandler
from card_factory.card_factory import CardFactory  
from config import DefaultConfig
CONFIG = DefaultConfig()

# Create adapter.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue to run this bot, please fix the bot source code.")

    if context.activity.channel_id == "emulator":
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        await context.send_activity(trace_activity)

ADAPTER.on_turn_error = on_error



# ✅ Use real CardFactory here
card_factory = CardFactory()
BOT = BotActivityHandler(card_factory)

# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    
    return await ADAPTER.process(req, BOT)

APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error