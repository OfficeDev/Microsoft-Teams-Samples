from flask import Flask, render_template, request, send_from_directory
import os

# Simple Tab class inline to avoid import issues
class Tab:
    def __init__(self):
        self.colors = {
            'gray': '',
            'red': ' '
        }
    
    def get_color(self, color):
        """Get color message based on selection"""
        return self.colors.get(color.lower(), f'Color {color} not found')

app = Flask(__name__)

# Configure static files to serve from Images folder
@app.route('/images/<path:filename>')
def serve_images(filename):
    return send_from_directory('Images', filename)

@app.route('/tab')
def tab_config():
    """Route for tab configuration page"""
    return render_template('tab.html')

@app.route('/gray')
def gray_tab():
    """Route for gray tab selection"""
    tab = Tab()
    message = tab.get_color('gray')
    return render_template('gray.html', message=message)

@app.route('/red')
def red_tab():
    """Route for red tab selection"""
    tab = Tab()
    message = tab.get_color('red')
    return render_template('red.html', message=message)

@app.route('/privacy')
def privacy():
    """Route for privacy policy page"""
    return render_template('privacy.html')

@app.route('/tou')
def terms_of_use():
    """Route for terms of use page"""
    return render_template('tou.html')

@app.route('/termsofuse')
def termsofuse():
    """Route for terms of use page (manifest compatibility)"""
    return render_template('tou.html')

@app.route('/health')
def health_check():
    """Health check endpoint"""
    return "Flask app is running successfully on port 3978!"

if __name__ == '__main__':
    print("Starting Flask app on port 3978...")
    print("Available routes:")
    print("  /tab - Tab configuration")
    print("  /gray - Gray tab")
    print("  /red - Red tab")
    print("  /privacy - Privacy policy")
    print("  /tou - Terms of use")
    print("  /termsofuse - Terms of use (manifest)")
    print("  /images/<filename> - Static images")
    
    # SSL certificate paths - customize these paths for your setup
    ssl_cert = os.getenv('SSL_CRT_FILE', 'Path of ssl certificate')
    ssl_key = os.getenv('SSL_KEY_FILE', 'Path of ssl key')
    
    # Check if SSL certificates exist
    if os.path.exists(ssl_cert) and os.path.exists(ssl_key):
        print("Running with SSL certificates")
        print("App will be available at: https://localhost:3978")
        
        # Run with SSL context
        context = (ssl_cert, ssl_key)
        try:
            app.run(debug=False, host='0.0.0.0', port=3978, ssl_context=context, threaded=True)
        except Exception as e:
            print(f"SSL failed: {e}, falling back to HTTP")
            app.run(debug=False, host='0.0.0.0', port=3978)
    else:
        print("SSL certificates not found, running without HTTPS")
        print("App will be available at: http://localhost:3978")
        app.run(debug=False, host='0.0.0.0', port=3978)
