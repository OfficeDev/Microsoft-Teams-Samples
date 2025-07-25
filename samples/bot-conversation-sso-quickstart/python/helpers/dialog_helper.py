# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import logging
from botbuilder.core import StatePropertyAccessor, TurnContext
from botbuilder.dialogs import Dialog, DialogSet, DialogTurnStatus
from botbuilder.schema import InvokeResponse

# Set the logging level to INFO
logging.basicConfig(level=logging.INFO)

class DialogHelper:
    # Runs the provided dialog, handling both normal and OAuth invoke activities.
    @staticmethod
    async def run_dialog(
        dialog: Dialog, turn_context: TurnContext, accessor: StatePropertyAccessor
    ):
        try:
            logging.info("Initializing DialogSet and adding dialog.")
            dialog_set = DialogSet(accessor)
            dialog_set.add(dialog)

            dialog_context = await dialog_set.create_context(turn_context)

            logging.info("Continuing any existing dialog.")
            results = await dialog_context.continue_dialog()

            if results.status == DialogTurnStatus.Empty:
                logging.info(f"No active dialog. Starting new dialog: {dialog.id}")
                await dialog_context.begin_dialog(dialog.id)
            else:
                logging.info(f"Continuing dialog. Status: {results.status.name}")

            # Special handling for OAuth invoke activities
            if turn_context.activity.name in ["signin/tokenExchange", "signin/verifyState"]:
                logging.info("OAuth prompt token exchange or verifyState handled.")
                return InvokeResponse(status=200)

        except AttributeError as attr_error:
            # Handle the specific "'ErrorResponse' object has no attribute 'token'" error
            if "has no attribute 'token'" in str(attr_error):
                logging.error(f"OAuth token error handled: {attr_error}")
                # Send a friendly message to the user
                await turn_context.send_activity("Authentication failed. Please try signing in again.")
                # Return successful invoke response to prevent framework errors
                if turn_context.activity.name in ["signin/tokenExchange", "signin/verifyState"]:
                    return InvokeResponse(status=200)
            else:
                # Re-raise other AttributeError exceptions
                raise
        except Exception as e:
            logging.error(f"Error running dialog: {e}")
            import traceback
            traceback.print_exc()
            await turn_context.send_activity("Sorry, something went wrong starting the dialog.")