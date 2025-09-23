# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from aiohttp import web
from aiohttp.web import Request, Response
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import DailyReminderBot 
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create the adapter
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


async def on_error(context: TurnContext, error: Exception):
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
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

FILE_PATHS = {
    "scheduletask": 'src/views/scheduletask.html',
}

async def scheduletask(request):
    """Handles requests to the scheduletask page."""
    return web.FileResponse(FILE_PATHS["scheduletask"])

bot = DailyReminderBot(CONFIG.APP_ID, CONFIG.APP_PASSWORD)

# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    return await ADAPTER.process(req, bot)

APP = web.Application(middlewares=[aiohttp_error_middleware])

APP.router.add_get("/scheduletask", scheduletask)
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
