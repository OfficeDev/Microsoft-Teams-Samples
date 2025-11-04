#Copyright (c) Microsoft Corporation. All rights reserved.
#Licensed under the MIT License.

import os
import logging
from aiohttp import web
from botbuilder.core import (
    TurnContext
    
)
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from bots.bot import AppCheckInBot 
from botbuilder.schema import Activity
from config import DefaultConfig

logging.basicConfig(level=logging.INFO)

CONFIG = DefaultConfig()

# Use direct environment variables for authentication
AUTH_CONFIG = {
    "MicrosoftAppType": os.environ.get("MicrosoftAppType", "SingleTenant"),
    "MicrosoftAppId": os.environ.get("MicrosoftAppId", ""),
    "MicrosoftAppPassword": os.environ.get("MicrosoftAppPassword", ""),
    "MicrosoftAppTenantId": os.environ.get("MicrosoftAppTenantId", "")
}

# Create authentication configuration
BOT_AUTHENTICATION = ConfigurationBotFrameworkAuthentication(AUTH_CONFIG)
ADAPTER = CloudAdapter(BOT_AUTHENTICATION)

async def on_error(context: TurnContext, error: Exception):
    logging.error(f"[on_turn_error] unhandled error: {error}")
    await context.send_activity("Sorry, something went wrong.")
    await context.send_trace_activity(
        label="OnTurnError Trace",
        name="TurnError",
        value=str(error),
        value_type="https://www.botframework.com/schemas/error"
    )

ADAPTER.on_turn_error = on_error

bot = AppCheckInBot()

app = web.Application()
routes = web.RouteTableDef()

@routes.get('/CheckIn')
async def checkin(request):
    return web.FileResponse('./src/views/CheckIn.html')

@routes.get('/ViewLocation')
async def viewlocation(request):
    return web.FileResponse('./src/views/ViewLocation.html')

@routes.get('/{tail:.*}')
async def fallback(request):
    return web.json_response({'error': 'Route not found'}, status=404)

@routes.post('/api/messages')
async def messages(request):
    body = await request.json()
    activity = Activity().deserialize(body)
    auth_header = request.headers.get("Authorization", "")
    response = await ADAPTER.process(activity, auth_header, bot.on_turn)
    if response:
        return web.json_response(data=response.body, status=response.status)
    return web.Response(status=201)

app.router.add_routes(routes)
app.router.add_static('/Images/', path='./Images', name='images')

if __name__ == '__main__':
    port = int(os.getenv("PORT", 3979))  # Use 3979 as default to avoid conflict
    web.run_app(app, port=port)
