import os
import logging
import asyncio
from flask import Flask, request, jsonify, send_from_directory, Response
from botbuilder.core import BotFrameworkAdapter, BotFrameworkAdapterSettings, TurnContext
from botbuilder.schema import Activity
from bots import DeepLinkTabsBot

app = Flask(__name__)

# Load environment variables
PORT = int(os.getenv("PORT", 3978))
MICROSOFT_APP_ID = os.getenv("MicrosoftAppId", "")
MICROSOFT_APP_PASSWORD = os.getenv("MicrosoftAppPassword", "")

# Configure logging
logging.basicConfig(level=logging.INFO)

# Configure Bot Adapter
settings = BotFrameworkAdapterSettings(app_id=MICROSOFT_APP_ID, app_password=MICROSOFT_APP_PASSWORD)
adapter = BotFrameworkAdapter(settings)

# Bot instance
bot = DeepLinkTabsBot()

# Error handling
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
        await bot.on_turn(turn_context)  # Correct method name

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    loop.run_until_complete(adapter.process_activity(activity, auth_header, aux_func))

    return Response(status=200)  # Explicitly return an empty response with status 200

@app.route("/api/getAppId", methods=["GET"])
def get_app_id():
    return jsonify({"microsoftAppId": MICROSOFT_APP_ID})

@app.route("/<filename>")
def redirect_static(filename):
    return send_from_directory("static", filename)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=PORT)