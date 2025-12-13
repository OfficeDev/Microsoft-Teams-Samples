# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import os
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
from bots import TeamsBot
from config import DefaultConfig  # Ensure this has your App ID and App Password

# Fetch configuration
CONFIG = DefaultConfig()

# Create adapter settings from environment variables or default config
SETTINGS = BotFrameworkAdapterSettings(
    CONFIG.APP_ID, CONFIG.APP_PASSWORD
)

# Create the Bot Framework Adapter instance
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors during bot processing
async def on_error(context: TurnContext, error: Exception):
    # Log the error for debugging
    print(f"\n[on_turn_error] Unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a friendly message to the user about the error
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("Please fix the bot source code to continue.")

    # Optionally log errors to the Emulator for debugging purposes
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

# Base directory for resolving paths (assuming HTML views are located here)
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
FILE_PATHS = {
    "tab": os.path.join(BASE_DIR, "src", "views", "hello.html"),
    "configure": os.path.join(BASE_DIR, "src", "views", "configure.html")
}

# Route for tab page
async def tab(request):
    file_path = FILE_PATHS["tab"]
    if not os.path.exists(file_path):
        return web.Response(text=f"File not found: {file_path}", status=404)
    return web.FileResponse(file_path)

# Route for configure page
async def configure(request):
    file_path = FILE_PATHS["configure"]
    if not os.path.exists(file_path):
        return web.Response(text=f"File not found: {file_path}", status=404)
    return web.FileResponse(file_path)

# Bot setup
APP_ID = SETTINGS.app_id
if not APP_ID:
    raise ValueError("App ID is missing in the config!")

APP_PASSWORD = SETTINGS.app_password
if not APP_PASSWORD:
    raise ValueError("App Password is missing in the config!")

bot = TeamsBot()

# Bot message endpoint (incoming activity processing)
async def messages(req: Request) -> Response:
    if "application/json" in req.headers.get("Content-Type", ""):
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    # Process activity using the Bot Framework Adapter
    response = await ADAPTER.process_activity(activity, auth_header, bot.on_turn)
    if response:
        return json_response(data=response.body, status=response.status)

    return Response(status=HTTPStatus.OK)

# Web app setup with error handling middleware
APP = web.Application(middlewares=[aiohttp_error_middleware])

# Register routes
APP.router.add_get("/tab", tab)
APP.router.add_get("/configure", configure)
APP.router.add_post("/api/messages", messages)

# Start the web app (listening for incoming requests)
if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        print(f"Error while starting the server: {error}")
        raise error

