#!/usr/bin/env python3

import sys
import traceback
from aiohttp import web
from aiohttp.web import Request, Response
from botbuilder.core import (
    TurnContext,
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from config import DefaultConfig
from bots.teams_conversation_bot import TeamsConversationBot
from controllers.auth_routes import auth_callback

CONFIG = DefaultConfig()

# Adapter setup
adapter = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# Catch-all error handler
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue to run this bot, please fix the bot source code.")

adapter.on_turn_error = on_error

# Bot instance
bot = TeamsConversationBot()

# Root page
async def index(request):
    return web.Response(
        text="<html><head><title>Teams Bot + Auth0</title></head><body>"
             "<h1>Teams Bot + Auth0</h1>"
             "<p>This bot is running. You can interact with it in Microsoft Teams.</p>"
             "</body></html>",
        content_type="text/html",
    )

# Bot message endpoint
async def messages(req: Request) -> Response:
    return await adapter.process(req, bot)

APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_get("/", index)
APP.router.add_post("/api/messages", messages)
APP.router.add_get("/api/auth/callback", auth_callback)
APP.router.add_static("/src/views", "src/views")

if __name__ == "__main__":
    web.run_app(APP, host="0.0.0.0", port=CONFIG.PORT)