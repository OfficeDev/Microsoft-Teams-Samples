# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from http import HTTPStatus

from aiohttp import web
from botbuilder.core import TurnContext
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import Activity, ActivityTypes, InvokeResponse

from bots import BotActivityHandler
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create the adapter
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    try:
        await context.send_activity("The bot encountered an error or bug.")
    except Exception as send_error:
        print(f"Failed to send error message: {send_error}", file=sys.stderr)

    # Log additional details
    if context.activity:
        print(f"Activity ID: {context.activity.id}")
        print(f"Activity Type: {context.activity.type}")
        print(f"Activity Text: {context.activity.text}")


ADAPTER.on_turn_error = on_error

# Create bot instance
BOT = BotActivityHandler()


async def messages(request: web.Request) -> web.Response:
    return await ADAPTER.process(request, BOT)


APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="0.0.0.0", port=CONFIG.PORT)
    except Exception as error:
        print(f"Error starting app: {error}", file=sys.stderr)