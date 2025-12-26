# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))

import requests
from fastapi.staticfiles import StaticFiles
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.apps.http_plugin import HttpPlugin
from microsoft_teams.api import MessageActivity, MessageActivityInput, Attachment
from services.token_store import TokenStore
from controllers.auth_routes import router as auth_router
from config import Config

config = Config()

# Create the Teams App
app = App(
    client_id=config.APP_ID,
    client_secret=config.APP_PASSWORD,
    tenant_id=config.APP_TENANTID
)

# Token store instance
token_store = TokenStore()

# Handle incoming message activities from users
@app.on_message
async def on_message_activity(ctx: ActivityContext[MessageActivity]):
    user_id = ctx.activity.from_.id
    text = (ctx.activity.text or "").strip().lower()
    if text == "logout":
        token_store.remove_token(user_id)
        logout_url = (
            f"https://{config.AUTH0_DOMAIN}/v2/logout"
            f"?client_id={config.AUTH0_CLIENT_ID}"
        )
        logout_card = Attachment(
            content={
                "type": "AdaptiveCard",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "You've been logged out.",
                        "size": "Medium",
                        "weight": "Bolder",
                        "wrap": True,
                    }
                ],
                "actions": [
                    {
                        "type": "Action.OpenUrl",
                        "title": "Logout from Auth0",
                        "url": logout_url,
                    }
                ],
            },
            content_type="application/vnd.microsoft.card.adaptive"
        )
        message = MessageActivityInput(attachments=[logout_card])
        await ctx.send(message)
        return
    access_token = token_store.get_token(user_id)
    if access_token:
        if text == "profile details":
            try:
                response = requests.get(
                    f"https://{config.AUTH0_DOMAIN}/userinfo",
                    headers={"Authorization": f"Bearer {access_token}"},
                )
                if response.status_code == 200:
                    profile_data = response.json()
                    profile_card = Attachment(
                        content={
                            "type": "AdaptiveCard",
                            "version": "1.3",
                            "body": [
                                {
                                    "type": "TextBlock",
                                    "text": "Auth0 Profile",
                                    "size": "Large",
                                    "weight": "Bolder",
                                    "wrap": True,
                                },
                                {
                                    "type": "Image",
                                    "url": profile_data.get("picture", "https://via.placeholder.com/150"),
                                    "size": "Medium",
                                    "style": "Person",
                                },
                                {
                                    "type": "TextBlock",
                                    "text": f"Name: {profile_data.get('name')}",
                                    "wrap": True,
                                },
                                {
                                    "type": "TextBlock",
                                    "text": f"Email: {profile_data.get('email')}",
                                    "wrap": True,
                                },
                            ],
                        },
                        content_type="application/vnd.microsoft.card.adaptive"
                    )
                    message = MessageActivityInput(attachments=[profile_card])
                    await ctx.send(message)
                else:
                    await ctx.send("Failed to fetch profile details.")
            except Exception as e:
                print(f"Error fetching profile: {e}")
                await ctx.send("Error retrieving profile details.")
        else:
            await ctx.send("Say 'profile details' to get your profile or 'logout' to log out.")
    else:
        login_url = generate_login_url(user_id)
        login_card = Attachment(
            content={
                "type": "AdaptiveCard",
                "version": "1.3",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Login Required",
                        "size": "Medium",
                        "weight": "Bolder",
                        "wrap": True,
                    }
                ],
                "actions": [
                    {
                        "type": "Action.OpenUrl",
                        "title": "Login",
                        "url": login_url,
                    }
                ],
            },
            content_type="application/vnd.microsoft.card.adaptive"
        )
        message = MessageActivityInput(attachments=[login_card])
        await ctx.send(message)


# Generate Auth0 login URL with user context
def generate_login_url(user_id: str) -> str:
    return (
        f"https://{config.AUTH0_DOMAIN}/authorize"
        f"?response_type=code&client_id={config.AUTH0_CLIENT_ID}"
        f"&redirect_uri={config.BOT_ENDPOINT}/api/auth/callback"
        f"&scope=openid profile email"
        f"&state={user_id}"
    )


# Configure custom FastAPI routes for authentication
def setup_custom_routes():
    http_plugin = next((p for p in app.plugins if isinstance(p, HttpPlugin)), None)
    fastapi_app = http_plugin.app
    fastapi_app.include_router(auth_router, prefix="/api/auth")
    fastapi_app.mount("/src/views", StaticFiles(directory=os.path.join(os.path.dirname(__file__), "views")), name="views")


# Start the bot application
async def main():
    setup_custom_routes()
    await app.start()
    print(f"\nBot started, app listening to port {config.PORT}")

if __name__ == "__main__":
    asyncio.run(main())
