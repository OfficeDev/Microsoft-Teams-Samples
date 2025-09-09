import os
import sys
from flask import request, jsonify
from botbuilder.core import BotFrameworkAdapter, BotFrameworkAdapterSettings
from botbuilder.schema import Activity, ActivityTypes
from dotenv import load_dotenv

load_dotenv(dotenv_path='env/.env.local')
load_dotenv(dotenv_path='env/.env.local.user')

sys.path.append(os.path.dirname(os.path.dirname(__file__)))
sys.path.append(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))

from bot.bot_activity_handler import BotActivityHandler
from config import DefaultConfig

BOT_ID = os.environ.get("AAD_APP_CLIENT_ID") or DefaultConfig.APP_ID
BOT_PASSWORD = os.environ.get("SECRET_AAD_APP_CLIENT_SECRET") or DefaultConfig.APP_PASSWORD

try:
    from botframework.connector.auth import MicrosoftAppCredentials
    test_credentials = MicrosoftAppCredentials(BOT_ID, BOT_PASSWORD)
except Exception as e:
    print(f"Failed to create MicrosoftAppCredentials: {str(e)}")

SETTINGS = BotFrameworkAdapterSettings(
    app_id=BOT_ID,
    app_password=BOT_PASSWORD
)

ADAPTER = BotFrameworkAdapter(SETTINGS)

async def on_turn_error(context, error):
    print(f"\n [onTurnError] unhandled error: {error}")
    try:
        from botbuilder.schema import ActivityTypes, Activity
        from botbuilder.core import MessageFactory

        trace_activity = Activity(
            type=ActivityTypes.trace,
            name="OnTurnError Trace",
            value=str(error),
            value_type="https://www.botframework.com/schemas/error",
            label="TurnError"
        )
        await context.send_activity(trace_activity)
    except Exception as trace_error:
        print(f"Error sending trace activity: {trace_error}")

ADAPTER.on_turn_error = on_turn_error
BOT_ACTIVITY_HANDLER = BotActivityHandler()

def handle_messages():
    try:
        body = request.get_data()
        auth_header = request.headers.get('Authorization', '')

        async def process_activity():
            try:
                import json
                body_dict = json.loads(body.decode('utf-8')) if body else {}
                activity = Activity().deserialize(body_dict)
                invoke_response = await ADAPTER.process_activity(activity, auth_header, BOT_ACTIVITY_HANDLER.on_turn)

                if activity.type == ActivityTypes.invoke:
                    if invoke_response:
                        return invoke_response.body
                    else:
                        return None
                return None

            except Exception as e:
                import traceback
                traceback.print_exc()
                raise e

        import asyncio
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            result = loop.run_until_complete(process_activity())
            if result is not None:
                return jsonify(result)
            else:
                return jsonify({'status': 'OK'})
        finally:
            loop.close()

    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({'error': str(e)}), 500

def test_bot_direct():
    try:
        from flask import request, jsonify
        from botbuilder.schema import Activity, ChannelAccount, ConversationAccount
        import asyncio

        data = request.get_json() if request.get_json() else {}

        activity = Activity(
            type="message",
            text=data.get("text", "Test message"),
            from_property=ChannelAccount(
                id="29:1test-user-id",
                name="Test User"
            ),
            recipient=ChannelAccount(
                id=BOT_ID,
                name="Meeting Details Bot"
            ),
            conversation=ConversationAccount(
                id="19:test-conversation-id@thread.v2"
            ),
            channel_id="msteams",
            service_url="https://smba.trafficmanager.net/amer/test/",
            id="test-message-direct",
            timestamp="2025-07-27T00:00:00.000Z"
        )

        from botbuilder.core import TurnContext
        from botbuilder.core.conversation_state import ConversationState
        from botbuilder.core.memory_storage import MemoryStorage
        from botbuilder.core.user_state import UserState

        memory_storage = MemoryStorage()
        conversation_state = ConversationState(memory_storage)
        user_state = UserState(memory_storage)

        turn_context = TurnContext(ADAPTER, activity)

        async def test_bot_handler():
            await BOT_ACTIVITY_HANDLER.on_message_activity(turn_context)
            return "Bot handler test completed successfully"

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            result = loop.run_until_complete(test_bot_handler())
            return jsonify({"success": True, "message": result}), 200
        finally:
            loop.close()

    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({"success": False, "error": str(e)}), 500

def debug_auth():
    try:
        from flask import request, jsonify

        auth_header = request.headers.get('Authorization', '')

        if auth_header.startswith('Bearer '):
            token = auth_header[7:]
            segments = token.split('.')

        return jsonify({
            "success": True, 
            "auth_header_present": bool(auth_header),
            "auth_header_length": len(auth_header),
            "token_segments": len(auth_header[7:].split('.')) if auth_header.startswith('Bearer ') else 0
        }), 200

    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({"success": False, "error": str(e)}), 500
