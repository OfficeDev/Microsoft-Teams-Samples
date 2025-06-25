import os
from botbuilder.core import TurnContext, MessageFactory
from botbuilder.schema import Activity, ActivityTypes
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema.teams import AppBasedLinkQuery, MessagingExtensionResult, MessagingExtensionAttachment, MessagingExtensionResponse
from botbuilder.schema import Attachment

from models import adaptive_card, adaptive_card_unfurling


class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_message_activity(self, turn_context: TurnContext):
        base_url = os.getenv("BaseUrl", "")
        base_url = base_url.split("://")[-1] if "://" in base_url else base_url

        card = adaptive_card.adaptive_card_for_tab_stage_view(base_url)
        attachment = Attachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=card
        )

        await turn_context.send_activity(Activity(
            type=ActivityTypes.message,
            attachments=[attachment]
        ))

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        welcome_text = "Hello and welcome! Please type any bot command to see the stage view feature"
        await turn_context.send_activity(MessageFactory.text(welcome_text))

    async def on_teams_app_based_link_query(self, turn_context: TurnContext, query: AppBasedLinkQuery):
        card = adaptive_card_unfurling.adaptive_card_for_tab_stage_view()
        attachment = MessagingExtensionAttachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=card
        )

        result = MessagingExtensionResult(
            type="result",
            attachment_layout="list",
            attachments=[attachment]
        )

        return MessagingExtensionResponse(compose_extension=result)
