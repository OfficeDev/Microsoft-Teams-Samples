from flask import Flask, render_template, jsonify
import ssl
import os

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
    import os
    import ssl
    
    # Check if we should use devtunnel (HTTP) or localhost HTTPS
    use_devtunnel = os.environ.get('USE_DEVTUNNEL', 'false').lower() == 'true'
    
    if use_devtunnel:
        print("Starting HTTP Flask server on localhost:3978 (for devtunnel)")
        app.run(host='127.0.0.1', port=3978, debug=True)
    else:
        print("Starting HTTPS Flask server on localhost:3978")
        
        # Create SSL context for localhost
        context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        
        # Check if SSL certificates exist from environment
        ssl_cert = os.environ.get('SSL_CRT_FILE')
        ssl_key = os.environ.get('SSL_KEY_FILE')
        
        if ssl_cert and ssl_key and os.path.exists(ssl_cert) and os.path.exists(ssl_key):
            context.load_cert_chain(ssl_cert, ssl_key)
            print(f"Using SSL certificates: {ssl_cert}, {ssl_key}")
        else:
            # Create adhoc SSL context for development
            context = 'adhoc'
            print("Using adhoc SSL certificates for development")
        
        app.run(host='127.0.0.1', port=3978, debug=True, ssl_context=context)
