# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
from http import HTTPStatus

from aiohttp import web
from botbuilder.core import (
    TurnContext
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes, InvokeResponse

from bots import TeamsConversationBot
from config import DefaultConfig

CONFIG = DefaultConfig()

# Create adapter.
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


# Catch-all for errors.
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    try:
        await context.send_activity("The bot encountered an error or bug.")
    except Exception as send_error:
        print(f"Failed to send error message: {send_error}", file=sys.stderr)

    # Log additional details
    if context.activity:
        print(f"Activity ID: {context.activity.id}")
        print(f"Activity Type: {context.activity.type}")
        print(f"Activity Text: {context.activity.text}")


ADAPTER.on_turn_error = on_error

BOT = TeamsConversationBot()


async def messages(request: web.Request) -> web.Response:
    if "application/json" in request.headers["Content-Type"]:
        body = await request.json()
    else:
        return web.Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = request.headers.get("Authorization", "")

    try:
        response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)
        if response:
            return web.json_response(data=response.body, status=response.status)
        return web.Response(status=HTTPStatus.OK)
    except Exception as e:
        print(f"Exception in messages: {e}", file=sys.stderr)
        return web.Response(status=HTTPStatus.INTERNAL_SERVER_ERROR)


APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="0.0.0.0", port=CONFIG.PORT)
    except Exception as error:
        print(f"Error starting app: {error}", file=sys.stderr)