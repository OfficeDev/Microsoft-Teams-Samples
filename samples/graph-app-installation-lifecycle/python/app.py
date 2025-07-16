from flask import Flask, render_template, request, jsonify
from flask_cors import CORS
from dotenv import load_dotenv
from datetime import datetime
import os
import auth  # This should be a Python module you create to handle authentication
import pathlib
import logging
from datetime import datetime

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize Flask app
app = Flask(__name__, template_folder='views', static_folder='Styles')

# Enable CORS for all routes (required for Teams apps)
CORS(app, resources={
    r"/*": {
        "origins": ["*"],
        "methods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
        "allow_headers": ["Content-Type", "Authorization", "X-Requested-With"]
    }
})

# Add security headers for Teams integration
@app.after_request
def after_request(response):
    response.headers['X-Frame-Options'] = 'ALLOWALL'
    response.headers['X-Content-Type-Options'] = 'nosniff'
    response.headers['Content-Security-Policy'] = "frame-ancestors *"
    return response

# Load environment variables from multiple sources
# 1. Load from root .env file
root_env_path = pathlib.Path('.env')
if root_env_path.exists():
    load_dotenv(dotenv_path=root_env_path)
    logger.info("Loaded root .env file")

# 2. Load from env/.env.local
env_path = pathlib.Path('env/.env.local')
if env_path.exists():
    load_dotenv(dotenv_path=env_path)
    logger.info("Loaded env/.env.local file")

# 3. Load from env/.env.local.user (contains secrets)
env_user_path = pathlib.Path('env/.env.local.user')
if env_user_path.exists():
    load_dotenv(dotenv_path=env_user_path)
    logger.info("Loaded env/.env.local.user file")

# Log loaded environment variables (without secrets)
logger.info(f"AAD_APP_CLIENT_ID: {os.getenv('AAD_APP_CLIENT_ID')}")
logger.info(f"AAD_APP_TENANT_ID: {os.getenv('AAD_APP_TENANT_ID')}")
logger.info(f"TEAMS_APP_ID: {os.getenv('TEAMS_APP_ID')}")
logger.info(f"TAB_DOMAIN: {os.getenv('TAB_DOMAIN')}")
logger.info(f"TAB_ENDPOINT: {os.getenv('TAB_ENDPOINT')}")

# Validate required environment variables
required_vars = [
    'AAD_APP_CLIENT_ID',
    'SECRET_AAD_APP_CLIENT_SECRET',
    'AAD_APP_TENANT_ID'
]

missing_vars = [var for var in required_vars if not os.getenv(var)]
if missing_vars:
    logger.error(f"Missing required environment variables: {missing_vars}")
    raise ValueError(f"Missing required environment variables: {missing_vars}")

logger.info("All required environment variables are loaded")

def validate_configuration():
    """
    Validate that all required configuration is present
    """
    issues = []
    
    # Check client ID
    client_id = os.getenv('AAD_APP_CLIENT_ID') or os.getenv('MicrosoftAppId')
    if not client_id:
        issues.append("Missing Client ID: Set AAD_APP_CLIENT_ID or MicrosoftAppId")
    
    # Check client secret
    client_secret = os.getenv('SECRET_AAD_APP_CLIENT_SECRET') or os.getenv('MicrosoftAppPassword')
    if not client_secret:
        issues.append("Missing Client Secret: Set SECRET_AAD_APP_CLIENT_SECRET or MicrosoftAppPassword")
    elif client_secret.startswith('crypto_'):
        issues.append("Client secret appears to be encrypted. Use plain text secret.")
    
    # Check tenant ID
    tenant_id = os.getenv('AAD_APP_TENANT_ID')
    if not tenant_id:
        issues.append("Missing Tenant ID: Set AAD_APP_TENANT_ID")
    
    # Check Teams App ID
    teams_app_id = os.getenv('TEAMS_APP_ID') or os.getenv('TeamsAppId')
    if not teams_app_id:
        issues.append("Missing Teams App ID: Set TEAMS_APP_ID or TeamsAppId")
    
    return issues

