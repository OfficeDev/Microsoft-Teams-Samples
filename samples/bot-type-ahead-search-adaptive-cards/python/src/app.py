import asyncio
import re
import urllib.parse
import aiohttp

from azure.identity import ManagedIdentityCredential
from microsoft.teams.api import (
    MessageActivity,
    MessageActivityInput,
    InvokeActivity,
    InvokeResponse,
)
from microsoft.teams.apps import ActivityContext, App
from config import Config

config = Config()


def create_token_factory():
    def get_token(scopes, tenant_id=None):
        credential = ManagedIdentityCredential(client_id=config.APP_ID)
        if isinstance(scopes, str):
            scopes_list = [scopes]
        else:
            scopes_list = scopes
        token = credential.get_token(*scopes_list)
        return token.token

    return get_token


app = App(
    token=create_token_factory() if config.APP_TYPE == "UserAssignedMsi" else None
)


# ============================================================================
# Helper: Create Adaptive Card Message
# ============================================================================


def create_card_message(card_dict: dict) -> MessageActivityInput:
    """Create a MessageActivityInput with an adaptive card attachment."""
    return MessageActivityInput(
        attachments=[
            {
                "contentType": "application/vnd.microsoft.card.adaptive",
                "content": card_dict,
            }
        ]
    )


# ============================================================================
# Adaptive Card Definitions
# ============================================================================


def adaptive_card_for_static_search() -> dict:
    """Create an adaptive card with static filtered search for IDEs."""
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.2",
        "type": "AdaptiveCard",
        "body": [
            {
                "text": "Please search for the IDE from static list.",
                "wrap": True,
                "type": "TextBlock",
            },
            {
                "columns": [
                    {
                        "width": "stretch",
                        "items": [
                            {
                                "choices": [
                                    {"title": "Visual studio", "value": "visual_studio"},
                                    {"title": "IntelliJ IDEA", "value": "intelliJ_IDEA"},
                                    {"title": "Aptana Studio 3", "value": "aptana_studio_3"},
                                    {"title": "PyCharm", "value": "pycharm"},
                                    {"title": "PhpStorm", "value": "phpstorm"},
                                    {"title": "WebStorm", "value": "webstorm"},
                                    {"title": "NetBeans", "value": "netbeans"},
                                    {"title": "Eclipse", "value": "eclipse"},
                                    {"title": "RubyMine", "value": "rubymine"},
                                    {"title": "Visual Studio Code", "value": "visual_studio_code"},
                                ],
                                "style": "filtered",
                                "placeholder": "Search for an IDE",
                                "id": "choiceselect",
                                "type": "Input.ChoiceSet",
                            }
                        ],
                        "type": "Column",
                    }
                ],
                "type": "ColumnSet",
            },
        ],
        "actions": [{"type": "Action.Submit", "id": "submit", "title": "Submit"}],
    }


def adaptive_card_for_dynamic_search() -> dict:
    """Create an adaptive card with dynamic search for npm packages."""
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.2",
        "type": "AdaptiveCard",
        "body": [
            {
                "text": "Please search for npm packages using dynamic search control.",
                "wrap": True,
                "type": "TextBlock",
            },
            {
                "columns": [
                    {
                        "width": "stretch",
                        "items": [
                            {
                                "choices": [
                                    {"title": "Static Option 1", "value": "static_option_1"},
                                    {"title": "Static Option 2", "value": "static_option_2"},
                                    {"title": "Static Option 3", "value": "static_option_3"},
                                ],
                                "isMultiSelect": False,
                                "style": "filtered",
                                "choices.data": {
                                    "type": "Data.Query",
                                    "dataset": "npmpackages",
                                },
                                "id": "choiceselect",
                                "type": "Input.ChoiceSet",
                            }
                        ],
                        "type": "Column",
                    }
                ],
                "type": "ColumnSet",
            },
        ],
        "actions": [{"type": "Action.Submit", "id": "submitdynamic", "title": "Submit"}],
    }


def adaptive_card_for_dependant_search() -> dict:
    """Create an adaptive card with dependent/cascading dropdowns (Country -> City)."""
    return {
        "type": "AdaptiveCard",
        "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "body": [
            {
                "size": "ExtraLarge",
                "text": "Country Picker",
                "weight": "Bolder",
                "wrap": True,
                "type": "TextBlock",
            },
            {
                "id": "choiceselect",
                "type": "Input.ChoiceSet",
                "label": "Select a country or region:",
                "choices": [
                    {"title": "USA", "value": "usa"},
                    {"title": "France", "value": "france"},
                    {"title": "India", "value": "india"},
                ],
                "valueChangedAction": {
                    "type": "Action.ResetInputs",
                    "targetInputIds": ["city"],
                },
                "isRequired": True,
                "errorMessage": "Please select a country or region",
            },
            {
                "style": "filtered",
                "choices.data": {
                    "type": "Data.Query",
                    "dataset": "cities",
                    "associatedInputs": "auto",
                },
                "id": "city",
                "type": "Input.ChoiceSet",
                "label": "Select a city:",
                "placeholder": "Type to search for a city in the selected country",
                "isRequired": True,
                "errorMessage": "Please select a city",
            },
        ],
        "actions": [{"title": "Submit", "type": "Action.Submit"}],
    }


