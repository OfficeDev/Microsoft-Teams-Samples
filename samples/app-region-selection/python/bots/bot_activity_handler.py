import json
from botbuilder.core import ActivityHandler, TurnContext, UserState, MessageFactory, CardFactory
from botbuilder.schema import HeroCard, CardAction, ActionTypes
from typing import List
import os

class RegionSelectionTab(ActivityHandler):
    def __init__(self, user_state: UserState):
        self._user_state = user_state

    async def on_message_activity(self, turn_context: TurnContext):
        welcome_user_state_accessor = self._user_state.create_property("WelcomeUserState")
        did_bot_welcome_user = await welcome_user_state_accessor.get(turn_context, lambda: WelcomeUserState())

        text = turn_context.activity.text.lower()

        if did_bot_welcome_user.did_user_selected_domain and text in ["change", "yes"]:
            await self.send_change_domain_confirmation_card(turn_context)
            return

        if text in ["reset", "change", "yes"]:
            await self.send_domain_lists_card(turn_context)
        elif text in ["no", "cancel"]:
            await self.welcome_card(turn_context)
        else:
            await self.send_welcome_intro_card(turn_context)

    async def on_members_added(self, members_added: List, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                welcome_user_state_accessor = self._user_state.create_property("WelcomeUserState")
                did_bot_welcome_user = await welcome_user_state_accessor.get(turn_context, lambda: WelcomeUserState())

                if did_bot_welcome_user.did_user_selected_domain:
                    did_bot_welcome_user.did_user_selected_domain = False
                    did_bot_welcome_user.selected_region = ""
                    did_bot_welcome_user.selected_domain = ""

                await self.send_welcome_intro_card(turn_context)

    async def send_welcome_intro_card(self, turn_context: TurnContext):
        domain, region = "", ""
        if turn_context.activity.text and self.is_any_domain_selected(turn_context.activity.text):
            await self.welcome_card(turn_context)
            return

        welcome_user_state_accessor = self._user_state.create_property("WelcomeUserState")
        did_bot_welcome_user = await welcome_user_state_accessor.get(turn_context, lambda: WelcomeUserState())

        if did_bot_welcome_user.did_user_selected_domain:
            domain = did_bot_welcome_user.selected_domain
            region = did_bot_welcome_user.selected_region
        else:
            domain, region = self.get_default_info(turn_context)

        welcome_msg = f"Your default Region is {region}."
        card = HeroCard(
            title="Welcome to Region Selection App!",
            subtitle="This will help you to choose your data center's region.",
            text=welcome_msg + " Would you like to change region?",
            buttons=[
                CardAction(type=ActionTypes.message_back, title="Yes", text="Yes"),
                CardAction(type=ActionTypes.message_back, title="No", text="No")
            ]
        )

        response = MessageFactory.attachment(CardFactory.hero_card(card))
        await turn_context.send_activity(response)

    def get_default_info(self, turn_context: TurnContext):
        service_url = turn_context.activity.service_url
        domain = service_url[service_url.rfind(".") + 1:].strip("/")
        region = turn_context.activity.locale
        return domain, region

    def get_selected_info(self, text):
        domain, region = "", ""
        if len(text.split("-")) > 1:
            domain = text.split("-")[0].strip()

        # Read JSON data from file
        file_path = os.path.join(os.getcwd(), "ConfigData", "Regions.json")
        with open(file_path, 'r') as file:
            json_data = json.load(file)
            selected_info = next((item for item in json_data["regionDomains"] if item["region"] == domain), None)

        if selected_info:
            region = selected_info["region"]
            domain = selected_info["domain"]

        return domain, region

    async def send_domain_lists_card(self, turn_context: TurnContext):
        # Read JSON data from file
        file_path = os.path.join(os.getcwd(), "ConfigData", "Regions.json")
        with open(file_path, 'r') as file:
            json_data = json.load(file)

        region_button_list = [
            CardAction(
                type=ActionTypes.message_back,
                title=f"{item['region']} - {item['country']}",
                text=f"{item['region']} - {item['country']}",
                display_text=f"{item['region']} - {item['country']}",
                image="https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"
            ) for item in json_data["regionDomains"]
        ]

        card = HeroCard(
            text="Please select your region,",
            buttons=region_button_list
        )

        response = MessageFactory.attachment(CardFactory.hero_card(card))
        await turn_context.send_activity(response)

    async def welcome_card(self, turn_context: TurnContext):
        welcome_user_state_accessor = self._user_state.create_property("WelcomeUserState")
        did_bot_welcome_user = await welcome_user_state_accessor.get(turn_context, lambda: WelcomeUserState())

        user_name = turn_context.activity.from_property.name
        domain_name, region_name = "", ""

        # Get selected info
        domain_name, region_name = self.get_selected_info(turn_context.activity.text)

        if not domain_name and did_bot_welcome_user.did_user_selected_domain:
            domain_name = did_bot_welcome_user.selected_domain
            region_name = did_bot_welcome_user.selected_region

        if not domain_name:
            domain_name, region_name = self.get_default_info(turn_context)

        card = HeroCard(
            title=f"Welcome {user_name}, ",
            subtitle=f"You are in {region_name} Region's Data Center",
            text="If you want to change data center's region, please enter text 'Change'"
        )

        did_bot_welcome_user.did_user_selected_domain = True
        did_bot_welcome_user.selected_domain = domain_name
        did_bot_welcome_user.selected_region = region_name

        await self._user_state.save_changes(turn_context)
        response = MessageFactory.attachment(CardFactory.hero_card(card))
        await turn_context.send_activity(response)

    async def send_change_domain_confirmation_card(self, turn_context: TurnContext):
        user_name = turn_context.activity.from_property.name
        welcome_user_state_accessor = self._user_state.create_property("WelcomeUserState")
        did_bot_welcome_user = await welcome_user_state_accessor.get(turn_context, lambda: WelcomeUserState())

        domain_button_list = [
            CardAction(type=ActionTypes.message_back, title="Reset", text="Reset"),
            CardAction(type=ActionTypes.message_back, title="Cancel", text="Cancel")
        ]

        card = HeroCard(
            text=f"Hi {user_name}, You have already selected your data center region: {did_bot_welcome_user.selected_region}. Would you like to change this?",
            buttons=domain_button_list
        )

        response = MessageFactory.attachment(CardFactory.hero_card(card))
        await turn_context.send_activity(response)

    def is_any_domain_selected(self, text):
        domain = ""
        if len(text.split("-")) > 1:
            domain = text.split("-")[0].strip()

        if not domain:
            return False

        file_path = os.path.join(os.getcwd(), "ConfigData", "Regions.json")
        with open(file_path, 'r') as file:
            json_data = json.load(file)
            is_domain_selected = any(item["region"] == domain for item in json_data["regionDomains"])

        return is_domain_selected
    
class WelcomeUserState:
    def __init__(self):
        self.did_user_selected_domain = False
        self.selected_region = ""
        self.selected_domain = ""