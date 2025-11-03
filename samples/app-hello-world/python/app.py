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
    TurnContext
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import HelloWorldBot 
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create adapter.
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))



# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # This check writes out errors to console log .vs. app insights.
    # NOTE: In production environment, you should consider logging this to Azure
    #       application insights.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
    # Send a trace activity if we're talking to the Bot Framework Emulator
    if context.activity.channel_id == "emulator":
        # Create a trace activity that contains the error object
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.utcnow(),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        # Send a trace activity, which will be displayed in Bot Framework Emulator
        await context.send_activity(trace_activity)


ADAPTER.on_turn_error = on_error

# If the channel is the Emulator, and authentication is not in use, the AppId will be null.
# We generate a random AppId for this case only. This is not required for production, since
# the AppId will have a value.

FILE_PATHS = {
    "home": 'src/views/hello.html',
    "first": 'src/views/first.html',
    "second": 'src/views/second.html',
    "configure": 'src/views/configure.html'
}
# aiohttp route for home page
async def home(request):
    """Handles requests to the home page."""
    return web.FileResponse(FILE_PATHS["home"])

# aiohttp route for /hello
async def hello(request):
    """Handles requests to the hello page."""
    return web.FileResponse(FILE_PATHS["home"])

# aiohttp route for /first
async def first(request):
    """Handles requests to the first page."""
    return web.FileResponse(FILE_PATHS["first"])

# aiohttp route for /second
async def second(request):
    """Handles requests to the second page."""
    return web.FileResponse(FILE_PATHS["second"])

# aiohttp route for /configure
async def configure(request):
    """Handles requests to the configure page."""
    return web.FileResponse(FILE_PATHS["configure"])

# Create the Bot
bot = HelloWorldBot(CONFIG.APP_ID, CONFIG.APP_PASSWORD)

# Listen for incoming requests on /api/messages
async def messages(req: Request) -> Response:
    return await ADAPTER.process(req, bot)


APP = web.Application(middlewares=[aiohttp_error_middleware])

# Register routes
APP.router.add_get("/", home)
APP.router.add_get("/hello", hello)
APP.router.add_get("/first", first)
APP.router.add_get("/second", second)
APP.router.add_get("/configure", configure)
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
