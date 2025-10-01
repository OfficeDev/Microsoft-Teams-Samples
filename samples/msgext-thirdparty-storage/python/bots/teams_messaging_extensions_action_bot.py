# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext
from botbuilder.core.card_factory import CardFactory
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionActionResponse,
    TaskModuleContinueResponse,
    TaskModuleResponse,
    TaskModuleTaskInfo,
    MessagingExtensionResult,
    TaskModuleMessageResponse,
)
from botbuilder.core.teams import TeamsActivityHandler
from config import DefaultConfig


class TeamsMessagingExtensionsActionBot(TeamsActivityHandler):
    def __init__(self, config: DefaultConfig):
        # Initialize the bot with the base URL from the config
        super().__init__()
        self.__base_url = config.BASE_URL

    def get_file_icon(self, file_name: str) -> str:
        """
        Returns the appropriate icon URL based on the file extension.
        """
        # Check for specific file extensions and return corresponding icon
        if file_name.endswith(".pdf"):
            return f"{self.__base_url}/icons/PDFIcons.png"
        elif file_name.endswith(".doc") or file_name.endswith(".docx"):
            return f"{self.__base_url}/icons/WordIcons.png"
        elif file_name.endswith(".xls") or file_name.endswith(".xlsx"):
            return f"{self.__base_url}/icons/Excel_Icons.png"
        elif file_name.endswith(".png"):
            return f"{self.__base_url}/icons/ImageIcon.png"
        elif file_name.endswith(".jpg") or file_name.endswith(".jpeg"):
            return f"{self.__base_url}/icons/ImageIcon.png"
        else:
            return f"{self.__base_url}/icons/DefaultIcon.png"

    async def on_teams_messaging_extension_submit_action_dispatch(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        """
        Handles the submit action from a messaging extension.
        This method processes the list of files submitted by the user,
        generates a dynamic Adaptive Card, and returns the result as an attachment.
        """
        user_input = action.data

        # Check if user input is a list (expecting an array of files)
        if not isinstance(user_input, list):
            return MessagingExtensionActionResponse(
                compose_extension=MessagingExtensionResult(
                    type="message",
                    text="Invalid input: Expected an array of files.",
                )
            )
        
        # Generate Adaptive Card dynamically
        body = []
        for file in user_input:
            file_name = file.get("name", "Unknown File")  # Get the file name or default to "Unknown File"
            icon_url = self.get_file_icon(file_name)  # Get the corresponding file icon URL
            body.append(
                {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "auto",
                            "items": [
                                {
                                    "type": "Image",
                                    "url": icon_url,  # Use the dynamically generated icon URL
                                    "size": "Small",
                                }
                            ],
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": file_name,
                                    "wrap": True,
                                    "weight": "Default",
                                    "size": "Medium",
                                },
                            ],
                        },
                    ],
                }
            )
        
        # Construct the Adaptive Card
        adaptive_card = {
            "type": "AdaptiveCard",
            "version": "1.4",
            "body": body,
        }
        
        # Create the card attachment
        card_attachment = CardFactory.adaptive_card(adaptive_card)
        
        return MessagingExtensionActionResponse(
            compose_extension=MessagingExtensionResult(
                type="result",
                attachment_layout="list",  # Attach the generated Adaptive Card as a list item
                attachments=[card_attachment],  # Directly use the Attachment object
            )
        )

    async def on_teams_messaging_extension_fetch_task(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> TaskModuleResponse:
        """
        Fetches a task module and returns the URL for a custom form in the WebView.
        """
        # Check the conditions for displaying the task module
        if action.message_payload.reply_to_id == '' and action.command_context == 'thirdParty':
            task_info = TaskModuleTaskInfo(
                width=700,  # Set the width of the task module
                height=450,  # Set the height of the task module
                title="Task Module WebView",  # Set the title of the task module
                url=f"{self.__base_url}/customForm",  # Set the URL for the custom form
            )

            # Return the task module response to continue with the task module view
            return TaskModuleResponse(
                task=TaskModuleContinueResponse(value=task_info)
            )
        else:
            # If conditions are not met, return a message response
            return TaskModuleResponse(
                task=TaskModuleMessageResponse(value="The conditions for displaying the task module are not met.")
            )

