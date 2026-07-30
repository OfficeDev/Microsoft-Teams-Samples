# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime, timezone
from http import HTTPStatus

from aiohttp import web
import os
from aiohttp.web import Request, Response, json_response
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from server.bots import BotSequentialFlowAdaptiveCard
from config import DefaultConfig

CONFIG = DefaultConfig()

bot_authentication = ConfigurationBotFrameworkAuthentication(CONFIG)

ADAPTER = CloudAdapter(bot_authentication)

async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
    if context.activity.channel_id == "emulator":
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.now(timezone.utc),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        await context.send_activity(trace_activity)


ADAPTER.on_turn_error = on_error

# Create the Bot
BOT = BotSequentialFlowAdaptiveCard()

async def messages(req: Request) -> Response:
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    invoke_response = await ADAPTER.process_activity(auth_header, activity, BOT.on_turn)

    if activity.type == ActivityTypes.invoke and invoke_response:
        return json_response(data=invoke_response.body, status=invoke_response.status)

    return Response(status=HTTPStatus.OK)

APP = web.Application(middlewares=[aiohttp_error_middleware])

# Serve images from /images/ folder
STATIC_DIR = os.path.join(os.path.dirname(__file__), "Images")
APP.router.add_static("/Images/", STATIC_DIR, show_index=True)

APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