# Default route
@app.route('/')
def index():
    return render_template('index.html')

# Route for configuration page
@app.route('/configure')
def configure():
    logger.info("Configure page accessed")
    try:
        return render_template('configure.html')
    except Exception as e:
        logger.error(f"Error rendering configure page: {e}")
        return render_template('error.html', error=str(e)), 500

# Direct access to configure page (for testing outside Teams)
@app.route('/configure-direct')
def configure_direct():
    """Direct access to configure page for testing"""
    logger.info("Direct configure page accessed")
    try:
        # Create a simplified version for direct testing
        test_html = """
        <!DOCTYPE html>
        <html>
        <head>
            <title>Configure Page - Direct Access</title>
            <style>
                body { font-family: Arial, sans-serif; padding: 20px; }
                .container { max-width: 600px; margin: 0 auto; }
                .status { padding: 10px; margin: 10px 0; border-radius: 4px; }
                .success { background-color: #dff6dd; color: #107c10; }
                .error { background-color: #fed9cc; color: #d83b01; }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>Configure Page - Direct Access</h1>
                <p>This is a direct access version of the configure page for testing purposes.</p>
                
                <div class="status">
                    <strong>Status:</strong> Configure page is accessible
                </div>
                
                <p><strong>Next steps:</strong></p>
                <ol>
                    <li>Check if the full configure page works: <a href="/configure">Go to Configure</a></li>
                    <li>Test app installation page: <a href="/installedAppsList">View Apps</a></li>
                    <li>Check configuration status: <a href="/config-status-page">Config Status</a></li>
                </ol>
                
                <p><a href="/">← Back to Home</a></p>
            </div>
        </body>
        </html>
        """
        return test_html
    except Exception as e:
        logger.error(f"Error in configure-direct: {e}")
        return f"Error: {str(e)}", 500

# Test route to check if configure page is accessible
@app.route('/test-configure')
def test_configure():
    try:
        # Test if we can access the configure template
        from flask import render_template_string
        test_html = """
        <!DOCTYPE html>
        <html>
        <head><title>Configure Test</title></head>
        <body>
            <h1>Configure Page Test</h1>
            <p>If you can see this, the configure route is working.</p>
            <p><a href="/configure">Go to actual configure page</a></p>
            <p><a href="/">Back to home</a></p>
        </body>
        </html>
        """
        return render_template_string(test_html)
    except Exception as e:
        return f"Error: {str(e)}", 500

# Route to display installed apps
@app.route('/installedAppsList')
def installed_apps_list():
    tenant_id = request.args.get('tenantId') or request.args.get('tid') or os.getenv('AAD_APP_TENANT_ID')
    
    logger.info(f"Received request for installed apps list. TenantId: {tenant_id}")
    logger.info(f"Request args: {dict(request.args)}")
    
    if not tenant_id:
        logger.error("Tenant ID not found in request or environment")
        return render_template('error.html', error="Tenant ID not found"), 400
    
    try:
        token = auth.get_access_token(tenant_id)
        logger.info("Successfully retrieved access token")
        return render_template('installedAppList.html', token=token)
    except Exception as e:
        logger.error(f"Error getting token: {e}")
        return render_template('error.html', error=str(e)), 500

# API route to get access token
@app.route('/api/token')
def get_token():
    tenant_id = request.args.get('tenantId') or os.getenv('AAD_APP_TENANT_ID')
    if not tenant_id:
        return jsonify({'error': 'Tenant ID not found'}), 400
    
    try:
        token = auth.get_access_token(tenant_id)
        return jsonify({'access_token': token})
    except Exception as e:
        logger.error(f"Error getting token via API: {e}")
        return jsonify({'error': str(e)}), 500

# Health check route
@app.route('/health')
def health_check():
    return jsonify({'status': 'healthy', 'service': 'graph-app-installation-lifecycle'})

