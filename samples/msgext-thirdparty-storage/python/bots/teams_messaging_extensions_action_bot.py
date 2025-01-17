from botbuilder.core import TurnContext
from botbuilder.core.card_factory import CardFactory
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionActionResponse,
    TaskModuleContinueResponse,
    TaskModuleResponse,
    TaskModuleTaskInfo,
    MessagingExtensionResult,
)
from botbuilder.core.teams import TeamsActivityHandler
from config import DefaultConfig


class TeamsMessagingExtensionsActionBot(TeamsActivityHandler):
    def __init__(self, config: DefaultConfig):
        super().__init__()
        self.__base_url = config.BASE_URL

    def get_file_icon(self, file_name: str) -> str:
        if file_name.endswith(".pdf"):
            return f"{self.__base_url}/icons/PDFIcons.png"
        elif file_name.endswith(".doc") or file_name.endswith(".docx"):
            return f"{self.__base_url}/icons/WordIcons.png"
        elif file_name.endswith(".xls") or file_name.endswith(".xlsx"):
            return f"{self.__base_url}/icons/ExcelIcons.png"
        elif file_name.endswith(".png"):
            return f"{self.__base_url}/icons/ImageIcon.png"
        elif file_name.endswith(".jpg") or file_name.endswith(".jpeg"):
            return f"{self.__base_url}/icons/ImageIcon.png"
        else:
            return f"{self.__base_url}/icons/DefaultIcon.png"

    async def on_teams_messaging_extension_submit_action_dispatch(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
            
            user_input = action.data
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
                file_name = file.get("name", "Unknown File")
                icon_url = self.get_file_icon(file_name)
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
                                        "url": icon_url,  # Placeholder URL for icons
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
            
            adaptive_card = {
                "type": "AdaptiveCard",
                "version": "1.4",
                "body": body,
            }
            
            card_attachment = CardFactory.adaptive_card(adaptive_card)
            
            return MessagingExtensionActionResponse(
                compose_extension=MessagingExtensionResult(
                    type="result",
                    attachment_layout="list",
                    attachments=[card_attachment],  # Directly use the Attachment object
                )
            )


    async def on_teams_messaging_extension_fetch_task(
    self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> TaskModuleResponse:
        task_info = TaskModuleTaskInfo(
            width=700,
            height=450,
            title="Task Module WebView",
            url=f"{self.__base_url}/customForm",
        )
        return TaskModuleResponse(
            task=TaskModuleContinueResponse(value=task_info)
        )