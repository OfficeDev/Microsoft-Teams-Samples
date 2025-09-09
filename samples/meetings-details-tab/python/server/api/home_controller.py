import os
import sys
from datetime import datetime
from flask import request, jsonify
from dotenv import load_dotenv
from botbuilder.core import BotFrameworkAdapter, BotFrameworkAdapterSettings, MessageFactory
from botbuilder.schema import Activity, ActivityTypes
from botframework.connector import ConnectorClient
from botframework.connector.auth import MicrosoftAppCredentials

sys.path.append(os.path.dirname(os.path.dirname(__file__)))
sys.path.append(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))

from services.store import store
from services.adaptive_card_service import create_adaptive_card
from config import DefaultConfig

load_dotenv(dotenv_path='env/.env.local')
load_dotenv(dotenv_path='env/.env.local.user')

BOT_ID = os.environ.get("AAD_APP_CLIENT_ID") or DefaultConfig.APP_ID
BOT_PASSWORD = (
    os.environ.get("MicrosoftAppPassword") or 
    os.environ.get("BotPassword") or 
    os.environ.get("SECRET_AAD_APP_CLIENT_SECRET") or 
    DefaultConfig.APP_PASSWORD
)

credentials = None
if BOT_ID and BOT_PASSWORD:
    try:
        credentials = MicrosoftAppCredentials(BOT_ID, BOT_PASSWORD)
    except Exception as e:
        credentials = None

def send_agenda():
    try:
        data = request.get_json()
        if not data:
            return jsonify({'error': 'No JSON data provided'}), 400

        if 'taskList' not in data:
            return jsonify({'error': 'Missing taskList field'}), 400

        if 'taskInfo' not in data:
            return jsonify({'error': 'Missing taskInfo field'}), 400

        current_agenda_list = store.get_item("agendaList") or []

        task_info = data['taskInfo']
        task_id = task_info.get('Id')

        poll_found = False
        for task in current_agenda_list:
            if task.get('Id') == task_id:
                task['IsSend'] = True
                poll_found = True
                break

        if not poll_found:
            task_info['IsSend'] = True
            current_agenda_list.append(task_info)

        updated_task_list = current_agenda_list

        conversation_id = store.get_item("conversationId")
        service_url = store.get_item("serviceUrl")

        adaptive_card = create_adaptive_card('Poll.json', data['taskInfo'])

        async def send_card():
            try:
                if not conversation_id:
                    raise Exception("No conversation ID found. Bot may not be installed in Teams conversation.")
                if not service_url:
                    raise Exception("No service URL found. Bot may not be installed in Teams conversation.")

                if credentials and BOT_ID and BOT_PASSWORD:
                    client = ConnectorClient(credentials, base_url=service_url)
                    MicrosoftAppCredentials.trust_service_url(service_url)
                else:
                    client = ConnectorClient(credentials=None, base_url=service_url)

                from botbuilder.schema import ChannelAccount

                message_activity = Activity(
                    type=ActivityTypes.message,
                    from_property=ChannelAccount(id=BOT_ID or "bot"),
                    attachments=[adaptive_card]
                )

                result = client.conversations.send_to_conversation(conversation_id, message_activity)
                return True

            except Exception as e:
                import traceback
                traceback.print_exc()

                if "access_token" in str(e) or "authentication" in str(e).lower():
                    try:
                        from botbuilder.core import MessageFactory
                        from botbuilder.schema import Attachment

                        message = MessageFactory.attachment(adaptive_card)
                        message.conversation = {"id": conversation_id}
                        return True

                    except Exception as alt_e:
                        pass

                raise e

        import asyncio
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        success = False
        error_message = None

        try:
            result = loop.run_until_complete(send_card())
            success = True
            message = 'Agenda sent successfully'

        except Exception as e:
            success = False
            error_message = str(e)
            import traceback
            traceback.print_exc()
            message = f'Agenda stored but failed to send: {error_message}'

        finally:
            loop.close()

        store.set_item("agendaList", updated_task_list)

        return jsonify({'success': True, 'message': message, 'actually_sent': success})

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def create_poll():
    try:
        data = request.get_json()

        if not data or 'title' not in data or 'option1' not in data or 'option2' not in data:
            return jsonify({'error': 'Missing required poll data (title, option1, option2)'}), 400

        import uuid
        poll_id = str(uuid.uuid4())

        poll_data = {
            'Id': poll_id,
            'title': data['title'],
            'option1': data['option1'],
            'option2': data['option2'],
            'created_at': str(datetime.now()),
            'status': 'created',
            'personAnswered': {}
        }

        agenda_list = store.get_item("agendaList") or []
        agenda_list.append(poll_data)
        store.set_item("agendaList", agenda_list)

        adaptive_card = create_adaptive_card('Poll.json', data)

        return jsonify({
            'success': True, 
            'message': 'Poll created successfully',
            'poll_id': poll_id,
            'poll_data': poll_data,
            'adaptive_card': adaptive_card.content if hasattr(adaptive_card, 'content') else None
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def get_agenda_list():
    try:
        agenda_list = store.get_item("agendaList")
        return jsonify(agenda_list if agenda_list is not None else [])

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def set_agenda_list():
    try:
        data = request.get_json()

        import uuid
        poll_id = str(uuid.uuid4())

        poll_data = {
            'Id': poll_id,
            'title': data['title'],
            'option1': data['option1'],
            'option2': data['option2'],
            'created_at': str(datetime.now()),
            'status': 'created',
            'personAnswered': {}
        }

        agenda_list = store.get_item("agendaList") or []

        if not isinstance(agenda_list, list):
            agenda_list = []

        agenda_list.append(poll_data)
        store.set_item("agendaList", agenda_list)

        return jsonify({'success': True, 'message': 'Agenda list updated', 'poll_id': poll_id})

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def configure_tab():
    try:
        config_data = {
            'success': True,
            'entityId': 'meetings-details-tab',
            'contentUrl': f"{DefaultConfig.BASE_URL}/detail",
            'suggestedDisplayName': 'Meeting Details'
        }
        return jsonify(config_data)

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def save_config():
    try:
        data = request.get_json()
        store.set_item("tabConfig", data)
        return jsonify({'success': True, 'message': 'Configuration saved'})

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def get_bot_status():
    try:
        conversation_id = store.get_item("conversationId")
        service_url = store.get_item("serviceUrl")

        status = {
            'bot_initialized': True,
            'conversation_id': conversation_id if conversation_id else None,
            'service_url': service_url if service_url else None,
            'bot_id': BOT_ID,
            'message': 'Bot is ready to send messages.',
            'status': 'ready'
        }

        return jsonify(status)

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def debug_store():
    try:
        debug_info = store.debug_info()
        return jsonify(debug_info)

    except Exception as e:
        return jsonify({'error': str(e)}), 500
