# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
import os
from aiohttp import web
from aiohttp.web import Request, Response
from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.hosting.aiohttp import CloudAdapter
from microsoft_agents.authentication.msal import MsalConnectionManager
from microsoft_agents.activity import load_configuration_from_env
from os import environ
from bots import DeepLinkTabsBot
from config import DefaultConfig

CONFIG = DefaultConfig()

# Configure Agent SDK authentication using environment variables
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__CLIENTID", CONFIG.APP_ID)
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__CLIENTSECRET", CONFIG.APP_PASSWORD)
environ.setdefault("CONNECTIONS__SERVICE_CONNECTION__SETTINGS__TENANTID", CONFIG.APP_TENANTID)

# Load Agent SDK configuration from environment
agents_sdk_config = load_configuration_from_env(environ)

# Create MSAL connection manager for bot authentication
CONNECTION_MANAGER = MsalConnectionManager(**agents_sdk_config)

# Create CloudAdapter with Agent SDK (replaces Bot Framework CloudAdapter)
ADAPTER = CloudAdapter(connection_manager=CONNECTION_MANAGER)

# Initialize bot instance
BOT = DeepLinkTabsBot()

# Error handler for bot turn errors
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue to run this bot, please fix the bot source code.")

ADAPTER.on_turn_error = on_error

# Bot message endpoint - processes incoming Teams messages
async def messages(req: Request) -> Response:
    """Main bot message handler."""
    return await ADAPTER.process(req, BOT)

# API endpoint to retrieve Microsoft App ID
async def get_app_id(req: Request) -> Response:
    """Return the Microsoft App ID."""
    return web.json_response({"microsoftAppId": CONFIG.APP_ID})

# Create aiohttp web application (Agent SDK uses aiohttp, not Flask)
APP = web.Application()
APP.router.add_post("/api/messages", messages)
APP.router.add_get("/api/getAppId", get_app_id)

# Serve static files (HTML, CSS, JS) for Teams tabs
STATIC_DIR = os.path.join(os.path.dirname(__file__), 'static')
APP.router.add_static('/', STATIC_DIR, name='static')

if __name__ == "__main__":
    try:
        web.run_app(APP, host="0.0.0.0", port=CONFIG.PORT)
    except Exception as error:
        raise error