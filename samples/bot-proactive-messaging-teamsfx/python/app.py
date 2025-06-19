import os
import sys
import traceback
from aiohttp import web
from dotenv import load_dotenv

from botbuilder.core import TurnContext, MemoryStorage
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware

from bots.teams_Bot import TeamsBot  # Your bot class
from config import DefaultConfig  # Your config loader module

# Load environment variables
load_dotenv(dotenv_path=os.path.join(os.path.dirname(__file__), "env/.env.local"))

# Bot configuration
CONFIG = DefaultConfig()
conversation_references = {}

# Create adapter
adapter = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# Catch-all for errors
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue to run this bot, please fix the bot source code.")

adapter.on_turn_error = on_error

# Create bot instance
bot = TeamsBot(conversation_references)

# POST /api/messages — receives all messages from user
async def messages(req: web.Request) -> web.Response:
    return await adapter.process(req, lambda context: bot.run(context))

# GET /api/notify — sends proactive messages
async def notify(req: web.Request) -> web.Response:
    print(f"🔔 Sending proactive messages to: {conversation_references}")
    for reference in conversation_references.values():
        await adapter.continue_conversation(reference, lambda ctx: ctx.send_activity("proactive hello"), CONFIG.BOT_ID)

    return web.Response(
        text="<html><body><h1>Proactive messages have been sent.</h1></body></html>",
        content_type="text/html"
    )

# Create the aiohttp app and register routes
app = web.Application(middlewares=[aiohttp_error_middleware])
app.router.add_post("/api/messages", messages)
app.router.add_get("/api/notify", notify)

# Start the web server
if __name__ == "__main__":
    try:
        web.run_app(app, host="localhost", port=CONFIG.PORT)
    except Exception as e:
        raise e
