from aiohttp import web
import os
import httpx
from services.token_store import TokenStore


async def auth_callback(request: web.Request) -> web.Response:
    code = request.query.get("code")
    state = request.query.get("state")

    if not code or not state:
        return web.Response(status=400, text="Missing code or state in the callback")

    try:
        async with httpx.AsyncClient() as client:
            response = await client.post(
                f"https://{os.environ['AUTH0_DOMAIN']}/oauth/token",
                headers={"Content-Type": "application/json"},
                json={
                    "grant_type": "authorization_code",
                    "client_id": os.environ["AUTH0_CLIENT_ID"],
                    "client_secret": os.environ["AUTH0_CLIENT_SECRET"],
                    "code": code,
                    "redirect_uri": f"{os.environ['BOT_ENDPOINT']}/api/auth/callback"
                }
            )

            if response.status_code != 200:
                return web.Response(status=500, text="Failed to exchange code for token")

            data = response.json()
            access_token = data.get("access_token")

            if access_token:
                TokenStore().set_token(state, access_token)
                raise web.HTTPFound("/src/views/auth-end.html")
            else:
                return web.Response(status=400, text="No access token received")

    except web.HTTPFound:
        raise
    except Exception as e:
        print("Auth callback error:", e)
        return web.Response(status=500, text="Authentication failed")