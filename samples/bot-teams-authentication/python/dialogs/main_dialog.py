# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory
from botbuilder.dialogs import WaterfallDialog, WaterfallStepContext, DialogTurnResult, PromptOptions
from botbuilder.dialogs.prompts import ConfirmPrompt, OAuthPromptSettings
from custom_oauth_prompt import OAuthPrompt  # Custom wrapper with bug fix
from dialogs import LogoutDialog
import logging

# Set logging
logging.basicConfig(level=logging.INFO)

class MainDialog(LogoutDialog):
    def __init__(self, connection_name: str):
        super(MainDialog, self).__init__(MainDialog.__name__, connection_name)
        self._connection_name = connection_name

        # Add OAuthPrompt for Teams SSO
        self.add_dialog(
            OAuthPrompt(
                OAuthPrompt.__name__,
                OAuthPromptSettings(
                    connection_name=connection_name,
                    text="Please sign in to continue.",
                    title="Sign In",
                    timeout=300000,  # 5 minutes
                ),
            )
        )

        # Add ConfirmPrompt
        self.add_dialog(ConfirmPrompt(ConfirmPrompt.__name__))

        # Waterfall dialog steps
        self.add_dialog(
            WaterfallDialog(
                "WFDialog",
                [
                    self.login_step,
                    self.display_token_phase1,
                    self.display_token_phase2,
                ],
            )
        )

        self.initial_dialog_id = "WFDialog"

    # Step 1: Attempt to get SSO token using OAuthPrompt
    async def login_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        # Use OAuthPrompt to get the token (handles SSO and fallback signin)
        return await step_context.begin_dialog(OAuthPrompt.__name__)

    # Step 2: Process the token response
    async def display_token_phase1(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        # The result from OAuthPrompt is the TokenResponse
        token_response = step_context.result
        
        # Check if we got a valid token response (not an error)
        if token_response and hasattr(token_response, 'token') and token_response.token:
            logging.info("Successfully retrieved token via OAuthPrompt")
            # Store token in step values for later use
            step_context.values["token"] = token_response.token
            await step_context.context.send_activity("You are now logged in!")
            return await step_context.prompt(
                ConfirmPrompt.__name__,
                PromptOptions(
                    prompt=MessageFactory.text("Would you like to view your token?")
                ),
            )
        else:
            logging.warning(f"Token response was None or invalid. Response type: {type(token_response)}")
            await step_context.context.send_activity(
                "Login was not successful. Please try again."
            )
            return await step_context.end_dialog()

    # Step 3: Display token to user
    async def display_token_phase2(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        if step_context.result:
            # Retrieve the token from step values
            token = step_context.values.get("token")
            
            if token:
                await step_context.context.send_activity(
                    f"Here is your token: {token}"
                )
            else:
                await step_context.context.send_activity(
                    "Token not available."
                )
        else:
            await step_context.context.send_activity("Thank you.")

        return await step_context.end_dialog()
