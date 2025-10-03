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
from .dialog_bot import DialogBot


class AuthBot(DialogBot):
    def __init__(
        self,
        conversation_state: ConversationState,
        user_state: UserState,
        dialog: Dialog,
    ):
        super(AuthBot, self).__init__(conversation_state, user_state, dialog)

    async def on_members_added_activity(
        self, members_added: List[ChannelAccount], turn_context: TurnContext
    ):
        # Handles new members added to the conversation and sends a welcome message.
        for member in members_added:
            # Greet anyone that was not the target (recipient) of this message.
            # To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Welcome to AuthenticationBot. Type anything to get logged in. Type "
                    "'logout' to sign-out."
                )

    async def on_token_response_event(self, turn_context: TurnContext):
        # Handles the token response event by continuing the dialog.
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.conversation_state.create_property("DialogState"),
        )
