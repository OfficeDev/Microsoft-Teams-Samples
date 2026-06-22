# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import sys
import traceback

from aiohttp import web
from botbuilder.core import TurnContext
from botbuilder.core.integration import aiohttp_error_middleware
from botbuilder.integration.aiohttp import (
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
)

from bots import BotActivityHandler
from config import DefaultConfig

CONFIG = DefaultConfig()

ADAPTER = CloudAdapter(ConfigurationBotFrameworkAuthentication(CONFIG))


async def on_error(context: TurnContext, error: Exception) -> None:
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()
    await context.send_activity("The bot encountered an error or bug.")


ADAPTER.on_turn_error = on_error

BOT = BotActivityHandler()


async def messages(request: web.Request) -> web.Response:
    return await ADAPTER.process(request, BOT)


APP = web.Application(middlewares=[aiohttp_error_middleware])
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    web.run_app(APP, host="0.0.0.0", port=CONFIG.PORT)
