# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
from flask import Flask, send_from_directory
import os
import ssl

app = Flask(__name__, static_folder='html')  # Changed to point to our html folder

# Add CORS headers for Teams compatibility
@app.after_request
def after_request(response):
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS')
    return response

@app.route('/')
def serve_index():
    return send_from_directory(app.static_folder, 'index.html')

@app.route('/tab')
def serve_tab():
    return send_from_directory(app.static_folder, 'tab.html')

@app.route('/privacy')
def serve_privacy():
    return send_from_directory(app.static_folder, 'privacy.html')

@app.route('/termsofuse')
def serve_terms():
    return send_from_directory(app.static_folder, 'termsofuse.html')

@app.route('/styles.css')
def serve_css():
    response = send_from_directory(app.static_folder, 'styles.css')
    response.headers['Content-Type'] = 'text/css'
    return response

# Health check endpoint
@app.route('/health')
def health_check():
    return {"status": "healthy"}, 200

# Fallback route for any other static files
@app.route('/<path:path>')
def serve_file(path):
    try:
        return send_from_directory(app.static_folder, path)
    except:
        # If file not found, redirect to index
        return send_from_directory(app.static_folder, 'index.html')

if __name__ == '__main__':
    # Load environment variables from .env.local file
    env_file = os.path.join(os.path.dirname(__file__), 'env', '.env.local')
    if os.path.exists(env_file):
        with open(env_file, 'r') as f:
            for line in f:
                if '=' in line and not line.strip().startswith('#'):
                    key, value = line.strip().split('=', 1)
                    os.environ[key] = value
    
    # Get SSL certificate paths
    ssl_cert_file = os.environ.get('SSL_CRT_FILE')
    ssl_key_file = os.environ.get('SSL_KEY_FILE')
    
    # Get port from environment or use default
    bot_endpoint = os.environ.get('BOT_ENDPOINT', 'https://localhost:3978')
    try:
        port = int(bot_endpoint.split(':')[-1])
    except:
        port = 3978
    
    print(f"Starting Flask server on port {port}")
    print(f"Bot endpoint: {bot_endpoint}")
    
    # Start with HTTPS
    if ssl_cert_file and ssl_key_file and os.path.exists(ssl_cert_file) and os.path.exists(ssl_key_file):
        print("Using provided SSL certificates")
        context = ssl.SSLContext(ssl.PROTOCOL_TLSv1_2)
        context.load_cert_chain(ssl_cert_file, ssl_key_file)
        app.run(debug=True, host='0.0.0.0', port=port, ssl_context=context)
    else:
        print("Using adhoc SSL certificates")
        # Install pyOpenSSL if not already installed: pip install pyOpenSSL
        app.run(debug=True, host='0.0.0.0', port=port, ssl_context='adhoc')
