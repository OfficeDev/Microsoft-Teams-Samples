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
    BotFrameworkAdapterSettings,
    TurnContext,
    BotFrameworkAdapter,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from jinja2 import Environment, FileSystemLoader
from bots import BotActivityHandler
from config import DefaultConfig
from services.language_service import get_translated_res

CONFIG = DefaultConfig()

# Localization setup
LOCALES_DIR = "./translations"
DEFAULT_LOCALE = "en-us"
env = Environment(loader=FileSystemLoader("templates"))

# Create adapter.
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    await context.send_activity("The bot encountered an unexpected error. Please check the bot's logs for more details.")
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

APP_ID = SETTINGS.app_id if SETTINGS.app_id else uuid.uuid4()
BOT = BotActivityHandler()

# Main bot message handler.
async def messages(req: Request) -> Response:
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers["Authorization"] if "Authorization" in req.headers else ""

    response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)
    if response:
        return json_response(data=response.body, status=response.status)
    return Response(status=HTTPStatus.OK)

# Static tab route
async def static_tab(req: Request) -> Response:
    # Extract culture parameter from query string
    locale = req.query.get("culture", DEFAULT_LOCALE)
    translations = get_translated_res(locale)

    # Render HTML with translations
    template = env.get_template("static_tab.html")
    content = template.render(translations=translations)
    return Response(text=content, content_type="text/html")

# Set up routes
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)
APP.router.add_get("/", static_tab)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error