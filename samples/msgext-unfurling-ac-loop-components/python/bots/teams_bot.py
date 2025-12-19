"""Teams bot implementation with messaging extensions and adaptive cards."""

from botbuilder.core import ActivityHandler, TurnContext, CardFactory
from botbuilder.schema import Activity, Attachment, InvokeResponse
import json
import copy
import os

# Get the bot endpoint for serving static files
BOT_ENDPOINT = os.environ.get("BOT_ENDPOINT", "")
if BOT_ENDPOINT and not BOT_ENDPOINT.endswith('/'):
    BOT_ENDPOINT += '/'

# Adaptive Card definition - matching Node.js implementation
ADAPTIVE_CARD = {
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "metadata": {
        "webUrl": "https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main"
    },
    "body": [
        {
            "type": "TextBlock",
            "text": "Adaptive Card-based Loop component",
            "size": "Large",
            "weight": "Bolder"
        },
        {
            "type": "Container",
            "items": [
                {
                    "type": "TextBlock",
                    "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
                    "weight": "bolder",
                    "size": "medium"
                }
            ]
        }
    ],
    "actions": [
        {
            "type": "Action.Execute",
            "title": "Execute!",
            "verb": "userExecute",
            "fallback": "Action.Submit"
        },
        {
            "type": "Action.OpenUrl",
            "title": "Universal Actions for Adaptive Cards",
            "url": "https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/work-with-universal-actions-for-adaptive-cards"
        },
        {
            "type": "Action.OpenUrl",
            "title": "Adaptive Card-based Loop components",
            "url": "https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/cards-loop-component?branch=pr-en-us-9230"
        }
    ]
}


