# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
from flask import Flask, render_template, send_from_directory, jsonify, request
import os

# Get the absolute path to the current directory
current_dir = os.path.dirname(os.path.abspath(__file__))
templates_dir = os.path.join(current_dir, 'templates')
static_dir = os.path.join(current_dir, 'static')

app = Flask(__name__, 
           template_folder=templates_dir,
           static_folder=static_dir)

# Configuration data
app_config = {
    'entityID': 'MeetingCallAudioToggleTab',
    'suggestedTabName': 'Meeting Audio Toggle',
    'description': 'Toggle audio state in Teams meetings'
}

# Route to serve static files (CSS, JS, etc.)
@app.route('/static/<path:filename>')
def serve_static(filename):
    return send_from_directory(app.static_folder, filename)

# API route to get configuration data
@app.route('/api/config')
def get_config():
    return jsonify(app_config)

# API route to save configuration
@app.route('/api/config', methods=['POST'])
def save_config():
    global app_config
    data = request.get_json()
    if data:
        app_config.update(data)
        return jsonify({'status': 'success', 'message': 'Configuration saved'})
    return jsonify({'status': 'error', 'message': 'No data provided'})

# Route for /configure
@app.route('/configure')
def configure():
    return render_template('configure.html', config=app_config)

# Route for /ToggleAudioCall
@app.route('/ToggleAudioCall')
def toggle_audio_call():
    return render_template('ToggleAudioCall.html', config=app_config)

# Main entry point
if __name__ == '__main__':
    # Run with HTTP for dev tunnel compatibility
    app.run(debug=True, host='0.0.0.0', port=3978)
