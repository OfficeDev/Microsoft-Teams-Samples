import os
import logging
from aiohttp import web
from botbuilder.core import (
    BotFrameworkAdapterSettings,
    TurnContext,
    BotFrameworkAdapter
)
from bots.bot import AppCheckInBot 
from botbuilder.schema import Activity

logging.basicConfig(level=logging.INFO)

APP_ID = os.getenv("MicrosoftAppId", "")
APP_PASSWORD = os.getenv("MicrosoftAppPassword", "")

adapter_settings = BotFrameworkAdapterSettings(APP_ID, APP_PASSWORD)
adapter = BotFrameworkAdapter(adapter_settings)

async def on_error(context: TurnContext, error: Exception):
    logging.error(f"[on_turn_error] unhandled error: {error}")
    await context.send_activity("Sorry, something went wrong.")
    await context.send_trace_activity(
        label="OnTurnError Trace",
        name="TurnError",
        value=str(error),
        value_type="https://www.botframework.com/schemas/error"
    )

adapter.on_turn_error = on_error

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
    response = await adapter.process_activity(activity, auth_header, bot.on_turn)
    if response:
        return web.json_response(data=response.body, status=response.status)
    return web.Response(status=201)

app.router.add_routes(routes)
app.router.add_static('/Images/', path='./Images', name='images')

if __name__ == '__main__':
    port = int(os.getenv("PORT", 3978))
    web.run_app(app, port=port)
