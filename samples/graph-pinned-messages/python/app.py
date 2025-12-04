#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.import os

from flask import Flask, render_template, send_from_directory, request
from flask_cors import CORS
from config import DefaultConfig
from controller.pin_message_controller import chat_api

app = Flask(__name__, static_folder="static", template_folder="templates")
CORS(app)
config = DefaultConfig()
app.register_blueprint(chat_api, url_prefix='/api/chat')
MICROSOFT_APP_ID = config.APP_ID

# ---------- ROUTES ---------- #
@app.route('/')
@app.route('/dashboard')
def dashboard():
    return render_template("dashboard.html")

@app.route('/configure')
def configure():
    return render_template("configure.html")

@app.route('/auth-start')
def auth_start():
    return render_template("start.html", microsoft_app_id=MICROSOFT_APP_ID)

@app.route('/auth-end')
def auth_end():
    return render_template("end.html")

@app.route('/static/<path:filename>')
def static_files(filename):
    return send_from_directory(app.static_folder, filename)

if __name__ == '__main__':
    port = int(os.environ.get("PORT", config.PORT))
    app.run(host='0.0.0.0', port=port, debug=True)
