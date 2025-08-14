from fastapi import APIRouter, Request, HTTPException
from fastapi.responses import RedirectResponse
import os
import httpx
from services.token_store import TokenStore

router = APIRouter()

@router.get("/callback")
async def auth_callback(request: Request):
    code = request.query_params.get("code")
    state = request.query_params.get("state")

    if not code or not state:
        raise HTTPException(status_code=400, detail="Missing code or state in the callback")

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
                raise HTTPException(status_code=500, detail="Failed to exchange code for token")

            data = response.json()
            access_token = data.get("access_token")

            if access_token:
                TokenStore().set_token(state, access_token)
                return RedirectResponse(url="/src/views/auth-end.html")
            else:
                raise HTTPException(status_code=400, detail="No access token received")

    except Exception as e:
        print("Auth callback error:", e)
        raise HTTPException(status_code=500, detail="Authentication failed")