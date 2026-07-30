import os
import json
from botbuilder.core import ActivityHandler, TurnContext, MessageFactory, CardFactory
from botbuilder.schema import Attachment
from .deep_link_tab_helper import DeepLinkTabHelper  

class DeepLinkTabsBot(ActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_message_activity(self, turn_context: TurnContext):
        conversation_type = turn_context.activity.conversation.conversation_type
        extended_deep_link = ""
        side_panel_link = ""

        if conversation_type == "channel":
            bots_deep_link = DeepLinkTabHelper.get_deep_link_tab_channel(
                "topic1", 1, "Bots", turn_context.activity.channel_data["teamsChannelId"],
                os.getenv("TeamsAppId"), os.getenv("Channel_Entity_Id")
            )
            messaging_deep_link = DeepLinkTabHelper.get_deep_link_tab_channel(
                "topic2", 2, "Messaging Extension", turn_context.activity.channel_data["teamsChannelId"],
                os.getenv("TeamsAppId"), os.getenv("Channel_Entity_Id")
            )
            adaptive_card_deep_link = DeepLinkTabHelper.get_deep_link_tab_channel(
                "topic3", 3, "Adaptive Card", turn_context.activity.channel_data["teamsChannelId"],
                os.getenv("TeamsAppId"), os.getenv("Channel_Entity_Id")
            )
            extended_deep_link = DeepLinkTabHelper.get_deep_link_tab_channel(
                "", 4, "Extended Deeplink features", turn_context.activity.channel_data["teamsChannelId"],
                os.getenv("TeamsAppId"), os.getenv("Channel_Entity_Id")
            )
        else:
            bots_deep_link = DeepLinkTabHelper.get_deep_link_tab_static("topic1", 1, "Bots", os.getenv("TeamsAppId"))
            messaging_deep_link = DeepLinkTabHelper.get_deep_link_tab_static("topic2", 2, "Messaging Extension", os.getenv("TeamsAppId"))
            adaptive_card_deep_link = DeepLinkTabHelper.get_deep_link_tab_static("topic3", 3, "Adaptive Card", os.getenv("TeamsAppId"))
            extended_deep_link = DeepLinkTabHelper.get_deep_link_tab_static("", 4, "Extended Deeplink features", os.getenv("TeamsAppId"))
            side_panel_link = DeepLinkTabHelper.get_deep_link_to_meeting_side_panel(
                5, "Side panel Deeplink", os.getenv("TeamsAppId"), os.getenv("Base_URL"),
                turn_context.activity.conversation.id, "chat"
            )

        adaptive_card = self.create_adaptive_card(
            conversation_type, bots_deep_link, messaging_deep_link, adaptive_card_deep_link, extended_deep_link, side_panel_link
        )

        await turn_context.send_activity(MessageFactory.attachment(adaptive_card))

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        welcome_text = "Hello and welcome!"
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(MessageFactory.text(welcome_text))

    def create_adaptive_card(self, conversation_type, bots_deep_link, messaging_deep_link, adaptive_card_deep_link, extended_deep_link, side_panel_link) -> Attachment:
        with open("resources/AdaptiveCard.json", "r", encoding="utf-8") as json_file:
            template_payload = json.load(json_file)

        # Define replacement data
        card_data = {
            "BotsDeepLink": bots_deep_link["linkUrl"],
            "MEDeepLink": messaging_deep_link["linkUrl"],
            "CardsDeepLink": adaptive_card_deep_link["linkUrl"],
            "BotsTitle": bots_deep_link["TaskText"],
            "METitle": messaging_deep_link["TaskText"],
            "CardsTitle": adaptive_card_deep_link["TaskText"],
            "ExtendedDeepLink": extended_deep_link["linkUrl"],
            "ExtendedDeepLinkTitle": extended_deep_link["TaskText"],
            "sidePanelLink": side_panel_link["linkUrl"] if side_panel_link else "",
            "sidePanelLinkTitle": side_panel_link["TaskText"] if side_panel_link else ""
        }

        # Convert template to string for manual replacement
        adaptive_card_json = json.dumps(template_payload)

        # Replace placeholders manually
        for key, value in card_data.items():
            adaptive_card_json = adaptive_card_json.replace(f"${{{key}}}", value)

        # Convert back to dictionary
        adaptive_card = json.loads(adaptive_card_json)

        return CardFactory.adaptive_card(adaptive_card)