# Debug route to check configuration
@app.route('/debug')
def debug_config():
    config_info = {
        'tenant_id': os.getenv('AAD_APP_TENANT_ID'),
        'client_id_sources': {
            'AAD_APP_CLIENT_ID': os.getenv('AAD_APP_CLIENT_ID'),
            'MicrosoftAppId': os.getenv('MicrosoftAppId'),
            'CLIENT_ID': os.getenv('CLIENT_ID')
        },
        'client_secret_exists': {
            'SECRET_AAD_APP_CLIENT_SECRET': bool(os.getenv('SECRET_AAD_APP_CLIENT_SECRET')),
            'MicrosoftAppPassword': bool(os.getenv('MicrosoftAppPassword')),
            'CLIENT_SECRET': bool(os.getenv('CLIENT_SECRET'))
        },
        'tab_domain': os.getenv('TAB_DOMAIN'),
        'tab_endpoint': os.getenv('TAB_ENDPOINT'),
        'teams_app_id': os.getenv('TEAMS_APP_ID') or os.getenv('TeamsAppId'),
        'environment_files_loaded': {
            'root_env': pathlib.Path('.env').exists(),
            'local_env': pathlib.Path('env/.env.local').exists(),
            'user_env': pathlib.Path('env/.env.local.user').exists()
        }
    }
    return jsonify(config_info)

# Route to test authentication
@app.route('/test-auth')
def test_auth():
    tenant_id = request.args.get('tenantId') or os.getenv('AAD_APP_TENANT_ID')
    if not tenant_id:
        return jsonify({'error': 'Tenant ID not found'}), 400
    
    try:
        token = auth.get_access_token(tenant_id)
        return jsonify({'status': 'success', 'token_length': len(token)})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

# Configuration validation route
@app.route('/config-status')
def config_status():
    issues = validate_configuration()
    status = {
        'status': 'healthy' if not issues else 'issues_found',
        'issues': issues,
        'configuration': {
            'client_id_found': bool(os.getenv('AAD_APP_CLIENT_ID') or os.getenv('MicrosoftAppId')),
            'client_secret_found': bool(os.getenv('SECRET_AAD_APP_CLIENT_SECRET') or os.getenv('MicrosoftAppPassword')),
            'tenant_id_found': bool(os.getenv('AAD_APP_TENANT_ID')),
            'teams_app_id_found': bool(os.getenv('TEAMS_APP_ID') or os.getenv('TeamsAppId')),
            'tab_domain': os.getenv('TAB_DOMAIN'),
            'tab_endpoint': os.getenv('TAB_ENDPOINT')
        }
    }
    return jsonify(status)

# Route for configuration status page
@app.route('/config-status-page')
def config_status_page():
    return render_template('config-status.html')

# Simple test endpoint for connectivity
@app.route('/test')
def test():
    return "OK", 200

# Test endpoint with CORS headers
@app.route('/test-cors')
def test_cors():
    response = jsonify({'message': 'CORS test successful', 'timestamp': str(datetime.utcnow())})
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS')
    return response

# Debug endpoint to test Graph API call
@app.route('/debug-graph/<team_id>')
def debug_graph(team_id):
    """Debug endpoint to test Graph API call for installed apps"""
    tenant_id = request.args.get('tenantId') or os.getenv('AAD_APP_TENANT_ID')
    
    if not tenant_id:
        return jsonify({'error': 'Tenant ID required'}), 400
    
    try:
        import requests
        token = auth.get_access_token(tenant_id)
        
        # Test the Graph API call
        url = f"https://graph.microsoft.com/v1.0/teams/{team_id}/installedApps?$expand=teamsAppDefinition,teamsApp"
        headers = {
            'Authorization': f'Bearer {token}',
            'Content-Type': 'application/json'
        }
        
        response = requests.get(url, headers=headers)
        
        debug_info = {
            'token_length': len(token),
            'token_prefix': token[:50] + '...' if len(token) > 50 else token,
            'graph_url': url,
            'response_status': response.status_code,
            'response_headers': dict(response.headers),
            'team_id': team_id,
            'tenant_id': tenant_id
        }
        
        if response.status_code == 200:
            data = response.json()
            debug_info['apps_count'] = len(data.get('value', []))
            debug_info['apps_list'] = [
                {
                    'id': app.get('id'),
                    'name': app.get('teamsAppDefinition', {}).get('displayName', 'N/A'),
                    'distribution': app.get('teamsApp', {}).get('distributionMethod', 'N/A')
                }
                for app in data.get('value', [])[:5]  # Show first 5 apps
            ]
            debug_info['success'] = True
        else:
            debug_info['error'] = response.text
            debug_info['success'] = False
            
        return jsonify(debug_info)
        
    except Exception as e:
        return jsonify({'error': str(e), 'success': False}), 500

