# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import sys
import traceback
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response

from botbuilder.core import (
    BotFrameworkAdapter,
    BotFrameworkAdapterSettings,
    TurnContext,
)
from botbuilder.schema import Activity

from bots import SentimentAnalysis
from config import DefaultConfig

# Load configuration
CONFIG = DefaultConfig()

# Set up the adapter
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)


# Error handler for adapter
async def on_error(context: TurnContext, error: Exception):
    print(f"\n[on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue, please fix the bot source code.")


ADAPTER.on_turn_error = on_error

# Initialize the bot
BOT = SentimentAnalysis()

# aiohttp application
APP = web.Application()


# Endpoint to receive messages from the Bot Framework
async def messages(req: Request) -> Response:
    if "application/json" not in req.headers["Content-Type"]:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    body = await req.json()
    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)

    if response:
        return web.json_response(data=response.body, status=response.status)
    return Response(status=HTTPStatus.OK)

# Register routes
APP.router.add_post("/api/messages", messages)

# Run the app
if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as e:
        print(f"Error starting app: {e}")
