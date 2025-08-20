# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core import TurnContext, MessageFactory
from cards import cards
from botbuilder.schema import HeroCard, CardAction, ActionTypes, Attachment
from config import DefaultConfig
CONFIG = DefaultConfig()

class BotAllCards(TeamsActivityHandler):
    def __init__(self):
        # Constructor for the TeamsBot class. Initializes the parent class (TeamsActivityHandler)
        super(BotAllCards, self).__init__()

    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        """
        Handles the event when new members are added to a team.
        """
        for member in teams_members_added:
            # Check if the added member is not the bot itself
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    " Welcome to Cards Bot!\n\n"
                    "This bot will introduce you to different types of cards.\n\n"
                    "Please select one from the options below."
                )

        # Send card options (HeroCard with buttons instead of SuggestedActions)
        await self.send_card_options(turn_context)

    async def on_message_activity(self, turn_context: TurnContext):
    # Normalize text input
     text = (turn_context.activity.text or "").strip()

     # Create a list with valid card options
     valid_cards = [
         "AdaptiveCard", "HeroCard", "ListCard", "Office365", "CollectionCard",
         "SignIn", "OAuth", "ThumbnailCard"
     ]

     # If text is in the list, send the corresponding card
     if text in valid_cards:
         attachment = None

         if text == "AdaptiveCard":
             attachment = cards.adaptive_Card()
         elif text == "HeroCard":
             attachment = cards.hero_Card()
         elif text == "Office365":
             attachment = cards.o365_Connector_Card()
         elif text == "ThumbnailCard":
             attachment = cards.thumbnail_Card()
         elif text == "SignIn":
             attachment = cards.signin_Card()
         elif text == "OAuth":
             attachment = cards.oauth_Card(CONFIG.CONNECTION_NAME)
         elif text == "ListCard":
             attachment = cards.list_Card()
         elif text == "CollectionCard":
             attachment = cards.collection_Card()

         if attachment:
             await turn_context.send_activity(Activity(attachments=[attachment]))

         await turn_context.send_activity(f"You have selected **{text}**")

     else:
         # If the input was not recognized, send a friendly message
         await turn_context.send_activity("Please choose one of the card types below.")

     # Always send card options again (outside the IF block)
     await self.send_card_options(turn_context)


    async def send_card_options(self, turn_context: TurnContext):
        """
        Sends a HeroCard with buttons instead of SuggestedActions (Teams limit is 3 for suggested actions).
        """
        card = HeroCard(
            title="Select a card type",
            text="Click a button below to view a card sample.",
            buttons=[
                CardAction(type=ActionTypes.im_back, title="AdaptiveCard", value="AdaptiveCard"),
                CardAction(type=ActionTypes.im_back, title="ListCard", value="ListCard"),
                CardAction(type=ActionTypes.im_back, title="HeroCard", value="HeroCard"),
                CardAction(type=ActionTypes.im_back, title="Office365", value="Office365"),
                CardAction(type=ActionTypes.im_back, title="CollectionCard", value="CollectionCard"),
                CardAction(type=ActionTypes.im_back, title="SignIn", value="SignIn"),
                CardAction(type=ActionTypes.im_back, title="ThumbnailCard", value="ThumbnailCard"),
                CardAction(type=ActionTypes.im_back, title="OAuth", value="OAuth"),
            ]
        )

        # In Python, wrap HeroCard into Attachment manually
        attachment = Attachment(
            content_type="application/vnd.microsoft.card.hero",
            content=card
        )

        reply = MessageFactory.attachment(attachment)
        await turn_context.send_activity(reply)
    