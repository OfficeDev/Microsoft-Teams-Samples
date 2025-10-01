# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
from flask import Flask, render_template, jsonify
import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

app = Flask(__name__, template_folder='components')

@app.route('/')
def home():
    return render_template('index.html')

@app.route('/configure')
def configure():
    return render_template('configure.html')

@app.route('/app-in-meeting')
def app_in_meeting():
    return render_template('app-in-meeting.html')

@app.route('/share-to-meeting')
def share_to_meeting():
    return render_template('share-to-meeting.html')

@app.route('/shareview')
def shareview():
    return render_template('shareview.html')

@app.route('/api/participants')
def api_participants():
    return jsonify({'participants': []})

@app.route('/api/mute')
def api_mute():
    return jsonify({'success': True, 'message': 'Participant muted'})

@app.route('/api/unmute')
def api_unmute():
    return jsonify({'success': True, 'message': 'Participant unmuted'})

@app.route('/api/audio-state')
def api_audio_state():
    return jsonify({'muted': False, 'available': True})

@app.route('/api/config')
def api_config():
    return jsonify({'config': {}})

if __name__ == '__main__':
    # Always run in HTTP mode for devtunnel
    print("Starting HTTP Flask server on localhost:3978 (for devtunnel)")
    app.run(host='127.0.0.1', port=3978, debug=True)
