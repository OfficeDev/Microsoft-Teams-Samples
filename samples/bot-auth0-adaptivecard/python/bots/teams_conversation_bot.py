import os
import json
import requests
from botbuilder.core import ActivityHandler, MessageFactory, CardFactory, TurnContext
from services.token_store import TokenStore
from config import DefaultConfig

CONFIG = DefaultConfig()

class TeamsConversationBot(ActivityHandler):
    async def on_message_activity(self, turn_context: TurnContext):
        user_id = turn_context.activity.from_property.id
        text = (turn_context.activity.text or "").strip().lower()

        token_store = TokenStore()  # Get the singleton instance

        if text == "logout":
            token_store.remove_token(user_id)
            logout_url = (
                f"https://{CONFIG.AUTH0_DOMAIN}/v2/logout"
                f"?client_id={CONFIG.AUTH0_CLIENT_ID}"
            )

            logout_card = {
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
            }

            await turn_context.send_activity(
                MessageFactory.attachment(CardFactory.adaptive_card(logout_card))
            )
            return

        access_token = token_store.get_token(user_id)

        if access_token:
            if text == "profile details":
                try:
                    response = requests.get(
                        f"https://{CONFIG.AUTH0_DOMAIN}/userinfo",
                        headers={"Authorization": f"Bearer {access_token}"},
                    )

                    if response.status_code == 200:
                        profile_data = response.json()
                        profile_card = {
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
                        }

                        await turn_context.send_activity(
                            MessageFactory.attachment(CardFactory.adaptive_card(profile_card))
                        )
                    else:
                        await turn_context.send_activity(
                            MessageFactory.text("Failed to fetch profile details.")
                        )
                except Exception as e:
                    print(f"Error fetching profile: {e}")
                    await turn_context.send_activity(
                        MessageFactory.text("Error retrieving profile details.")
                    )
            else:
                await turn_context.send_activity(
                    MessageFactory.text("Say 'profile details' to get your profile or 'logout' to log out.")
                )
        else:
            login_url = self.generate_login_url(user_id)
            login_card = {
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
            }

            await turn_context.send_activity(
                MessageFactory.attachment(CardFactory.adaptive_card(login_card))
            )

    def generate_login_url(self, user_id: str) -> str:
        return (
            f"https://{CONFIG.AUTH0_DOMAIN}/authorize"
            f"?response_type=code&client_id={CONFIG.AUTH0_CLIENT_ID}"
            f"&redirect_uri={CONFIG.BOT_ENDPOINT}/api/auth/callback"
            f"&scope=openid profile email"
            f"&state={user_id}"
        )