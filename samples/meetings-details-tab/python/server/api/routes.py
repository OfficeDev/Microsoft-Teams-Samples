from flask import Blueprint, request, jsonify
from . import home_controller
from . import bot_controller

api_bp = Blueprint('api', __name__)

# Optional v1 versioned routes
try:
    from . import v1
    api_bp.register_blueprint(v1.v1_bp, url_prefix='/v1')
except ImportError:
    pass

api_bp.add_url_rule('/messages', 'bot_messages',
                   bot_controller.handle_messages, methods=['POST'])

api_bp.add_url_rule('/sendAgenda', 'send_agenda',
                   home_controller.send_agenda, methods=['POST'])

api_bp.add_url_rule('/createPoll', 'create_poll',
                   home_controller.create_poll, methods=['POST'])

api_bp.add_url_rule('/getAgendaList', 'get_agenda_list',
                   home_controller.get_agenda_list, methods=['GET'])

api_bp.add_url_rule('/setAgendaList', 'set_agenda_list',
                   home_controller.set_agenda_list, methods=['POST'])

api_bp.add_url_rule('/configureTab', 'configure_tab',
                   home_controller.configure_tab, methods=['GET'])

api_bp.add_url_rule('/saveConfig', 'save_config',
                   home_controller.save_config, methods=['POST'])

api_bp.add_url_rule('/botStatus', 'get_bot_status',
                   home_controller.get_bot_status, methods=['GET'])

api_bp.add_url_rule('/debug', 'debug_store',
                   home_controller.debug_store, methods=['GET'])

@api_bp.route('/reset-conversation', methods=['POST'])
def reset_conversation():
    try:
        import sys
        import os
        sys.path.append(os.path.dirname(os.path.dirname(__file__)))
        from services.store import store

        real_conversation_id = '19:1dc43345017d4d319ee85cf41729ce69@thread.v2'
        real_service_url = 'https://smba.trafficmanager.net/amer/90700657-79c0-4a4b-9939-53ef19c8ef4d/'

        store.set_item('conversationId', real_conversation_id)
        store.set_item('serviceUrl', real_service_url)

        agenda = store.get_item('agendaList')

        return jsonify({
            "success": True,
            "message": "Conversation reset to Teams conversation",
            "conversation_id": real_conversation_id,
            "service_url": real_service_url,
            "agenda_count": len(agenda) if agenda else 0
        })

    except Exception as e:
        return jsonify({"error": f"Reset failed: {str(e)}"}), 500

api_bp.add_url_rule('/test-bot-direct', 'test_bot_direct',
                   bot_controller.test_bot_direct, methods=['POST'])

@api_bp.route('/test-debug', methods=['GET', 'POST'])
def test_debug():
    import json
    request_data = {
        'method': request.method,
        'url': request.url,
        'headers': dict(request.headers),
        'data': request.get_data(as_text=True) if request.method == 'POST' else None
    }

    return {
        'success': True,
        'message': 'Debug endpoint working',
        'request_info': request_data
    }

@api_bp.route('/test-bot-message', methods=['POST'])
def test_bot_message():
    try:
        data = request.get_json() or {}
        message_text = data.get('message', 'Test message from bot')

        import sys
        import os
        import asyncio
        from botframework.connector import ConnectorClient
        from botframework.connector.auth import MicrosoftAppCredentials
        from botbuilder.schema import Activity, ActivityTypes, ChannelAccount

        sys.path.append(os.path.dirname(os.path.dirname(__file__)))
        from services.store import store

        bot_id = os.getenv('AAD_APP_CLIENT_ID')
        bot_password = (
            os.getenv('MicrosoftAppPassword') or 
            os.getenv('BotPassword') or 
            os.getenv('SECRET_AAD_APP_CLIENT_SECRET')
        )

        if not bot_id or not bot_password:
            return jsonify({"error": "Bot credentials not configured"}), 500

        try:
            credentials = MicrosoftAppCredentials(bot_id, bot_password)
        except Exception as e:
            return jsonify({"error": f"Failed to create credentials: {str(e)}"}), 500

        conversation_id = store.get_item("conversationId")
        service_url = store.get_item("serviceUrl")

        if not conversation_id or not service_url:
            return jsonify({"error": "No conversation context available"}), 400

        try:
            client = ConnectorClient(credentials, base_url=service_url)
            MicrosoftAppCredentials.trust_service_url(service_url)

            message_activity = Activity(
                type=ActivityTypes.message,
                from_property=ChannelAccount(id=bot_id),
                text=message_text
            )

            result = client.conversations.send_to_conversation(conversation_id, message_activity)

            return jsonify({
                "success": True,
                "message": "Test message sent successfully",
                "result": str(result),
                "pattern": "sendAgenda_no_explicit_token"
            })

        except Exception as e:
            import traceback
            traceback.print_exc()
            return jsonify({"error": f"Failed to send message: {str(e)}"}), 500

    except Exception as e:
        return jsonify({"error": f"Test failed: {str(e)}"}), 500
