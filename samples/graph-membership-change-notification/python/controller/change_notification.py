# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from flask import Blueprint, request, abort, jsonify
import logging
import traceback
from helper.graph_helper import GraphHelper

change_notification_bp = Blueprint('change_notification', __name__)

@change_notification_bp.route('/', methods=['POST'])
def create_channel_async():
    # Handle validation request from Microsoft Graph
    validation_token = request.args.get('validationToken')
    if validation_token:
        print(f"Received validation token: {validation_token}")
        return validation_token, 200, {'Content-Type': 'text/plain'}
    
    team_id = request.args.get('teamId')
    channel_id = request.args.get('channelId')
    page_id = '1'
    
    try:
        GraphHelper.create_subscription(team_id, page_id)
        GraphHelper.create_shared_with_team_subscription(team_id, page_id, channel_id)
        return ('', 202)
    except Exception as ex:
        logging.exception(f"Error creating channel subscription for team {team_id}")
        error_response = {
            'error': str(ex),
            'trace': traceback.format_exc(),
            'team_id': team_id,
            'channel_id': channel_id
        }
        return jsonify(error_response), 500

