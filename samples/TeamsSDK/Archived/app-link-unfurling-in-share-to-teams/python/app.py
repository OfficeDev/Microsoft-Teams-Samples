# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import sys
import traceback
from datetime import datetime

from aiohttp import web
from aiohttp.web import Request, Response
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import Activity, ActivityTypes

from bots import LinkUnfurlingBot
from config import DefaultConfig

CONFIG = DefaultConfig()

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

BOT = LinkUnfurlingBot()


# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    return await ADAPTER.process(req, BOT)


async def serve_tab_page(request: Request) -> Response:
    file_path = os.path.join("templates", "tab.html")
    if os.path.isfile(file_path):
        return web.FileResponse(path=file_path, headers={"Content-Type": "text/html"})
    return web.Response(text="tab.html not found", status=404)


APP = web.Application()
APP.router.add_post("/api/messages", messages)
APP.router.add_get("/tab", serve_tab_page)
APP.router.add_static("/static", path="static", name="static")
APP.router.add_static("/Images", path="Images", name="images")

if __name__ == "__main__":
    try:
        print(f"Bot running on http://localhost:{CONFIG.PORT}")
        web.run_app(APP, port=CONFIG.PORT)
    except Exception as error:
        raise error

