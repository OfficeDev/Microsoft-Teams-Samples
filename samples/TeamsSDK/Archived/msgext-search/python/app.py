# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import logging
import sys
import traceback

from dotenv import load_dotenv

load_dotenv()  # Must run before config.py is imported so os.environ is populated

import requests
from aiohttp import web
from botbuilder.core import CardFactory, TurnContext
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.integration.aiohttp import CloudAdapter, ConfigurationBotFrameworkAuthentication
from botbuilder.schema import HeroCard
from botbuilder.schema.teams import (
    MessagingExtensionQuery,
    MessagingExtensionResponse,
    MessagingExtensionResult,
    MessagingExtensionAttachment,
)

from config import DefaultConfig

logging.basicConfig(
    level=logging.INFO,
    stream=sys.stdout,
    format="%(asctime)s %(levelname)-8s %(name)s %(message)s",
)
LOGGER = logging.getLogger(__name__)

CONFIG = DefaultConfig()
AUTH = ConfigurationBotFrameworkAuthentication(CONFIG)
ADAPTER = CloudAdapter(AUTH)


async def on_error(context: TurnContext, error: Exception):
    LOGGER.error("on_turn_error: %s", error, exc_info=True)
    traceback.print_exc()


ADAPTER.on_turn_error = on_error

def get_search_results(query: str):
    """Search GitHub repositories."""
    url = f"https://api.github.com/search/repositories?q={query}"
    headers = {"Accept": "application/vnd.github.v3+json"}
    response = requests.get(url, headers=headers)
    search_results = []
    for result in response.json().get("items", []):
        name = result["name"]
        description = result["description"]
        repo_url = result["html_url"]
        search_results.append({
            "name": name,
            "summary": description if description else "",
            "url": repo_url,
        })
    return search_results[:10]

class TeamsMessagingExtensionsSearchBot(TeamsActivityHandler):
    async def on_teams_messaging_extension_query(
        self, turn_context: TurnContext, query: MessagingExtensionQuery
    ):
        search_query = ""
        if query.parameters and len(query.parameters) > 0:
            search_query = query.parameters[0].value or ""

        results = get_search_results(search_query) if search_query else []

        attachments = []
        for item in results:
            # Adaptive Card for main content
            adaptive_card_content = {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.4",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": item["name"],
                        "size": "Large",
                        "weight": "Bolder",
                    },
                    {
                        "type": "TextBlock",
                        "text": item["summary"][:100] if item["summary"] else "",
                        "wrap": True,
                    },
                ],
                "actions": [
                    {
                        "type": "Action.OpenUrl",
                        "title": "View on GitHub",
                        "url": item["url"],
                    }
                ],
            }
            card_attachment = CardFactory.adaptive_card(adaptive_card_content)

            # Hero Card for thumbnail preview
            preview_attachment = CardFactory.hero_card(
                HeroCard(
                    title=item["name"],
                    subtitle=item["summary"][:50] if item["summary"] else "",
                )
            )

            attachments.append(
                MessagingExtensionAttachment(
                    content_type=card_attachment.content_type,
                    content=card_attachment.content,
                    preview=preview_attachment,
                )
            )

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",
                attachment_layout="list",
                attachments=attachments,
            )
        )


BOT = TeamsMessagingExtensionsSearchBot()

async def messages(req: web.Request) -> web.Response:
    LOGGER.info("POST /api/messages received")
    return await ADAPTER.process(req, BOT)

APP = web.Application()
APP.router.add_post("/api/messages", messages)

if __name__ == "__main__":
    web.run_app(APP, host="localhost", port=CONFIG.PORT)
