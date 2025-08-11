# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from typing import List
from botbuilder.core import(
    ConversationState,
    UserState,
    TurnContext,
    MessageFactory,
    CardFactory
)
from botbuilder.schema import ActionTypes, CardAction
from botbuilder.dialogs import Dialog
from botbuilder.schema import ChannelAccount
from helpers.dialog_helper import DialogHelper
from botbuilder.schema import InvokeResponse
from bots.dialog_bot import DialogBot
import logging


class TeamsBot(DialogBot):
    def __init__(
        self,
        conversation_state: ConversationState,
        user_state: UserState,
        dialog: Dialog
    ):
        # Initializes the TeamsBot with conversation and user state, and main dialog.
        super(TeamsBot, self).__init__(conversation_state, user_state, dialog)
    
    async def on_message_activity(self, turn_context: TurnContext):
        # Handle specific commands before passing to dialog
        TurnContext.remove_recipient_mention(turn_context.activity)
        text = (turn_context.activity.text or "").strip().lower()
        
        if text == 'hello':
            await self.mention_activity(turn_context)
            return
        elif text == 'clear':
            # Handle clear command in parent class
            await super().on_message_activity(turn_context)
            return
        elif text == '':
            # Show default card when no text is provided
            await self.show_welcome_card(turn_context)
            return
        elif text == 'start tag mention':
            # Start the tag mention dialog flow
            await DialogHelper.run_dialog(
                self.dialog,
                turn_context,
                self.conversation_state.create_property("DialogState"),
            )
            return
        else:
            # For any other message, run the dialog (tag mention functionality)
            await DialogHelper.run_dialog(
                self.dialog,
                turn_context,
                self.conversation_state.create_property("DialogState"),
            )

    async def mention_activity(self, turn_context: TurnContext):
        """Say hello and @ mention the current user (Python equivalent of Node.js mentionActivityAsync)"""
        try:
            from_user = turn_context.activity.from_property
            
            # Create mention entity
            mention = {
                "mentioned": {
                    "id": from_user.id,
                    "name": from_user.name
                },
                "text": f"<at>{from_user.name}</at>",
                "type": "mention"
            }
            
            # Create reply with mention
            reply_text = f"Hi {mention['text']}"
            reply_activity = MessageFactory.text(reply_text)
            reply_activity.entities = [mention]
            
            await turn_context.send_activity(reply_activity)
            
        except Exception as e:
            logging.error(f"Error in mention_activity: {e}")
            await turn_context.send_activity("Hello!")

    async def show_welcome_card(self, turn_context: TurnContext):
        """Show welcome card with action buttons (Python equivalent of Node.js default case)"""
        try:
            # Create hero card with action button
            card = CardFactory.hero_card(
                title="Let's talk...",
                subtitle="Choose an action below:",
                images=[],
                buttons=[
                    CardAction(
                        type=ActionTypes.message_back,
                        title="Say Hello",
                        value={"count": 0},
                        text="Hello"
                    ),
                    CardAction(
                        type=ActionTypes.message_back,
                        title="Tag Mention",
                        value={"count": 0},
                        text="Start tag mention"
                    )
                ]
            )
            
            card_activity = MessageFactory.attachment(card)
            await turn_context.send_activity(card_activity)
            
        except Exception as e:
            logging.error(f"Error in show_welcome_card: {e}")
            await turn_context.send_activity("Welcome! Type 'Hello' to get started.")
    
    # Sends welcome message to new members added to the conversation.
    async def on_members_added(
            self, members_added: List[ChannelAccount], turn_context: TurnContext
    ):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    f"Welcome to the team {member.given_name} {member.surname}."
                )
                # Show welcome card to new members
                await self.show_welcome_card(turn_context)

    async def on_token_response_event(self, turn_context: TurnContext):
        # Handles the token response event by continuing the dialog.
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )

    async def on_installation_update_activity(self, turn_context: TurnContext):
        """Handle app installation update activity - matches Node.js onInstallationUpdateActivity"""
        try:
            if turn_context.activity.conversation.conversation_type == 'channel':
                welcome_msg = ("Welcome to Tag mention Teams bot app. Please follow the below commands for mentioning the tags:\r\n\r\n\r\n"
                              "1. Command: \"`@<Bot-name> <your-tag-name>`\" - It will work only if you have Graph API permissions to fetch the tags "
                              "and bot will mention the tag accordingly in team's channel scope.\r\n\r\n\r\n"
                              "2. Command \"`@<Bot-name> @<your-tag>`\" - It will work without Graph API permissions but you need to provide the tag "
                              "as command to experience tag mention using bot.")
                await turn_context.send_activity(welcome_msg)
            else:
                await turn_context.send_activity('Welcome to Tag mention demo bot. Type anything to get logged in. Type \'logout\' to sign-out')
        except Exception as e:
            logging.error(f"Error in on_installation_update_activity: {e}")

    async def on_teams_signin_verify_state(self, turn_context: TurnContext, query):
        """Handle Teams signin verify state - matches Node.js handleTeamsSigninVerifyState"""
        logging.info('Running dialog with signin/verifystate from an Invoke Activity.')
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )

    async def on_teams_signin_token_exchange(self, turn_context: TurnContext, query):
        """Handle Teams signin token exchange - matches Node.js handleTeamsSigninTokenExchange"""
        logging.info('Running dialog with signin/tokenExchange from an Invoke Activity.')
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )