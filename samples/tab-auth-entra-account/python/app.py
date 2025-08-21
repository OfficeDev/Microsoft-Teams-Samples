# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import json
import requests
from urllib.parse import urlencode, unquote
from flask import Flask, render_template, request, session, jsonify, redirect
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)

CLIENT_ID = os.getenv('ClientId')
CLIENT_SECRET = os.getenv('ClientSecret')
REDIRECT_URI = os.getenv('REDIRECT_URI')
PORT = int(os.getenv('PORT', 3978))

@app.route('/')
def index():
    """Render the main index page"""
    return render_template('index.html')

@app.route('/authstart')
def auth_start():
    """Render the authentication start page"""
    auth_id = request.args.get('authId')
    oauth_redirect_method = request.args.get('oauthRedirectMethod')
    host_redirect_url = request.args.get('hostRedirectUrl')
    
    return render_template('authStart.html',
                         clientId=CLIENT_ID,
                         redirectUri=REDIRECT_URI,
                         authId=auth_id,
                         oauthRedirectMethod=oauth_redirect_method,
                         hostRedirectUrl=host_redirect_url)

@app.route('/Auth/AuthEnd')
def auth_end():
    """Handle the OAuth callback and token exchange"""
    code = request.args.get('code')
    state_param = request.args.get('state', '{}')
    
    try:
        state = json.loads(unquote(state_param))
    except (json.JSONDecodeError, ValueError):
        state = {}
    
    if not code:
        return render_template('error.html', message="Authorization code not found")
    
    try:
        token_data = {
            'client_id': CLIENT_ID,
            'scope': 'openid profile email User.Read offline_access',
            'code': code,
            'redirect_uri': REDIRECT_URI,
            'grant_type': 'authorization_code',
            'client_secret': CLIENT_SECRET
        }
        
        token_response = requests.post(
            'https://login.microsoftonline.com/common/oauth2/v2.0/token',
            data=token_data,
            headers={'Content-Type': 'application/x-www-form-urlencoded'}
        )
        
        if token_response.status_code != 200:
            return render_template('error.html', message="Token exchange failed")
        
        token_json = token_response.json()
        
        session['access_token'] = token_json.get('access_token')
        session['id_token'] = token_json.get('id_token')
        
        return render_template('authEnd.html',
                             hostRedirectUrl=state.get('hostRedirectUrl'),
                             idToken=token_json.get('id_token'),
                             accessToken=token_json.get('access_token'))
        
    except requests.RequestException as e:
        return render_template('error.html', message="Token exchange failed")
    except Exception as e:
        return render_template('error.html', message="An unexpected error occurred")

@app.route('/getAuthAccessToken', methods=['POST'])
def get_auth_access_token():
    """Get user profile using the stored access token"""
    id_token = request.json.get('idToken') if request.is_json else request.form.get('idToken')
    
    access_token = session.get('access_token')
    if not access_token:
        return jsonify({"error": "No access token available"}), 401
    
    try:
        headers = {
            'Authorization': f'Bearer {access_token}'
        }
        
        profile_response = requests.get(
            'https://graph.microsoft.com/v1.0/me',
            headers=headers
        )
        
        if profile_response.status_code != 200:
            return jsonify({"error": "Failed to fetch profile"}), 500
        
        return jsonify(profile_response.json())
        
    except requests.RequestException as e:
        return jsonify({"error": "Failed to fetch profile"}), 500

@app.route('/error')
def error():
    """Render the error page"""
    return render_template('error.html', message="An error occurred")

if __name__ == '__main__':
    print(f"Server is running on http://localhost:{PORT}")
    app.run(host='0.0.0.0', port=PORT, debug=True)
