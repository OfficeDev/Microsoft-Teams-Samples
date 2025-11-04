# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.dialogs import DialogTurnResult, ComponentDialog, DialogContext
from botbuilder.core import BotFrameworkAdapter
from botbuilder.core.cloud_adapter_base import CloudAdapterBase
from botbuilder.schema import ActivityTypes
import logging

class LogoutDialog(ComponentDialog):
    def __init__(self, dialog_id: str, connection_name: str):
        # Initializes the LogoutDialog with a dialog ID and OAuth connection name.
        super(LogoutDialog, self).__init__(dialog_id)

        self.connection_name = connection_name

    async def on_begin_dialog(self, inner_dc: DialogContext, options: object) -> DialogTurnResult:
        # Intercepts the dialog at the beginning to check for logout command.
        result = await self._interrupt(inner_dc)
        if result:
            return result
        return await super().on_begin_dialog(inner_dc, options)

    async def on_continue_dialog(self, inner_dc: DialogContext) -> DialogTurnResult:
        # Intercepts the dialog while it is running to check for logout command.
        result = await self._interrupt(inner_dc)
        if result:
            return result
        return await super().on_continue_dialog(inner_dc)

    async def _interrupt(self, inner_dc: DialogContext):
        # Checks for the 'logout' command and signs the user out if detected.
        if inner_dc.context.activity.type == ActivityTypes.message:
            text = inner_dc.context.activity.text.lower()
            if text == "logout":
                # Use UserTokenClient for CloudAdapter
                adapter = inner_dc.context.adapter
                
                # Try BotFrameworkAdapter method first (for compatibility)
                if hasattr(adapter, 'sign_out_user'):
                    await adapter.sign_out_user(inner_dc.context, self.connection_name)
                else:
                    # CloudAdapter uses UserTokenClient
                    token_client = inner_dc.context.turn_state.get(CloudAdapterBase.USER_TOKEN_CLIENT_KEY)
                    if token_client:
                        try:
                            await token_client.sign_out_user(
                                inner_dc.context.activity.from_property.id,
                                self.connection_name,
                                inner_dc.context.activity.channel_id
                            )
                        except Exception as ex:
                            logging.error(f"Error signing out user: {ex}")
                    else:
                        logging.warning("UserTokenClient not found in turn state; cannot sign out.")
                
                await inner_dc.context.send_activity("You have been signed out.")
                return await inner_dc.cancel_all_dialogs()
