# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from aiohttp import web
from aiohttp.web import Request, Response
from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.hosting.aiohttp import CloudAdapter
from microsoft_agents.authentication.msal import MsalConnectionManager
from microsoft_agents.hosting.aiohttp import jwt_authorization_middleware
from microsoft_agents.activity import Activity, ActivityTypes, load_configuration_from_env
from os import environ
from bots import HelloWorldBot 
from config import DefaultConfig

CONFIG = DefaultConfig()

# Set up environment variables for Agent SDK
# The Agent SDK expects specific environment variable format for service connection
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__CLIENTID", CONFIG.APP_ID)
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__CLIENTSECRET", CONFIG.APP_PASSWORD)
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__TENANTID", CONFIG.APP_TENANTID)

# Load configuration from environment using Agent SDK helper
agents_sdk_config = load_configuration_from_env(environ)

# Create connection manager for authentication
CONNECTION_MANAGER = MsalConnectionManager(**agents_sdk_config)

# Create adapter with connection manager
ADAPTER = CloudAdapter(connection_manager=CONNECTION_MANAGER)

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

# Custom middleware wrapper to apply JWT only to /api/messages
@web.middleware
async def conditional_jwt_middleware(request, handler):
    # Only apply JWT authentication to /api/messages endpoint
    if request.path.startswith('/api/messages'):
        return await jwt_authorization_middleware(request, handler)
    else:
        # For other routes (tabs, static pages), skip JWT validation
        return await handler(request)

APP = web.Application(middlewares=[conditional_jwt_middleware])

# Set the agent configuration in app state for jwt_authorization_middleware
APP["agent_configuration"] = CONNECTION_MANAGER.get_default_connection_configuration()

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
