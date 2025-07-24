import os
from flask import Flask, render_template, request, jsonify, send_file
import requests
from config import DefaultConfig

from helpers.chatHelper import get_adaptive_card, create_group_chat  # You'll need to implement this

app = Flask(__name__, template_folder='src/views')
config = DefaultConfig()

PORT = int(os.getenv("PORT", os.getenv("FLASK_PORT", 3978)))

# Routes
@app.route("/tab")
def tab():
    return render_template("chatLifecycle.html")

@app.route("/api/getAdaptiveCard", methods=["POST"])
def adaptive_card():
    return get_adaptive_card(request)

@app.route("/api/createGroupChat", methods=["POST"])
def group_chat():
    return create_group_chat(request)

@app.route("/configure")
def configure():
    return render_template("configure.html")

@app.route("/auth/auth-start")
def auth_start():
    client_id = config.APP_ID
    return render_template("auth-start.html", clientId=client_id)

@app.route("/auth/auth-end")
def auth_end():
    client_id = config.APP_ID
    return render_template("auth-end.html", clientId=client_id)

@app.route("/auth/token", methods=["POST"])
def obo_token():
    tid = request.json.get("tid")
    token = request.json.get("token")
    scopes = ["https://graph.microsoft.com/User.Read"]
    app_id = config.APP_ID
    app_secret = config.APP_PASSWORD

    url = f"https://login.microsoftonline.com/{tid}/oauth2/v2.0/token"
    params = {
        "client_id": app_id,
        "client_secret": app_secret,
        "grant_type": "urn:ietf:params:oauth:grant-type:jwt-bearer",
        "assertion": token,
        "requested_token_use": "on_behalf_of",
        "scope": " ".join(scopes)
    }

    headers = {
        "Accept": "application/json",
        "Content-Type": "application/x-www-form-urlencoded"
    }

    try:
        response = requests.post(url, data=params, headers=headers)
        response.raise_for_status()
        return jsonify(response.json().get("access_token"))
    except requests.RequestException as err:
        return jsonify({"error": str(err)}), 400

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=PORT)
