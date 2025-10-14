# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from aiohttp import web
from aiohttp.web import Request, Response
from botbuilder.core import (
    ConversationState,
    MemoryStorage,
    TurnContext,
    UserState,
)
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import Activity, ActivityTypes

from config import DefaultConfig
from dialogs import MainDialog
from bots import AuthBot

import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Load configuration
CONFIG = DefaultConfig()

# Create MemoryStorage and state management objects
MEMORY = MemoryStorage()
USER_STATE = UserState(MEMORY)
CONVERSATION_STATE = ConversationState(MEMORY)

# Create adapter with CloudAdapter (BotFrameworkAdapter alternative)
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# NOTE: TeamsSSOTokenExchangeMiddleware is not needed when using OAuthPrompt
# OAuthPrompt handles all token exchange activities automatically

# Assign the global error handler to the adapter
async def on_error(context: TurnContext, error: Exception):
    """Global error handler."""
    logger.error(f"Unhandled error: {error}")
    traceback.print_exc()

    # Notify the user
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("Please fix the bot source code to continue.")

    # Send trace activity for Emulator
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

# Create dialog instance
DIALOG = MainDialog(CONFIG.CONNECTION_NAME)

# Create the main bot instance
BOT = AuthBot(CONVERSATION_STATE, USER_STATE, DIALOG)

# Listen for incoming requests on /api/messages
async def messages(req: Request) -> Response:
    try:
        body = await req.text()
        if body:
            import json
            activity_data = json.loads(body)
            logger.info(f"Incoming activity: {activity_data.get('type')} from {activity_data.get('from', {}).get('id')}")
            
            # Log if it's a token exchange activity
            if activity_data.get('name') == 'signin/tokenExchange':
                logger.info(f"Token exchange activity detected. Value: {activity_data.get('value', {})}")
    except Exception as e:
        logger.warning(f"Failed to parse activity for logging: {e}")

    return await ADAPTER.process(req, BOT)

# Create aiohttp app and register routes
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

# Run aiohttp web server
if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        logger.error(f"Failed to start bot: {error}")
        raise error
