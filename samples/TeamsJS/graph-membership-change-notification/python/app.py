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
channel_members_list = {}  # Store member lists by teamId-channelId key 

# Mount changeNotification controller (to be implemented)
app.register_blueprint(change_notification_bp, url_prefix='/api/changeNotification')

@app.route('/api/members/<team_id>/<channel_id>', methods=['GET'])
def get_channel_members(team_id, channel_id):
    """Get channel members list"""
    try:
        member_key = f"{team_id}-{channel_id}"
        
        # Get fresh member list from Graph API
        members = GraphHelper.get_channel_members(team_id, channel_id)
        
        # Update local cache
        channel_members_list[member_key] = members
        
        return jsonify({
            'teamId': team_id,
            'channelId': channel_id,
            'members': members,
            'timestamp': datetime.now().isoformat()
        })
    except Exception as error:
        print(f'Error getting channel members: {error}')
        return jsonify({
            'error': 'Failed to get channel members',
            'message': str(error)
        }), 500

@app.route('/api/members/cached/<team_id>/<channel_id>', methods=['GET'])
def get_cached_channel_members(team_id, channel_id):
    """Get cached member list"""
    member_key = f"{team_id}-{channel_id}"
    
    cached_members = channel_members_list.get(member_key, [])
    
    return jsonify({
        'teamId': team_id,
        'channelId': channel_id,
        'members': cached_members,
        'cached': True,
        'timestamp': datetime.now().isoformat()
    })

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
    should_update_member_list = True
    
    # Extract team and channel IDs from the notification
    if body.get('value') and body['value'][0].get('resourceData', {}).get('@odata.id'):
        odata_id = body['value'][0]['resourceData']['@odata.id']
        team_match = re.search(r"teams\('([^']+)'\)", odata_id)
        channel_match = re.search(r"channels\('([^']+)'\)", odata_id)
        
        team_id = team_match.group(1) if team_match else None
        channel_id = channel_match.group(1) if channel_match else None

    # Extract notification for getting additional data
    notification = body['value'][0] if body.get('value') else {}
    
    current_time = datetime.now().isoformat()
    
    notification_data = {
        'createdDate': current_time,
        'displayName': decrypted_data.get('displayName'),
        'changeType': decrypted_data.get('changeType'),
        'teamId': team_id,
        'channelId': channel_id
    }

    # Handle different change types with conditional member list updates
    if decrypted_data.get('changeType') == 'deleted':
        # Extract notification details from the body
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
        
        if team_id and channel_id and user_id and tenant_id:
            has_access = GraphHelper.check_user_channel_access(
                team_id, channel_id, user_id, tenant_id
            )
            notification_data['hasUserAccess'] = has_access

            # Skip member list update if user still has access
            should_update_member_list = has_access

            if has_access:
                print(f"Skipping member list update for user {user_id} - user still has access")
            else:
                print(f"User {user_id} no longer has access - updating member list")
    
    # Handle shared/unshared events
    if decrypted_data.get('changeType') == 'created' and decrypted_data.get('displayName'):
        # Shared event - update member list
        should_update_member_list = True
        print(f"Channel shared with team {decrypted_data.get('displayName')} - updating member list")
    
    if (decrypted_data.get('changeType') == 'deleted' and 
        decrypted_data.get('displayName') and 
        has_access is not True):
        # Unshared event - update member list
        should_update_member_list = True
        print(f"Channel unshared from team {decrypted_data.get('displayName')} - updating member list")
    
    # Update member list conditionally
    if should_update_member_list and team_id and channel_id:
        try:
            member_key = f"{team_id}-{channel_id}"
            updated_members = GraphHelper.get_channel_members(team_id, channel_id)
            channel_members_list[member_key] = updated_members
            
            notification_data['memberListUpdated'] = True
            notification_data['currentMemberCount'] = len(updated_members)
            
            print(f"Member list updated for {member_key}. Current count: {len(updated_members)}")
        except Exception as error:
            print(f'Error updating member list: {error}')
            notification_data['memberListUpdateError'] = str(error)
    else:
        notification_data['memberListUpdated'] = False
        if (decrypted_data.get('changeType') == 'deleted' and 
            notification_data.get('hasUserAccess')):
            notification_data['memberListSkipReason'] = "User still has access"

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