# Test route for installed apps without Teams context
@app.route('/test-installed-apps')
def test_installed_apps():
    """Test route to verify installed apps functionality"""
    tenant_id = os.getenv('AAD_APP_TENANT_ID')
    
    try:
        token = auth.get_access_token(tenant_id)
        
        # Create a test page with token and instructions
        test_html = f"""
        <!DOCTYPE html>
        <html>
        <head>
            <title>Test Installed Apps</title>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20px; }}
                .container {{ max-width: 800px; margin: 0 auto; }}
                .token {{ background-color: #f0f0f0; padding: 10px; border-radius: 4px; word-break: break-all; }}
                .status {{ padding: 10px; margin: 10px 0; border-radius: 4px; }}
                .success {{ background-color: #dff6dd; color: #107c10; }}
                .info {{ background-color: #d4edda; color: #155724; }}
            </style>
        </head>
        <body>
            <div class="container">
                <h1>Test Installed Apps</h1>
                
                <div class="status success">
                    <strong>✅ Token Generated Successfully</strong>
                </div>
                
                <h3>Token Information:</h3>
                <div class="info">
                    <strong>Token Length:</strong> {len(token)} characters<br>
                    <strong>Token Prefix:</strong> {token[:50]}...<br>
                    <strong>Tenant ID:</strong> {tenant_id}
                </div>
                
                <h3>Test Links:</h3>
                <ul>
                    <li><a href="/installedAppsList?tenantId={tenant_id}">View Installed Apps List</a></li>
                    <li><a href="/api/token?tenantId={tenant_id}">Get Token (JSON)</a></li>
                    <li><a href="/config-status">Configuration Status</a></li>
                </ul>
                
                <h3>For Teams Testing:</h3>
                <p>When testing inside Microsoft Teams, the app will automatically get the team context and tenant ID.</p>
                <p>The installed apps list will populate with the apps installed in the specific team.</p>
                
                <div class="status info">
                    <strong>Note:</strong> This test page shows that authentication is working correctly. 
                    The actual app list will populate when accessed from within Microsoft Teams.
                </div>
            </div>
        </body>
        </html>
        """
        
        return test_html
        
    except Exception as e:
        return f"<h1>Error</h1><p>{str(e)}</p>", 500

# Run the Flask server
if __name__ == '__main__':
    # Get SSL certificate paths from environment
    ssl_cert = os.getenv('SSL_CRT_FILE')
    ssl_key = os.getenv('SSL_KEY_FILE')
    
    # Check if SSL certificates exist
    if ssl_cert and ssl_key and os.path.exists(ssl_cert) and os.path.exists(ssl_key):
        logger.info("SSL certificates found, starting HTTPS server...")
        logger.info(f"SSL Certificate: {ssl_cert}")
        logger.info(f"SSL Key: {ssl_key}")
        # Run with SSL (HTTPS)
        app.run(host='0.0.0.0', port=3978, debug=True, ssl_context=(ssl_cert, ssl_key))
    else:
        logger.info("SSL certificates not found or not configured, starting HTTP server...")
        # Run on HTTP only
        app.run(host='0.0.0.0', port=3978, debug=True)
