# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
import os
from flask import Flask, send_from_directory, jsonify

app = Flask(__name__, static_folder=None)

# Serve images
@app.route('/images/<path:filename>')
def serve_images(filename):
    return send_from_directory(os.path.join(os.path.dirname(__file__), 'images'), filename)

# Serve styles (referenced in HTML files)
@app.route('/styles/<path:filename>')
def serve_styles(filename):
    return send_from_directory(os.path.join(os.path.dirname(__file__), 'templates/static/styles'), filename)

# Route: /configure
@app.route('/configure')
def configure():
    return send_from_directory(os.path.dirname(__file__), 'templates/views/configure.html')

# Route: /conversationTab
@app.route('/conversationTab')
def conversation_tab():
    return send_from_directory(os.path.dirname(__file__), 'templates/views/conversation-tab.html')

# Catch-all route
@app.route('/', defaults={'path': ''})
@app.route('/<path:path>')
def catch_all(path):
    return jsonify({'error': 'Route not found'}), 404

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 3978))
    print(f"Bot/ME service listening at http://localhost:{port}")
    app.run(host='0.0.0.0', port=port)
