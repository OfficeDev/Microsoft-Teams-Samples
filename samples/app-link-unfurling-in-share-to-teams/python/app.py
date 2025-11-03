# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import sys
import traceback
from datetime import datetime
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core import (
    TurnContext,
)
from botbuilder.schema import Activity, ActivityTypes

from bots.bot import LinkUnfurlingBot 
from config import DefaultConfig
CONFIG = DefaultConfig()

APP_ID = os.getenv("MicrosoftAppId", "")
APP_PASSWORD = os.getenv("MicrosoftAppPassword", "")
PORT = int(os.getenv("PORT", 3978))

ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

async def on_error(context: TurnContext, error: Exception):
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

BOT = LinkUnfurlingBot()

async def messages(req: Request) -> Response:
    

    return await ADAPTER.process(req, BOT)

async def serve_tab_page(request):
    file_path = os.path.join("templates", "tab.html")
    if os.path.isfile(file_path):
        return web.FileResponse(path=file_path, headers={"Content-Type": "text/html"})
    return web.Response(text="tab.html not found", status=404)

APP = web.Application()

APP.router.add_post("/api/messages", messages)
APP.router.add_get("/tab", serve_tab_page)
APP.router.add_static("/static", path="static", name="static")
APP.router.add_static("/Images", path="Images", name="images")
APP.router.add_route("*", "/{tail:.*}", lambda req: web.json_response({"error": "Route not found"}, status=404))

if __name__ == "__main__":
    try:
        print(f"Bot running on http://localhost:{PORT}")
        web.run_app(APP, port=PORT)
    except Exception as error:
        raise error
