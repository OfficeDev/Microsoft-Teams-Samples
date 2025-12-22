# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from aiohttp import web
from aiohttp.web import Request, Response
from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.hosting.aiohttp import CloudAdapter
from microsoft_agents.authentication.msal import MsalConnectionManager
from microsoft_agents.activity import load_configuration_from_env
from os import environ
from bots import LinkUnfurlingBot
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

ADAPTER.on_turn_error = on_error

# Create the Bot
BOT = LinkUnfurlingBot()


# Listen for incoming requests on /api/messages
async def messages(req: Request) -> Response:
    # Main bot message handler.
    return await ADAPTER.process(req, BOT)


APP = web.Application()
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
