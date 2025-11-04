# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from datetime import datetime
from aiohttp import web
import os
from aiohttp.web import Request, Response, json_response
from botbuilder.core import TurnContext
from http import HTTPStatus
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.integration.aiohttp import (
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
)
from botbuilder.schema import Activity, ActivityTypes
from server.bots import TeamsBot
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

# Create the Bot
BOT = TeamsBot()


# Listen for incoming requests on /api/messages.
async def messages(req: Request) -> Response:
    if req.content_type != "application/json":
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    body = await req.json()
    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    # Process activity
    invoke_response = await ADAPTER.process(req, BOT)

    # Handle invoke activity response properly
    if activity.type == ActivityTypes.invoke:
        if invoke_response:
            try:
                return json_response(
                    data=invoke_response.body, status=invoke_response.status
                )
            except Exception as e:
                print(">>> JSON Serialization Error <<<")
                print("Error:", str(e))
                print("InvokeResponse body:", invoke_response.body)
                return Response(
                    text="Internal Server Error: Failed to serialize response",
                    status=HTTPStatus.INTERNAL_SERVER_ERROR,
                )
        else:
            # If no invoke response is returned
            print(">>> No invoke response returned by bot <<<")
            return Response(status=HTTPStatus.NO_CONTENT)

    # For all other activity types
    return Response(status=HTTPStatus.OK)

APP = web.Application(middlewares=[aiohttp_error_middleware])

# Serve images from /images/ folder
STATIC_DIR = os.path.join(os.path.dirname(__file__), "Images")
APP.router.add_static("/Images/", STATIC_DIR, show_index=True)

APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        raise error
