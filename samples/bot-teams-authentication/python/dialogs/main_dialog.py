# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory
from botbuilder.dialogs import (
    WaterfallDialog,
    WaterfallStepContext,
    DialogTurnResult,
    PromptOptions,
)
from botbuilder.dialogs.prompts import OAuthPrompt, OAuthPromptSettings, ConfirmPrompt

from dialogs import LogoutDialog
import logging
# Set the logging level to INFO
logging.basicConfig(level=logging.INFO)

class MainDialog(LogoutDialog):
    def __init__(self, connection_name: str):
        # Initializes the MainDialog with OAuthPrompt, ConfirmPrompt, and WaterfallDialog.
        super(MainDialog, self).__init__(MainDialog.__name__, connection_name)

        self.add_dialog(
            OAuthPrompt(
                OAuthPrompt.__name__,
                OAuthPromptSettings(
                    connection_name=connection_name,
                    text="Please Sign In",
                    title="Sign In",
                    timeout=300000
                )
            )
        )

        self.add_dialog(ConfirmPrompt(ConfirmPrompt.__name__))

        self.add_dialog(
            WaterfallDialog(
                "WFDialog",
                [
                    self.prompt_step,
                    self.login_step,
                    self.display_token_phase1,
                    self.display_token_phase2,
                ],
            )
        )

        self.initial_dialog_id = "WFDialog"

    # Prompts the user to sign in using the OAuthPrompt.
    async def prompt_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        logging.info("Reached OAuthPrompt step — prompting for login.")
        return await step_context.begin_dialog(OAuthPrompt.__name__)

    # Handles the login result and optionally prompts to display the token.
    async def login_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        if step_context.result and step_context.result.token:
            logging.info("Login Step Reached — Successfully Logged In")
            await step_context.context.send_activity("You are now logged in.")
            return await step_context.prompt(
                ConfirmPrompt.__name__,
                PromptOptions(
                    prompt=MessageFactory.text("Would you like to view your token?")
                ),
            )

        # Handles invalid actions or cancelled login
        await step_context.context.send_activity(
            "Login was not successful or was cancelled. Let's try again."
        )
        return await step_context.replace_dialog(self.initial_dialog_id)

    # Re-prompts for OAuth token if the user wants to view it.
    async def display_token_phase1(
        self, step_context: WaterfallStepContext
    ) -> DialogTurnResult:
        await step_context.context.send_activity("Thank you.")

        if step_context.result:
            # Call the prompt again because we need the token. The reasons for this are:
            # 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            #    about refreshing it. We can always just call the prompt again to get the token.
            # 2. We never know how long it will take a user to respond. By the time the
            #    user responds the token may have expired. The user would then be prompted to login again.
            #
            # There is no reason to store the token locally in the bot because we can always just call
            # the OAuth prompt to get the token or get a new token if needed.
            return await step_context.begin_dialog(OAuthPrompt.__name__)

        return await step_context.end_dialog()

    # Displays the token to the user if they confirmed.
    async def display_token_phase2(
        self, step_context: WaterfallStepContext
    ) -> DialogTurnResult:
        if step_context.result:
            await step_context.context.send_activity(
                f"Here is your token {step_context.result.token}"
            )

        return await step_context.end_dialog()
