# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import ConversationState, UserState, TurnContext, MessageFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import InvokeResponse, Attachment
from helpers.dialog_helper import DialogHelper
from botbuilder.dialogs import Dialog
import logging
import traceback
import os
import aiohttp
import aiofiles

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
        # Handles message activities and manages dialog flow including specific commands.
        logging.info("on_message_activity triggered.")
        try:
            # Log the original activity text for debugging
            original_text = turn_context.activity.text or ""
            logging.info(f"Original activity text: '{original_text}'")
            
            # Remove mention text from activity
            cleaned_text = self._remove_mention_text(turn_context.activity)
            text = (cleaned_text.text or "").strip().lower()
            
            # Log the processed text for debugging
            logging.info(f"Processed text after mention removal: '{text}'")
            
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

            # Handle specific commands that should trigger the dialog
            # Check if any of the commands are present in the text
            if "getchat" in text or "logout" in text or "login" in text:
                logging.info(f"Command detected in text: '{text}'. Starting dialog.")
                await DialogHelper.run_dialog(
                    self.dialog,
                    turn_context,
                    dialog_state_accessor,
                )
            else:
                logging.info(f"No recognized command found in text: '{text}'")

        except Exception as e:
            logging.error(f"Error in on_message_activity: {e}")
            traceback.print_exc()
            await turn_context.send_activity("An error occurred handling your message.")

    def _remove_mention_text(self, activity):
        """
        Remove mention text from the activity to get clean command text.
        This handles both entity mentions and <at> tag mentions.
        """
        import re
        
        updated_activity = activity
        original_text = activity.text or ""
        
        # Log for debugging
        logging.info(f"Original text before mention removal: '{original_text}'")
        
        # First, try to remove mentions using entities
        cleaned_text = original_text
        if hasattr(activity, 'entities') and activity.entities:
            for entity in activity.entities:
                if hasattr(entity, 'type') and entity.type == "mention":
                    if hasattr(entity, 'text') and entity.text:
                        logging.info(f"Removing entity mention: '{entity.text}'")
                        cleaned_text = cleaned_text.replace(entity.text, "").strip()
                        break
        
        # Remove <at>...</at> patterns using regex
        at_pattern = r'<at>.*?</at>\s*'
        if re.search(at_pattern, cleaned_text):
            logging.info(f"Found <at> pattern in text: '{cleaned_text}'")
            cleaned_text = re.sub(at_pattern, '', cleaned_text).strip()
            logging.info(f"Text after removing <at> pattern: '{cleaned_text}'")
        
        updated_activity.text = cleaned_text
        return updated_activity

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

    async def on_teams_file_consent_accept(self, turn_context: TurnContext, file_consent_card_response):
        """
        Invoked when a file consent card activity is accepted.
        """
        try:
            file_path = os.path.join("public", file_consent_card_response.context["filename"])
            
            if not os.path.exists(file_path):
                await self._file_upload_failed(turn_context, "File not found")
                return

            # Read file content
            async with aiofiles.open(file_path, 'rb') as file:
                file_content = await file.read()

            file_size = os.path.getsize(file_path)
            
            # Upload file to Teams
            headers = {
                'Content-Type': 'text/plain',
                'Content-Length': str(file_size),
                'Content-Range': f'bytes 0-{file_size - 1}/{file_size}'
            }

            async with aiohttp.ClientSession() as session:
                async with session.put(
                    file_consent_card_response.upload_info.upload_url,
                    data=file_content,
                    headers=headers
                ) as response:
                    if response.status == 200 or response.status == 201:
                        await self._file_upload_completed(turn_context, file_consent_card_response)
                    else:
                        await self._file_upload_failed(turn_context, f"Upload failed with status {response.status}")

        except Exception as e:
            await self._file_upload_failed(turn_context, str(e))

    async def on_teams_file_consent_decline(self, turn_context: TurnContext, file_consent_card_response):
        """
        Invoked when a file consent card is declined by the user.
        """
        filename = file_consent_card_response.context.get("filename", "file")
        reply = MessageFactory.text(f"Declined. We won't upload file <b>{filename}</b>.")
        reply.text_format = "xml"
        await turn_context.send_activity(reply)

    async def _file_upload_completed(self, turn_context: TurnContext, file_consent_card_response):
        """Handle successful file upload."""
        download_card = {
            "uniqueId": file_consent_card_response.upload_info.unique_id,
            "fileType": file_consent_card_response.upload_info.file_type
        }

        attachment = Attachment(
            content=download_card,
            content_type="application/vnd.microsoft.teams.card.file.info",
            name=file_consent_card_response.upload_info.name,
            content_url=file_consent_card_response.upload_info.content_url
        )

        reply = MessageFactory.text(
            f"<b>File uploaded.</b> Your file <b>{file_consent_card_response.upload_info.name}</b> is ready to download"
        )
        reply.text_format = "xml"
        reply.attachments = [attachment]

        await turn_context.send_activity(reply)

    async def _file_upload_failed(self, turn_context: TurnContext, error: str):
        """Handle file upload failure."""
        reply = MessageFactory.text(f"<b>File upload failed.</b> Error: <pre>{error}</pre>")
        reply.text_format = "xml"
        await turn_context.send_activity(reply)
