# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
import os
import subprocess
import sys
from flask import Flask, send_from_directory, jsonify

# === Step 1: Run esbuild to bundle JavaScript ===
def bundle_js():
    try:
        result = subprocess.run(
            [
                'npx', 'esbuild',
                'server/index.js',
                '--bundle',
                '--platform=node',
                '--outfile=dist/index.js'
            ],
            check=True,
            capture_output=True,
            text=True
        )
        print("Build succeeded.")
    except subprocess.CalledProcessError as e:
        print("Error building:", e.stderr.strip())
        sys.exit(1)

# === Step 2: Start Flask server ===
def start_server():
    app = Flask(__name__, static_folder=None)    # Serve images
    @app.route('/images/<path:filename>')
    def serve_images(filename):
        return send_from_directory(os.path.join(os.path.dirname(__file__), 'images'), filename)
    
    # Serve static files
    @app.route('/static/<path:filename>')
    def serve_static(filename):
        return send_from_directory(os.path.join(os.path.dirname(__file__), 'static'), filename)
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
        return jsonify({'error': 'Route not found'})

    port = int(os.environ.get('PORT', 3978))
    print(f"Bot/ME service listening at http://localhost:{port}")
    app.run(host='0.0.0.0', port=port)

# === Main ===
if __name__ == '__main__':
    # Skip bundling for now since server/index.js doesn't exist
    # bundle_js()
    start_server()
