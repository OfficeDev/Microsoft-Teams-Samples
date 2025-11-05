# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import ConversationState, UserState, TurnContext
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import InvokeResponse
from helpers.dialog_helper import DialogHelper
from botbuilder.dialogs import Dialog
import logging
import traceback

class DialogBot(TeamsActivityHandler):
    def __init__(
        self,
        conversation_state: ConversationState,
        user_state: UserState,
        dialog: Dialog,
    ):
        # Initializes the DialogBot with conversation state, user state, and main dialog.
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

    async def on_turn(self, turn_context: TurnContext):
        # Handles every turn of the bot and saves any state changes.
        try:
            await super().on_turn(turn_context)

            # Save any state changes that might have occurred during the turn.
            await self.conversation_state.save_changes(turn_context, False)
            await self.user_state.save_changes(turn_context, False)

        except Exception as e:
            logging.error(f"Error in on_turn: {e}")
            traceback.print_exc()
            await turn_context.send_activity("Sorry, something went wrong processing your message.")
            
    async def on_message_activity(self, turn_context: TurnContext):
        # Handles message activities and manages dialog flow including 'clear' command.
        logging.info("on_message_activity triggered.")
        try:
            text = (turn_context.activity.text or "").strip().lower()
            dialog_state_accessor = self.conversation_state.create_property("DialogState")

            if text == "clear":
                logging.info("'clear' command received. Attempting to reset dialog state.")
                dialog_state = await dialog_state_accessor.get(turn_context)

                if dialog_state:
                    await dialog_state_accessor.delete(turn_context)
                    logging.info("Dialog state deleted.")
                    await turn_context.send_activity("Dialog state cleared. You can start again.")
                else:
                    logging.info("No dialog state found to delete.")
                    await turn_context.send_activity("No active dialog state found to clear.")
                return

            await DialogHelper.run_dialog(
                self.dialog,
                turn_context,
                dialog_state_accessor,
            )

        except Exception as e:
            logging.error(f"Error in on_message_activity: {e}")
            traceback.print_exc()
            await turn_context.send_activity("An error occurred handling your message.")

    async def on_invoke_activity(self, turn_context: TurnContext):
        # Handles invoke activities such as token exchange or state verification.
        try:
            logging.info(f"Incoming activity type: {turn_context.activity.type}")
            logging.info(f"Activity name: {turn_context.activity.name}")
    
            if turn_context.activity.name == "signin/tokenExchange":
                logging.info("Handling signin/tokenExchange")
    
                # Token exchange success, continue with dialog
                await DialogHelper.run_dialog(
                    self.dialog,
                    turn_context,
                    self.conversation_state.create_property("DialogState"),
                )
                return InvokeResponse(status=200)
    
            elif turn_context.activity.name == "signin/verifyState":
                logging.info("Handling signin/verifyState")
    
                await DialogHelper.run_dialog(
                    self.dialog,
                    turn_context,
                    self.conversation_state.create_property("DialogState"),
                )
    
                return InvokeResponse(status=200)
    
            return await super().on_invoke_activity(turn_context)
    
        except Exception as e:
            logging.error(f"Error in on_invoke_activity: {e}")
            traceback.print_exc()
            return InvokeResponse(status=500)
