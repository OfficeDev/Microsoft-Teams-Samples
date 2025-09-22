# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from typing import List
from botbuilder.core import (
    ConversationState,
    UserState,
    TurnContext,
)
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
        dialog: Dialog,
    ):
        # Initializes the TeamsBot with conversation and user state, and main dialog.
        super(TeamsBot, self).__init__(conversation_state, user_state, dialog)

    # Sends a welcome message to new members added to the conversation.
    async def on_members_added_activity(
        self, members_added: List[ChannelAccount], turn_context: TurnContext
    ):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Welcome to TeamsBot. Type anything to get logged in. Type 'logout' to sign-out."
                )

    # Handles the Teams signin/verifyState invoke activity and continues the dialog.
    async def on_teams_signin_verify_state(self, turn_context: TurnContext):
        logging.info("Running dialog with signin/verifyState from an Invoke Activity.")
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )
        return InvokeResponse(status=200)

    # Handles the Teams signin/tokenExchange invoke activity and continues the dialog.
    async def on_teams_signin_token_exchange(self, turn_context: TurnContext):
        logging.info("Running dialog with signin/tokenExchange from an Invoke Activity.")
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )
        return InvokeResponse(status=200)
