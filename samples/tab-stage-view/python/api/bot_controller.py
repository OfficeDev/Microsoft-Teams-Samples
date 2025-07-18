# api/bot_controller.py

import sys
import traceback
from http import HTTPStatus
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    BotFrameworkAdapterSettings,
    BotFrameworkAdapter,
    TurnContext,
)
from botbuilder.schema import Activity
from bots.bot_activity_handler import BotActivityHandler
import os

# Adapter settings using environment variables
adapter_settings = BotFrameworkAdapterSettings(
    app_id=os.getenv("MicrosoftAppId"),
    app_password=os.getenv("MicrosoftAppPassword")
)
adapter = BotFrameworkAdapter(adapter_settings)

# Error handler
async def on_error(context: TurnContext, error: Exception):
    print(f"\n[on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue, please fix the bot source code.")

    # Send a trace activity (for Emulator)
    await context.send_trace_activity(
        name="OnTurnError Trace",
        value=str(error),
        value_type="https://www.botframework.com/schemas/error",
        label="TurnError"
    )

adapter.on_turn_error = on_error

# Instantiate the bot
bot = BotActivityHandler()

# Main request handler for /api/messages
async def handle_messages(request: Request) -> Response:
    if "application/json" in request.headers.get("Content-Type", ""):
        body = await request.json()
    else:
        return Response(status=HTTPStatus.UNSUPPORTED_MEDIA_TYPE)

    activity = Activity().deserialize(body)
    auth_header = request.headers.get("Authorization", "")

    response = await adapter.process_activity(activity, auth_header, bot.on_turn)
    if response:
        return json_response(data=response.body, status=response.status)
    return Response(status=HTTPStatus.OK)
