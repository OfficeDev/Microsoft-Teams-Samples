# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
import sys
import traceback
from datetime import datetime
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import BotFrameworkAdapterSettings, BotFrameworkAdapter
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import TeamsFileUploadBot
from config import DefaultConfig

# Load configuration once
CONFIG = DefaultConfig()

# Create adapter with settings
# See https://aka.ms/about-bot-adapter to learn more about how bots work.
SETTINGS = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
ADAPTER = BotFrameworkAdapter(SETTINGS)

# Catch-all for errors
async def on_error(context, error):
    # Log error and stack trace
    # NOTE: In production environment, you should consider logging this to Azure
    #       application insights.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Notify user and provide a trace activity for Emulator
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("Please fix the bot source code to continue.")
    
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

# Create the Bot
BOT = TeamsFileUploadBot()

# Main message handler
async def messages(req: Request) -> Response:
    if req.content_type != "application/json":
        return Response(status=415)  # HTTP 415 Unsupported Media Type

    # Parse the incoming request body
    body = await req.json()
    activity = Activity().deserialize(body)
    auth_header = req.headers.get("Authorization", "")

    # Process the activity
    invoke_response = await ADAPTER.process_activity(activity, auth_header, BOT.on_turn)
    
    if invoke_response and invoke_response.body:
        return json_response(data=invoke_response.body, status=invoke_response.status)
    
    return Response(status=invoke_response.status if invoke_response else 200)  # Return status OK if no response body

# Initialize web application with error middleware
APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    try:
        web.run_app(APP, host="localhost", port=CONFIG.PORT)
    except Exception as error:
        print(f"Error running the app: {error}", file=sys.stderr)
        raise error
