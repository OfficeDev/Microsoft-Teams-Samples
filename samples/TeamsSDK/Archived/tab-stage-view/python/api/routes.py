# api/routes.py

from aiohttp import web
from .bot_controller import messages

# Define aiohttp routes
api_routes = web.RouteTableDef()

@api_routes.post('/api/messages')
async def messages_handler(request):
    return await messages(request)
