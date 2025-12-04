#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import traceback
from datetime import datetime
from http import HTTPStatus
from aiohttp import web
from aiohttp.web import Request, Response, json_response
import aiohttp_jinja2
import jinja2
from botbuilder.core import (
    BotFrameworkAdapterSettings,
    BotFrameworkAdapter,
    TurnContext,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots.bot import MsgextReminderBot
from config import DefaultConfig

CONFIG = DefaultConfig()
APP = web.Application(middlewares=[aiohttp_error_middleware])
aiohttp_jinja2.setup(APP, loader=jinja2.FileSystemLoader("views"))
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Error handling
async def on_error(context: TurnContext, error: Exception):
    traceback.print_exc()
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("Please fix the bot source code to continue.")
    if context.activity.channel_id == "emulator":
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=str(error),
            value_type="https://www.botframework.com/schemas/error",
        )
        await context.send_activity(trace_activity)

ADAPTER.on_turn_error = on_error
BASE_URL = os.environ.get("BaseUrl")
BOT = MsgextReminderBot(base_url=BASE_URL, adapter=ADAPTER)

async def messages(req: Request) -> Response:
    if "application/json" in req.headers.get("Content-Type", ""):
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)
    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")
    response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)
    if response:
        return json_response(data=response.body, status=response.status)
    return Response(status=HTTPStatus.OK)

@aiohttp_jinja2.template("ScheduleTask.html")
async def schedule_task(request: Request):
    return {}

# Register routes
APP.router.add_post("/api/messages", messages)
APP.router.add_get("/scheduleTask", schedule_task)
APP.router.add_static("/Images", path="Images", name="images")

# Start web server
if __name__ == "__main__":
    try:
        port = CONFIG.PORT or 3978
        web.run_app(APP, host="localhost", port=port)
    except Exception as e:
        raise e
