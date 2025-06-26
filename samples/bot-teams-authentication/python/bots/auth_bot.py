# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from typing import List
from http import HTTPStatus
from botbuilder.core import TurnContext, ConversationState, UserState
from botbuilder.dialogs import Dialog
from botbuilder.schema import ChannelAccount, InvokeResponse

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

    # Welcome new members (equivalent to handleMembersAdded)
    async def on_members_added_activity(
        self, members_added: List[ChannelAccount], turn_context: TurnContext
    ):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Welcome to AuthenticationBot. Type anything to get logged in. Type "
                    "'logout' to sign-out."
                )

    # Handle 'signin/verifyState' (equivalent to handleTeamsSigninVerifyState)
    async def on_token_response_event(self, turn_context: TurnContext):
        print("🔑 Handling signin/verifyState")
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.dialog_state  # ✅ Use stored dialog state
        )

    # Handle 'signin/tokenExchange' (equivalent to handleTeamsSigninTokenExchange)
    async def on_teams_signin_token_exchange(self, turn_context: TurnContext):
        print("🔄 Handling signin/tokenExchange")
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.dialog_state  # ✅ Use stored dialog state
        )
        return InvokeResponse(status=HTTPStatus.OK)