class TeamsBot(ActivityHandler):
    """Teams bot that handles messaging extensions and adaptive card interactions."""

    def __init__(self):
        super().__init__()

    async def on_message_activity(self, turn_context: TurnContext):
        """Handle message activities."""
        pass
        
    async def on_members_added_activity(
        self, members_added, turn_context: TurnContext
    ):
        """Handle members added to the conversation."""
        pass
        
    async def on_installation_update_activity(self, turn_context: TurnContext):
        """Handle installation update activities."""
        pass
        
    async def on_event_activity(self, turn_context: TurnContext):
        """Handle event activities."""
        pass
        
    async def on_conversation_update_activity(self, turn_context: TurnContext):
        """Handle conversation update activities."""
        pass

    async def on_invoke_activity(self, turn_context: TurnContext):
        """Handle invoke activities for messaging extensions and adaptive cards."""
        try:
            invoke_name = turn_context.activity.name
            
            if invoke_name == "composeExtension/query":
                return await self._handle_messaging_extension_query(turn_context)
            elif invoke_name == "composeExtension/selectItem":
                return await self._handle_messaging_extension_select_item(turn_context)
            elif invoke_name == "adaptiveCard/action":
                return await self._handle_adaptive_card_invoke(turn_context)
            elif invoke_name == "composeExtension/queryLink":
                return await self._handle_app_based_link_query(turn_context)
            else:
                return await super().on_invoke_activity(turn_context)
                
        except Exception as e:
            return InvokeResponse(status=500)

    async def _handle_adaptive_card_invoke(self, turn_context: TurnContext) -> InvokeResponse:
        """Handle adaptive card action invocations."""
        try:
            invoke_value = turn_context.activity.value
            
            # Handle different ways the action might be structured
            action_verb = None
            if invoke_value:
                if hasattr(invoke_value, 'action') and hasattr(invoke_value.action, 'verb'):
                    action_verb = invoke_value.action.verb
                elif isinstance(invoke_value, dict):
                    if 'action' in invoke_value and 'verb' in invoke_value['action']:
                        action_verb = invoke_value['action']['verb']
                    elif 'verb' in invoke_value:
                        action_verb = invoke_value['verb']
            
            if action_verb == "userExecute":
                # Success card matching the Node.js implementation
                success_card = {
                    "type": "AdaptiveCard",
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.5",
                    "body": [
                        {
                            "type": "TextBlock",
                            "size": "Default",
                            "text": "Adaptive Card-based Loop component Successfully Execute!!",
                            "style": "heading"
                        },
                        {
                            "type": "Image",
                            "url": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                            "height": "auto",
                            "size": "Medium",
                            "horizontalAlignment": "left",
                            "spacing": "None",
                            "width": "0px"
                        }
                    ]
                }
                
                # Use the same response format as Node.js for both Teams and Outlook
                response_body = {
                    "statusCode": 200,
                    "type": "application/vnd.microsoft.card.adaptive",
                    "value": success_card
                }
                
                return InvokeResponse(
                    status=200,
                    body=response_body
                )
            else:
                # Default response for other actions
                response_body = {
                    "type": "application/vnd.microsoft.card.adaptive",
                    "value": {
                        "type": "AdaptiveCard",
                        "version": "1.5",
                        "body": [
                            {
                                "type": "TextBlock",
                                "text": f"Action received: {action_verb or 'Unknown'}",
                                "wrap": True
                            }
                        ]
                    }
                }
                
                return InvokeResponse(
                    status=200,
                    body=response_body
                )
                
        except Exception as e:
            return InvokeResponse(status=500)

    def handle_teams_app_based_link_query(self) -> dict:
        """Handle Teams app-based link query for message extension link unfurling."""
        attachment = copy.deepcopy(ADAPTIVE_CARD)
        
        attachment["preview"] = {
            "content": {
                "title": "Adaptive Card-based Loop component",
                "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
            },
            "contentType": "application/vnd.microsoft.card.thumbnail",
        }

        result = {
            "attachmentLayout": "list",
            "type": "result",
            "attachments": [attachment]
        }

        response = {
            "composeExtension": result
        }

        return response

    async def _handle_app_based_link_query(self, turn_context: TurnContext) -> InvokeResponse:
        """Handle Teams app-based link query for message extension link unfurling."""
        try:
            # Create attachment using CardFactory like Node.js implementation
            card_attachment = CardFactory.adaptive_card(ADAPTIVE_CARD)
            
            # Convert to dict and add preview like Node.js implementation
            attachment = {
                "contentType": card_attachment.content_type,
                "content": card_attachment.content,
                "preview": {
                    "content": {
                        "title": "Adaptive Card-based Loop component",
                        "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
                    },
                    "contentType": "application/vnd.microsoft.card.thumbnail",
                }
            }

            result = {
                "attachmentLayout": "list",
                "type": "result",
                "attachments": [attachment]
            }

            response = {
                "composeExtension": result
            }

            return InvokeResponse(status=200, body=response)
            
        except Exception as e:
            return InvokeResponse(status=500)

    async def _handle_messaging_extension_query(self, turn_context: TurnContext) -> InvokeResponse:
        """Handle messaging extension query."""
        try:
            # Create the attachment using CardFactory like Node.js implementation
            card_attachment = CardFactory.adaptive_card(ADAPTIVE_CARD)
            
            # Convert to dict and add preview like Node.js implementation
            attachment = {
                "contentType": card_attachment.content_type,
                "content": card_attachment.content,
                "preview": {
                    "content": {
                        "title": "Adaptive Card-based Loop component",
                        "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
                    },
                    "contentType": "application/vnd.microsoft.card.thumbnail",
                }
            }

            response = {
                "composeExtension": {
                    "type": "result",
                    "attachmentLayout": "list",
                    "attachments": [attachment]
                }
            }
            
            return InvokeResponse(status=200, body=response)
            
        except Exception as e:
            return InvokeResponse(status=500)

    async def _handle_messaging_extension_select_item(self, turn_context: TurnContext) -> InvokeResponse:
        """
        Receives invoke activities with the name 'selectItem'.
        Used in creating a Search-based Message Extension.
        """
        try:
            # Create attachment using CardFactory like Node.js implementation
            card_attachment = CardFactory.adaptive_card(ADAPTIVE_CARD)
            
            # Convert to dict structure
            attachment = {
                "contentType": card_attachment.content_type,
                "content": card_attachment.content
            }
            
            response = {
                "composeExtension": {
                    "type": "result",
                    "attachmentLayout": "list",
                    "attachments": [attachment]
                }
            }
            
            return InvokeResponse(status=200, body=response)
            
        except Exception as e:
            return InvokeResponse(status=500)
