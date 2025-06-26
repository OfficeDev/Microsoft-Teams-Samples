# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import ActivityHandler, ConversationState, UserState, TurnContext
from botbuilder.dialogs import Dialog
from helpers.dialog_helper import DialogHelper


class DialogBot(ActivityHandler):
    def __init__(
        self,
        conversation_state: ConversationState,
        user_state: UserState,
        dialog: Dialog,
    ):
        if conversation_state is None:
            raise Exception(
                "[DialogBot]: Missing parameter. conversation_state is required"
            )
        if user_state is None:
            raise Exception("[DialogBot]: Missing parameter. user_state is required")
        if dialog is None:
            raise Exception("[DialogBot]: Missing parameter. dialog is required")

        self.conversation_state = conversation_state
        self.user_state = user_state
        self.dialog = dialog
        
        # ✅ Create and store dialog state once here
        self.dialog_state = self.conversation_state.create_property("DialogState")

    async def on_turn(self, turn_context: TurnContext):
        await super().on_turn(turn_context)

        # 🛡️ Defensive check before saving state
        if (
            turn_context.activity is not None
            and turn_context.activity.conversation is not None
            and turn_context.activity.channel_id
        ):
            await self.conversation_state.save_changes(turn_context, False)
            await self.user_state.save_changes(turn_context, False)
        else:
            print("⚠️ Skipping state save due to missing conversation or channel_id.")

    async def on_message_activity(self, turn_context: TurnContext):
        await DialogHelper.run_dialog(
            self.dialog,
            turn_context,
            self.dialog_state  # ✅ Reuse the dialog_state defined in __init__
            #self.conversation_state.create_property("DialogState"),
        )
