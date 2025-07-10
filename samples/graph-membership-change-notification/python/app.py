# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import re
from datetime import datetime
from flask import Flask, request, jsonify, render_template
from flask_cors import CORS
from dotenv import load_dotenv

from helper.decryption_helper import DecryptionHelper
from helper.graph_helper import GraphHelper
from controller.change_notification import change_notification_bp

app = Flask(__name__)
CORS(app)

# Load environment variables from .env file
env_path = os.path.join(os.path.dirname(__file__), '.env')
load_dotenv(dotenv_path=env_path)

notification_list = []
notification_ids = set() 

# Mount changeNotification controller (to be implemented)
app.register_blueprint(change_notification_bp, url_prefix='/api/changeNotification')

@app.route('/api/notifications', methods=['GET', 'POST'])
def notifications():
    # Handle validation token first, before any other processing
    validation_token = request.args.get('validationToken')
    if validation_token:
        print(f"Returning validation token: {validation_token}")
        return validation_token, 200, {'Content-Type': 'text/plain'}

    # Handle GET requests: return stored notifications
    if request.method == 'GET':
        return jsonify(notification_list)
        
    # POST requests: process incoming notification payload
    try:
        body = request.get_json(force=True) or {}
    except Exception as e:
        print(f"Error parsing JSON: {str(e)}")
        body = {}
        
    # If no notification data, return current list
    if not body or 'value' not in body:
        print("No notification data received")
        return jsonify(notification_list)

    # Decrypt incoming notifications
    decrypted_data = DecryptionHelper.process_encrypted_notification(body['value'])

    team_id = None
    channel_id = None
    if decrypted_data.get('changeType') == 'deleted':
        resource = body['value'][0].get('resourceData', {})
        odata_id = resource.get('@odata.id', '')
        team_match = re.search(r"teams\('([^']+)'\)", odata_id)
        channel_match = re.search(r"channels\('([^']+)'\)", odata_id)
        if team_match:
            team_id = team_match.group(1)
        if channel_match:
            channel_id = channel_match.group(1)

    # Extract notification for getting additional data
    notification = body['value'][0] if body.get('value') else {}
    
    current_time = datetime.now().isoformat()
    
    notification_data = {
        'createdDate': current_time,
        'displayName': decrypted_data.get('displayName'),
        'changeType': decrypted_data.get('changeType') 
    }

    if decrypted_data.get('changeType') == 'deleted':
        # Extract notification details from the body
        notification = body['value'][0] if body.get('value') else {}
        resource_data = notification.get('resourceData', {})
        
        
        encoded_id = resource_data.get('id')
        user_id = None
        if encoded_id:
            try:
                import base64
                decoded_bytes = base64.b64decode(encoded_id)
                decoded_string = decoded_bytes.decode('utf-8')
                parts = decoded_string.split('##')
                if len(parts) >= 5:
                    user_id = parts[-1]  # Get the last part
                print(f"Full decoded string: {decoded_string}")
            except Exception as e:
                print(f"Error processing user ID: {str(e)}")
        
        tenant_id = notification.get('tenantId')
        
        print(f"Notification details:")
        print(f"Encoded ID: {encoded_id} (from resourceData.id)")
        print(f"Decoded User ID: {user_id}")
        print(f"Tenant ID: {tenant_id} (from notification)")
        print(f"Team ID: {team_id}")
        print(f"Channel ID: {channel_id}")
        
        if team_id and channel_id and user_id and tenant_id:
            has_access = GraphHelper.check_user_channel_access(
                team_id, channel_id, user_id, tenant_id
            )
            notification_data['hasUserAccess'] = has_access

    notification_list.append(notification_data)
    print("Graph API Notifications For Team and Channel")

    return jsonify(notification_list)

# Static HTML pages
@app.route('/configure')
def configure_page():
    return render_template('configure.html')

@app.route('/channel-notification')
def channel_page():
    return render_template('channel-notification.html')

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 3978))
    app.run(host='0.0.0.0', port=port, debug=True)