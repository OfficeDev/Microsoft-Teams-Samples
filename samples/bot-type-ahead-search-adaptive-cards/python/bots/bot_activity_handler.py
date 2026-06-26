from botbuilder.core import ActivityHandler, CardFactory, TurnContext
import aiohttp
from botbuilder.schema import Activity, ActivityTypes,InvokeResponse
import urllib.parse

class TeamsBot(ActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_members_added_activity(self, members_added, context: TurnContext):
        for member in members_added:
            if member.id != context.activity.recipient.id:
                await context.send_activity("Hello and welcome! With this sample you can see the functionality of static and dynamic search in adaptive card")

    async def on_message_activity(self, context: TurnContext):
        text = context.activity.text.strip().lower() if context.activity.text else ""
        
        if text == "staticsearch":
            user_card = CardFactory.adaptive_card(self.adaptive_card_for_static_search())
            activity = Activity(
                type=ActivityTypes.message,
                attachments=[user_card]
            )
            await context.send_activity(activity)
        elif text == "dynamicsearch":
            user_card = CardFactory.adaptive_card(self.adaptive_card_for_dynamic_search())
            activity = Activity(
                type=ActivityTypes.message,
                attachments=[user_card]
            )
            await context.send_activity(activity)
        elif text == "dependantdropdown":
            user_card = CardFactory.adaptive_card(self.adaptive_card_for_dependant_search())
            activity = Activity(
                type=ActivityTypes.message,
                attachments=[user_card]
            )
            await context.send_activity(activity)
        elif context.activity.value:
            await context.send_activity(f"Selected option is: {context.activity.value.get('choiceselect', '')}")

    async def on_invoke_activity(self, context: TurnContext):
        if context.activity.name == 'application/search':
            dropdown_card = context.activity.value.get("data", {}).get("choiceselect")
            search_query = context.activity.value.get("queryText", "")
            
            url = f"http://registry.npmjs.com/-/v1/search?{urllib.parse.urlencode({'text': search_query, 'size': 8})}"
            async with aiohttp.ClientSession() as session:
                async with session.get(url) as response:
                    if response.status == 200:
                        data = await response.json()
                        npm_packages = [
                            {
                                'title': obj['package']['name'],
                                'value': f"{obj['package']['name']} - {obj['package'].get('description', 'No description available')}"
                            }
                            for obj in data.get('objects', [])
                        ]

                        return self._build_search_response(dropdown_card, npm_packages)
                    elif response.status == 204:
                        return {'status': 204, 'body': {'type': 'application/vnd.microsoft.search.searchResponse'}}
                    elif response.status == 500:
                        return {'status': 500, 'body': {'type': 'application/vnd.microsoft.error', 'value': {'code': '500', 'message': 'Error message: Internal Server Error'}}}
        return None

    def _build_search_response(self, dropdown_card, npm_packages):
        country_options = {
            'usa': [{'title': "CA", 'value': "CA"}, {'title': "FL", 'value': "FL"}, {'title': "TX", 'value': "TX"}],
            'france': [{'title': "Paris", 'value': "Paris"}, {'title': "Lyon", 'value': "Lyon"}, {'title': "Nice", 'value': "Nice"}],
            'default': [{'title': "Delhi", 'value': "Delhi"}, {'title': "Mumbai", 'value': "Mumbai"}, {'title': "Pune", 'value': "Pune"}]
        }
        results = country_options.get(dropdown_card.lower(), country_options['default']) if dropdown_card else npm_packages
        return InvokeResponse(status=200, body={'type': 'application/vnd.microsoft.search.searchResponse', 'value': {'results': results}} )

    def adaptive_card_for_static_search(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {"text": "Please search for the IDE from static list.", "wrap": True, "type": "TextBlock"},
                {"columns": [{"width": "stretch", "items": [{
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
                        {"title": "Visual Studio Code", "value": "visual_studio_code"}
                    ], "style": "filtered", "placeholder": "Search for an IDE", "id": "choiceselect", "type": "Input.ChoiceSet"
                }], "type": "Column"}], "type": "ColumnSet"}],
            "actions": [{"type": "Action.Submit", "id": "submit", "title": "Submit"}]
        }

    def adaptive_card_for_dynamic_search(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {"text": "Please search for npm packages using dynamic search control.", "wrap": True, "type": "TextBlock"},
                {"columns": [{"width": "stretch", "items": [{
                    "choices": [
                        {"title": "Static Option 1", "value": "static_option_1"},
                        {"title": "Static Option 2", "value": "static_option_2"},
                        {"title": "Static Option 3", "value": "static_option_3"}
                    ], "isMultiSelect": False, "style": "filtered", "choices.data": {"type": "Data.Query", "dataset": "npmpackages"}, "id": "choiceselect", "type": "Input.ChoiceSet"
                }], "type": "Column"}], "type": "ColumnSet"}],
            "actions": [{"type": "Action.Submit", "id": "submitdynamic", "title": "Submit"}]
        }

    def adaptive_card_for_dependant_search(self):
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
                    "type": "TextBlock"
                },
                {
                    "id": "choiceselect",
                    "type": "Input.ChoiceSet",
                    "label": "Select a country or region:",
                    "choices": [
                        {"title": "USA", "value": "usa"},
                        {"title": "France", "value": "france"},
                        {"title": "India", "value": "india"}
                    ],
                    "valueChangedAction": {
                        "type": "Action.ResetInputs",
                        "targetInputIds": ["city"]
                    },
                    "isRequired": True,
                    "errorMessage": "Please select a country or region"
                },
                {
                    "style": "filtered",
                    "choices.data": {
                        "type": "Data.Query",
                        "dataset": "cities",
                        "associatedInputs": "auto"
                    },
                    "id": "city",
                    "type": "Input.ChoiceSet",
                    "label": "Select a city:",
                    "placeholder": "Type to search for a city in the selected country",
                    "isRequired": True,
                    "errorMessage": "Please select a city"
                }
            ],
            "actions": [
                {
                    "title": "Submit",
                    "type": "Action.Submit"
                }
            ]
        }