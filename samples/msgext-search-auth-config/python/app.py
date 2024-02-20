# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from http import HTTPStatus

from aiohttp import web
from aiohttp.web import Request, Response, json_response

from botbuilder.core import (
    BotFrameworkAdapterSettings,
    TurnContext,
    BotFrameworkAdapter,
    MemoryStorage,
    UserState,
)
from botbuilder.core.integration import aiohttp_error_middleware

from botbuilder.schema import Activity, ActivityTypes
from bots import TeamsMessagingExtensionsSearchAuthConfigBot
from config import DefaultConfig

# New import for logging
import logging

CONFIG = DefaultConfig()

# Create adapter.
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    # Log errors to console and a dedicated error log file
    logging.error(f"Unhandled error: {error}")
    traceback.print_exc()
    
    # Send a detailed error message to the user
    error_message = f"Oops! An unexpected error occurred: {str(error)}"
    await context.send_activity(error_message)

    # Send a trace activity if in the Bot Framework Emulator
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

# Create MemoryStorage and state
MEMORY = MemoryStorage()
USER_STATE = UserState(MEMORY)

# Create the Bot
BOT = TeamsMessagingExtensionsSearchAuthConfigBot(
    USER_STATE, CONFIG.CONNECTION_NAME, CONFIG.SITE_URL
)

# New feature to log user interactions
async def log_user_interaction(context: TurnContext):
    # Log user interaction details to an external service or file
    user_interaction = f"User {context.activity.from_property.id} sent message: {context.activity.text}"
    logging.info(user_interaction)

# Listen for incoming requests on /api/messages
async def messages(req: Request) -> Response:
    if "application/json" in req.headers["Content-Type"]:
        body = await req.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = req.headers["Authorization"] if "Authorization" in req.headers else ""

    # Add logging of user interaction
    await log_user_interaction(TurnContext(ADAPTER, activity))

    invoke_response = await ADAPTER.process_activity(
        activity, auth_header, BOT.on_turn
    )
    if invoke_response:
        return json_response(
            data=invoke_response.body, status=invoke_response.status
        )
    return Response(status=HTTPStatus.OK)

APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)
APP.router.add_static("/", path="./wwwroot/", name="static")

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
