# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import logging
import asyncio
from flask import Flask, request, jsonify, send_from_directory, Response
from botbuilder.core import TurnContext
from botbuilder.schema import Activity
from botbuilder.integration.aiohttp import (
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
)

from bots import DeepLinkTabsBot
from config import DefaultConfig

CONFIG = DefaultConfig()

app = Flask(__name__)

PORT = int(os.getenv("PORT", CONFIG.PORT))
MICROSOFT_APP_ID = os.getenv("MicrosoftAppId", "")

logging.basicConfig(level=logging.INFO)

adapter = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))
bot = DeepLinkTabsBot()

async def on_turn_error(context: TurnContext, error: Exception):
    logging.error(f"\n [on_turn_error] unhandled error: {error}")
    await context.send_activity("Sorry, it looks like something went wrong.")

adapter.on_turn_error = on_turn_error

@app.route("/api/messages", methods=["POST"])
def messages():
    body = request.json
    activity = Activity().deserialize(body)
    auth_header = request.headers.get("Authorization", "")

    async def aux_func(turn_context: TurnContext):
        await bot.on_turn(turn_context)

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        loop.run_until_complete(adapter.process_activity(auth_header, activity, aux_func))
    finally:
        loop.close()

    return Response(status=200)

@app.route("/api/getAppId", methods=["GET"])
def get_app_id():
    return jsonify({"microsoftAppId": MICROSOFT_APP_ID})

@app.route("/<filename>")
def redirect_static(filename):
    return send_from_directory("static", filename)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=PORT)