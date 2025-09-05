"""
Teams Meeting Details Tab - Python Backend
Flask application providing REST API and static file serving
"""

import os
import sys
from pathlib import Path
from flask import Flask, send_from_directory, send_file, request, jsonify
from flask_cors import CORS
from dotenv import load_dotenv

# Add project root to Python path
project_root = Path(__file__).parent
server_path = project_root / 'server'
sys.path.insert(0, str(server_path))

# Load environment variables
ENV_FILE = project_root / '.env'
load_dotenv(dotenv_path=ENV_FILE)

# Import API routes
from api.routes import api_bp

def is_internal_request():
    """Check if request is coming from Teams or internal sources"""
    user_agent = request.headers.get('User-Agent', '').lower()
    referrer = request.headers.get('Referer', '').lower()
    
    # Allow Teams clients, localhost, and dev tunnels
    internal_indicators = ['teams', 'microsoft', 'msteams', 'localhost', 'devtunnels.ms']
    
    # Check if request is from Teams or development environment
    is_teams = any(indicator in user_agent for indicator in internal_indicators)
    is_internal_referrer = any(indicator in referrer for indicator in internal_indicators)
    is_localhost = 'localhost' in request.host or '127.0.0.1' in request.host
    
    return is_teams or is_internal_referrer or is_localhost

def create_app():
    """Create and configure Flask app"""
    # Configure static folder directly in Flask constructor
    client_build_path = project_root / 'client' / 'build'
    app = Flask(__name__, 
                static_folder=str(client_build_path / 'static'),
                static_url_path='/static')
    
    # Enable CORS (matches Node.js cors middleware)
    CORS(app)
    
    # Configure JSON handling (matches express.json())
    app.config['JSON_SORT_KEYS'] = False
    
    
    # Register API routes (matches server.use('/api', require('./api')))
    app.register_blueprint(api_bp, url_prefix='/api')
    
    # Tab configuration endpoint (matches Teams manifest configurationUrl)
    @app.route('/configuretab')
    def configure_tab_route():
        """Handle tab configuration - serves the config page"""
        index_path = client_build_path / 'index.html'
        if index_path.exists():
            return send_file(index_path)
        else:
            return "Client build not found. Please build the frontend first.", 404

    # Catch-all route to serve index.html (matches server.get('*'))
    @app.route('/', defaults={'path': ''})
    @app.route('/<path:path>')
    def serve_spa(path):
        """Serve React SPA - return index.html for all routes"""
        # Restrict access to detail page for external users
        if path == 'detail' and not is_internal_request():
            return jsonify({
                'error': 'Access denied', 
                'message': 'This page is only available within Microsoft Teams'
            }), 403
        
        # Skip certain paths that should not serve index.html
        if path.startswith('static/') or path.startswith('api/') or path.startswith('debug/'):
            return "Not found", 404
            
        index_path = client_build_path / 'index.html'
        if index_path.exists():
            return send_file(index_path)
        else:
            return "Client build not found. Please build the frontend first.", 404
    
    return app

def main():
    """Main entry point - matches Node.js server startup"""
    app = create_app()
    
    # Get port from environment (matches Node.js port detection)
    port = int(os.getenv('port', os.getenv('PORT', 3978)))
    
    print(f"Server listening on http://localhost:{port}")
    
    # Start server (matches Node.js server.listen())
    app.run(
        host='0.0.0.0',
        port=port,
        debug=True,
        threaded=True,
        use_reloader=True
    )

if __name__ == '__main__':
    main()