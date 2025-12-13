"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

Description: initialize the api and route incoming messages
to our app
"""

from http import HTTPStatus

from aiohttp import web
from botbuilder.core.integration import aiohttp_error_middleware

from bot import app

routes = web.RouteTableDef()


@routes.post("/api/messages")
async def on_messages(req: web.Request) -> web.Response:
    res = await app.process(req)

    if res is not None:
        return res

    return web.Response(status=HTTPStatus.OK)


api = web.Application(middlewares=[aiohttp_error_middleware])
api.add_routes(routes)
