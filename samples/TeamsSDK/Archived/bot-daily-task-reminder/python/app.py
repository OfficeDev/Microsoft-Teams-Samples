# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import traceback
from datetime import UTC, datetime
from aiohttp import web
from aiohttp.web import Request
from botbuilder.core import TurnContext
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.schema import Activity, ActivityTypes
from bots import DailyReminderBot 
from config import DefaultConfig

# Patch msrest/botbuilder serialization incompatibility:
# msrest's _serialize calls target_obj.is_xml_model() which fails on plain dicts.
import importlib as _il
_sh_mod = _il.import_module("botbuilder.core.serializer_helper")
_ca_mod = _il.import_module("botbuilder.integration.aiohttp.cloud_adapter")
_original_serializer_helper = _sh_mod.serializer_helper

def _patched_serializer_helper(obj):
    if isinstance(obj, dict):
        return obj
    return _original_serializer_helper(obj)

_sh_mod.serializer_helper = _patched_serializer_helper
_ca_mod.serializer_helper = _patched_serializer_helper

CONFIG = DefaultConfig()

# Create the adapter
ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


async def on_error(context: TurnContext, error: Exception):
    traceback.print_exc()

    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity(
        "To continue to run this bot, please fix the bot source code."
    )
    if context.activity.channel_id == "emulator":
        trace_activity = Activity(
            label="TurnError",
            name="on_turn_error Trace",
            timestamp=datetime.now(UTC),
            type=ActivityTypes.trace,
            value=f"{error}",
            value_type="https://www.botframework.com/schemas/error",
        )
        await context.send_activity(trace_activity)


ADAPTER.on_turn_error = on_error

async def scheduletask(request):
    return web.FileResponse('src/views/scheduletask.html')

bot = DailyReminderBot()

# Listen for incoming requests on /api/messages.
async def messages(req: Request):
    return await ADAPTER.process(req, bot)

APP = web.Application(middlewares=[aiohttp_error_middleware])

APP.router.add_get("/scheduletask", scheduletask)
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    web.run_app(APP, host="localhost", port=CONFIG.PORT)
