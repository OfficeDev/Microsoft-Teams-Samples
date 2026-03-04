#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from flask import Blueprint, request, jsonify
from utils.graph_client import GraphClient
from utils.token_exchange import exchange_sso_token_for_graph_token

chat_api = Blueprint('chat_api', __name__)
delegated_token = ""

# Retrieves pinned and recent messages from a Teams chat using Graph API.
@chat_api.route('/getGraphAccessToken', methods=['GET'])
def get_graph_access_token():
    global delegated_token
    sso_token = request.args.get('ssoToken')
    chat_id = request.args.get('chatId')

    if not sso_token or not chat_id:
        return jsonify({'error': 'Missing ssoToken or chatId'}), 400

    try:
        access_token = exchange_sso_token_for_graph_token(sso_token)
        if not access_token:
            return jsonify({'error': 'Failed to exchange token'}), 401
        delegated_token = access_token
        graph_client = GraphClient(access_token)
        try:
            pinned_messages = graph_client.get_pinned_messages(chat_id)
        except Exception as e:
            pinned_messages = {"value": []}
        recent_messages = graph_client.get_recent_messages(chat_id)
        message_list = []
        for message in recent_messages.get("value", []):
            message_id = message.get('id', 'NO_ID')
            message_type = message.get('messageType', 'unknown')
            if message_type in ["message", "unknownFutureValue"] or message_type is None:
                body = message.get("body", {})
                content = body.get("content", "") if body else ""
                if content:
                    import re
                    clean_content = re.sub(r'<[^>]+>', '', content)
                    if clean_content.strip():
                        message_details = {
                            "id": message_id,
                            "value": clean_content.strip()
                        }
                        message_list.append(message_details)

        pinned_values = pinned_messages.get('value', [])
        if pinned_values and len(pinned_values) > 0:
            pinned_msg = pinned_values[0]
            pinned_content = pinned_msg.get('message', {}).get('body', {}).get('content', '')
            import re
            clean_pinned_content = re.sub(r'<[^>]+>', '', pinned_content)
            response_data = {
                "id": pinned_msg.get('id', ''),
                "message": clean_pinned_content,
                "messages": message_list
            }
        else:
            response_data = {
                "id": '',
                "message": '',
                "messages": message_list
            }

        return jsonify(response_data)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

# Pins a specific message in a Teams chat.
@chat_api.route('/pinMessage', methods=['GET'])
def pin_new_message():
    sso_token = request.args.get('ssoToken')
    chat_id = request.args.get('chatId')
    message_id = request.args.get('messageId')

    if not sso_token:
        return jsonify({'error': 'Missing ssoToken'}), 400
    if not chat_id:
        return jsonify({'error': 'Missing chatId'}), 400
    if not message_id:
        return jsonify({'error': 'Missing messageId. Please select a message to pin.'}), 400

    try:
        access_token = exchange_sso_token_for_graph_token(sso_token)
        if not access_token:
            return jsonify({'error': 'Failed to exchange token'}), 401
        graph_client = GraphClient(access_token)
        result = graph_client.pin_message(chat_id, message_id)
        return '', 204
    except Exception as e:
        return jsonify({'error': str(e)}), 500

# Unpins a message from a Teams chat.
@chat_api.route('/unpinMessage', methods=['GET'])
def unpin_message():
    sso_token = request.args.get('ssoToken')
    chat_id = request.args.get('chatId')
    pinned_message_id = request.args.get('pinnedMessageId')

    if not sso_token:
        return jsonify({'error': 'Missing ssoToken'}), 400

    try:
        access_token = exchange_sso_token_for_graph_token(sso_token)
        if not access_token:
            return jsonify({'error': 'Failed to exchange token'}), 401
        graph_client = GraphClient(access_token)
        result = graph_client.unpin_message(chat_id, pinned_message_id)
        return '', 204
    except Exception as e:
        return jsonify({'error': str(e)}), 500
