# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
from flask import Flask, render_template, request, jsonify, redirect, url_for
from flask_cors import CORS
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

app = Flask(__name__, template_folder='templates', static_folder='templates', static_url_path='/static')

# Enable CORS for Teams integration
CORS(app, origins=["https://teams.microsoft.com", "https://*.teams.microsoft.com", "http://localhost:*"])

@app.route("/")
def home():
    # Redirect to tab page instead of showing home page
    return redirect(url_for('tab'))

@app.route("/tab")
def tab():
    return render_template("Tab.html")

@app.route("/config")
def config():
    return render_template("TabConfig.html")

@app.route("/privacy")
def privacy():
    return render_template("Privacy.html")

@app.route("/termsofuse")
def terms_of_use():
    return render_template("TermsOfUse.html")

# API endpoints for Teams integration
@app.route("/api/health")
def health_check():
    return jsonify({"status": "healthy", "version": "1.0.0"})

@app.route("/api/config", methods=['GET', 'POST'])
def api_config():
    if request.method == 'GET':
        return jsonify({
            "suggestedDisplayName": "My Tab",
            "entityId": "Test",
            "contentUrl": f"{request.url_root}tab",
            "websiteUrl": f"{request.url_root}tab"
        })
    elif request.method == 'POST':
        config_data = request.get_json()
        return jsonify({"success": True})

if __name__ == "__main__":
    # Get SSL configuration from environment variables with generic fallback paths
    ssl_crt = os.getenv('SSL_CRT_FILE', './certs/localhost.crt')
    ssl_key = os.getenv('SSL_KEY_FILE', './certs/localhost.key')
    
    # Check if SSL certificates exist
    if os.path.exists(ssl_crt) and os.path.exists(ssl_key):
        print(f"Starting Flask app with HTTPS on port 3978")
        print(f"SSL Certificate: {ssl_crt}")
        print(f"SSL Key: {ssl_key}")
        print(f"Access the app at: https://localhost:3978")
        # Run with SSL context for Teams integration
        app.run(host='0.0.0.0', port=3978, debug=True, ssl_context=(ssl_crt, ssl_key))
    else:
        print("SSL certificates not found, running with HTTP")
        print(f"SSL certificates should be placed in './certs/' directory")
        print(f"Expected files: {ssl_crt}, {ssl_key}")
        print(f"Access the app at: http://localhost:3978")
        # Run on port 3978 for Teams integration
        app.run(host='0.0.0.0', port=3978, debug=True)
