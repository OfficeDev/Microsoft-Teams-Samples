# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback
import json
import os
import requests
from http import HTTPStatus
from flask import Flask, request, render_template, redirect, url_for, jsonify
from aiohttp import web
from botbuilder.core import (
    MemoryStorage,
    UserState,
    TurnContext
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import Activity
from botbuilder.core.integration import aiohttp_error_middleware
from bots import RegionSelectionTab
from config import DefaultConfig

# Load configuration
CONFIG = DefaultConfig()

# Flask App Setup
app = Flask(__name__)

# Bot Adapter
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))

# Error Handling
async def on_error(context: TurnContext, error: Exception):
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()
    try:
        await context.send_activity("The bot encountered an error or bug.")
    except Exception as send_error:
        print(f"Failed to send error message: {send_error}", file=sys.stderr)

ADAPTER.on_turn_error = on_error

# Bot State
memory = MemoryStorage()
user_state = UserState(memory)
BOT = RegionSelectionTab(user_state)

# JSON Data for Regions
CONFIG_FILE = os.path.join(os.path.dirname(__file__), 'ConfigData', 'Regions.json')

def load_regions():
    with open(CONFIG_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

# **🔹 Flask Web Routes**
@app.route('/configure')
def index():
    domainlist = load_regions()
    return render_template('index.html', regionDomains=domainlist['regionDomains'])

@app.route('/welcome')
def welcome():
    selected_domain = request.args.get('selectedDomain')
    if not selected_domain:
        return redirect(url_for('index'))
    return render_template('welcome.html', selected_domain=selected_domain)

# **🔹 Proxy `/api/messages` to Aiohttp Bot Backend**
BOT_API_URL = "http://localhost:5001/api/messages"  # Change if needed

@app.route("/api/messages", methods=["POST"])
def proxy_messages():
    try:
        response = requests.post(BOT_API_URL, json=request.json, headers=request.headers)
        return (response.text, response.status_code, response.headers.items())
    except Exception as e:
        return jsonify({"error": str(e)}), 500

# **🔹 Aiohttp Bot Server**
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

# **🔹 Start Both Flask & Aiohttp**
if __name__ == "__main__":
    import threading

    def run_flask():
        app.run(host="0.0.0.0", port=3978, debug=False, use_reloader=False)

    def run_aiohttp():
        web.run_app(APP, host="0.0.0.0", port=5001)

    # Start Flask & Aiohttp in Parallel
    threading.Thread(target=run_flask, daemon=True).start()
    run_aiohttp()