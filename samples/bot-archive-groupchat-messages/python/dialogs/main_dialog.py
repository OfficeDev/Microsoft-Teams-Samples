# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import aiofiles
from typing import Dict, Any
from botbuilder.core import MessageFactory, TurnContext
from botbuilder.dialogs import (
    WaterfallDialog,
    WaterfallStepContext,
    DialogTurnResult,
)
from botbuilder.dialogs.prompts import OAuthPrompt, OAuthPromptSettings
from botbuilder.schema import Activity, Attachment, ActivityTypes, ChannelAccount
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from simple_graph_client import SimpleGraphClient

from dialogs import LogoutDialog
import logging

# Set the logging level to INFO
logging.basicConfig(level=logging.INFO)

class MainDialog(LogoutDialog):
    def __init__(self, connection_name: str):
        # Initializes the MainDialog with OAuthPrompt and WaterfallDialog.
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

        self.add_dialog(
            WaterfallDialog(
                "WFDialog",
                [
                    self.prompt_step,
                    self.login_step,
                ],
            )
        )

        self.initial_dialog_id = "WFDialog"

    # Prompts the user to sign in using the OAuthPrompt.
    async def prompt_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        logging.info("Reached OAuthPrompt step — prompting for login.")
        return await step_context.begin_dialog(OAuthPrompt.__name__)

    # Handles the login result and processes group chat or personal conversation.
    async def login_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        # Get the token from the previous step
        token_response = step_context.result

        if not token_response or not token_response.token:
            await step_context.context.send_activity("Login was not successful please try again.")
        else:
            # Check if this is a group chat (not personal conversation)
            if step_context.context.activity.conversation.conversation_type != "personal":
                try:
                    client = SimpleGraphClient(token_response.token)
                    messages_response = await client.get_messages(
                        step_context.context.activity.conversation.id
                    )
                    await self._create_file(messages_response.get("value", []))
                    await self._send_file_consent_card_async(step_context.context)
                    return await step_context.end_dialog()
                except Exception as e:
                    logging.error(f"Error fetching messages: {e}")
                    await step_context.context.send_activity(
                        "Error occurred while fetching chat messages. Please try again."
                    )
            else:
                await step_context.context.send_activity("Login successful")
                await step_context.context.send_activity(
                    "Please type 'getchat' command in the groupchat where the bot is added to fetch messages."
                )

        return await step_context.end_dialog()

    async def _create_file(self, messages: list):
        """Create archive messages file."""
        try:
            file_path = "public/chat.txt"
            
            # Ensure public directory exists
            os.makedirs(os.path.dirname(file_path), exist_ok=True)
            
            # Clear existing content
            async with aiofiles.open(file_path, 'w', encoding='utf-8') as file:
                await file.write("")
            
            # Write messages to file
            async with aiofiles.open(file_path, 'a', encoding='utf-8') as file:
                for message in messages:
                    if message.get("messageType") == "message":
                        from_user = message.get("from", {}).get("user")
                        from_app = message.get("from", {}).get("application")
                        display_name = (
                            from_user.get("displayName") if from_user 
                            else from_app.get("displayName") if from_app 
                            else "Unknown"
                        )
                        
                        body_content = message.get("body", {}).get("content", "")
                        timestamp = message.get("lastModifiedDateTime", "")
                        
                        await file.write(f"from:{display_name}\n")
                        await file.write(f"message:{body_content}\n")
                        await file.write(f"at:{timestamp}\n\n")
                        
        except Exception as e:
            logging.error(f"Error creating file: {e}")

    async def _send_file_consent_card_async(self, context: TurnContext):
        """Send file consent card."""
        try:
            filename = "chat.txt"
            file_path = f"public/{filename}"
            
            # Get file size
            file_size = os.path.getsize(file_path) if os.path.exists(file_path) else 0
            
            member = {
                "aadObjectId": context.activity.from_property.aad_object_id,
                "name": context.activity.from_property.name,
                "id": context.activity.from_property.id
            }
            
            message = await self._send_file_card(context, filename, file_size)
            
            # In a real implementation, you would create a conversation with the specific member
            # For now, just send the file consent card to the current context
            await context.send_activity(message)
            
        except Exception as e:
            logging.error(f"Error sending file consent card: {e}")

    async def _send_file_card(self, context: TurnContext, filename: str, filesize: int) -> Activity:
        """Create file consent card."""
        consent_context = {"filename": filename}
        
        file_card = {
            "description": "This is the file I want to send you",
            "sizeInBytes": filesize,
            "acceptContext": consent_context,
            "declineContext": consent_context
        }
        
        attachment = Attachment(
            content=file_card,
            content_type="application/vnd.microsoft.teams.card.file.consent",
            name=filename
        )
        
        return MessageFactory.attachment(attachment)