# ============================================================================
# Helper Functions
# ============================================================================


def build_search_response(dropdown_card: str, npm_packages: list) -> dict:
    """Build the search response based on dropdown selection or npm packages."""
    country_options = {
        "usa": [
            {"title": "CA", "value": "CA"},
            {"title": "FL", "value": "FL"},
            {"title": "TX", "value": "TX"},
        ],
        "france": [
            {"title": "Paris", "value": "Paris"},
            {"title": "Lyon", "value": "Lyon"},
            {"title": "Nice", "value": "Nice"},
        ],
        "india": [
            {"title": "Delhi", "value": "Delhi"},
            {"title": "Mumbai", "value": "Mumbai"},
            {"title": "Pune", "value": "Pune"},
        ],
    }

    # If a country is selected (for dependent dropdown), return cities for that country
    if dropdown_card:
        results = country_options.get(dropdown_card.lower(), country_options["india"])
    else:
        # Otherwise return npm packages
        results = npm_packages

    return {
        "status": 200,
        "body": {
            "type": "application/vnd.microsoft.search.searchResponse",
            "value": {"results": results},
        },
    }


async def search_npm_packages(search_query: str) -> list:
    """Search npm registry for packages matching the query."""
    url = f"http://registry.npmjs.com/-/v1/search?{urllib.parse.urlencode({'text': search_query, 'size': 8})}"

    async with aiohttp.ClientSession() as session:
        async with session.get(url) as response:
            if response.status == 200:
                data = await response.json()
                return [
                    {
                        "title": obj["package"]["name"],
                        "value": f"{obj['package']['name']} - {obj['package'].get('description', 'No description available')}",
                    }
                    for obj in data.get("objects", [])
                ]
            return []


# ============================================================================
# Message Handlers
# ============================================================================


@app.on_message_pattern(re.compile(r"^staticsearch$", re.IGNORECASE))
async def handle_static_search(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle 'staticsearch' command - send static filtered search card."""
    card = adaptive_card_for_static_search()
    message = create_card_message(card)
    await ctx.send(message)


@app.on_message_pattern(re.compile(r"^dynamicsearch$", re.IGNORECASE))
async def handle_dynamic_search(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle 'dynamicsearch' command - send dynamic search card."""
    card = adaptive_card_for_dynamic_search()
    message = create_card_message(card)
    await ctx.send(message)


@app.on_message_pattern(re.compile(r"^dependantdropdown$", re.IGNORECASE))
async def handle_dependant_dropdown(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle 'dependantdropdown' command - send dependent dropdown card."""
    card = adaptive_card_for_dependant_search()
    message = create_card_message(card)
    await ctx.send(message)


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """Handle all other message activities."""
    # Check if this is a card submit (value is present)
    if ctx.activity.value:
        choice = ctx.activity.value.get("choiceselect", "")
        city = ctx.activity.value.get("city", "")

        if city:
            await ctx.send(f"Selected country option: {choice}, City: {city}")
        elif choice:
            await ctx.send(f"Selected option is: {choice}")
        return

    # Default welcome message with available commands
    await ctx.send(
        "Hello and welcome! With this sample you can see the functionality of static "
        "and dynamic search in adaptive card.\n\n"
        "**Available commands:**\n"
        "- `staticsearch` - Show static filtered search card\n"
        "- `dynamicsearch` - Show dynamic search card (npm packages)\n"
        "- `dependantdropdown` - Show dependent dropdown card (Country -> City)"
    )


# ============================================================================
# Invoke Handler for Dynamic Search
# ============================================================================


@app.on_invoke
async def handle_invoke(ctx: ActivityContext[InvokeActivity]):
    """Handle invoke activities for dynamic search (application/search)."""
    if ctx.activity.name == "application/search":
        # Extract data from the invoke activity
        value = ctx.activity.value or {}
        dropdown_card = value.get("data", {}).get("choiceselect", "")
        search_query = value.get("queryText", "")

        # Search npm packages
        npm_packages = await search_npm_packages(search_query)

        # Build and return the search response
        response = build_search_response(dropdown_card, npm_packages)
        return InvokeResponse(status=response["status"], body=response["body"])

    # Return None for unhandled invoke activities
    return None


if __name__ == "__main__":
    asyncio.run(app.start())
