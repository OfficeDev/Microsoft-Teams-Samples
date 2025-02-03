# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core import TurnContext,MessageFactory
from cards import cards
from botbuilder.schema import SuggestedActions, CardAction, ActionTypes

class BotAllCards(TeamsActivityHandler):
    def __init__(self):
        # Constructor for the TeamsBot class. Initializes the parent class (TeamsActivityHandler)
        super(BotAllCards,self).__init__()

    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        """
        Handles the event when new members are added to a team.
        
        Args:
            teams_members_added: List of members added to the team.
            team_info: Information about the team where the members were added.
            turn_context: Context object for the current turn of the conversation.
        
        Behavior:
            Sends a welcome message to newly added members, excluding the bot itself.
        """
        for member in teams_members_added:
            # Check if the added member is not the bot itself
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    f"Welcome to Cards. This bot will introduce you to different types of cards. Please select the cards from given options"
                    f"Please select an option:"
                )
                
        await self.send_suggested_actions(turn_context)     

    async def on_message_activity(self, turn_context: TurnContext):
        
        text = turn_context.activity.text
        
        # Create a list with valid card options
        suggested_cards = [
            "AdaptiveCard", "HeroCard", "ListCard", "Office365", "CollectionCard",
            "SignIn", "OAuth", "ThumbnailCard"
        ]
        
        # If text is in the list, send the corresponding card
        if text in suggested_cards:
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
                attachment = cards.oauth_Card()
            elif text == "ListCard":
                attachment = cards.list_Card()
            elif text == "CollectionCard":
                attachment = cards.collection_Card()
            
            if attachment:
                await turn_context.send_activity(Activity(attachments=[attachment]))
            
            await turn_context.send_activity(f"You have Selected <b>{text}</b>")
            
            await self.send_suggested_actions(turn_context)
            
            
    async def send_suggested_actions(self, turn_context: TurnContext):
        actions = SuggestedActions(
            actions=[
                CardAction(type=ActionTypes.im_back, title="AdaptiveCard", value="AdaptiveCard"),
                CardAction(type=ActionTypes.im_back, title="ListCard", value="ListCard"),
                CardAction(type=ActionTypes.im_back, title="HeroCard", value="HeroCard"),
                CardAction(type=ActionTypes.im_back, title="Office365", value="Office365"),
                CardAction(type=ActionTypes.im_back, title="CollectionCard", value="CollectionCard"),
                CardAction(type=ActionTypes.im_back, title="SignIn", value="SignIn"),
                CardAction(type=ActionTypes.im_back, title="ThumbnailCard", value="ThumbnailCard"),
                CardAction(type=ActionTypes.im_back, title="OAuth", value="OAuth"),
                CardAction(type=ActionTypes.im_back, title="Office365", value="Office365")
            ]
        )
        reply = MessageFactory.text("Please select the card from the given suggested actions")
        reply.suggested_actions = actions
        await turn_context.send_activity(reply)
