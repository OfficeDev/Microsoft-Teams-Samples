# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import json
import os

from microsoft.teams.apps import App, ActivityContext
from microsoft.teams.api import (
    MessageActivity,
    MessageActivityInput,
    InvokeActivity,
    InvokeResponse,
    Attachment,
)

from config import Config
from models import (
    UISettings,
    TaskModuleUIConstants,
    TaskModuleIds,
    TaskModuleResponseFactory,
    TaskModuleTaskInfo,
)

config = Config()
app = App()


def get_base_url() -> str:
    """Get the base URL for static pages."""
    return config.BASE_URL


def create_hero_card_attachment() -> Attachment:
    """
    Creates a HeroCard with task module options.
    
    :return: An Attachment object containing the HeroCard.
    """
    buttons = []
    for card_type in [
        TaskModuleUIConstants.ADAPTIVE_CARD,
        TaskModuleUIConstants.CUSTOM_FORM,
        TaskModuleUIConstants.YOUTUBE,
    ]:
        buttons.append({
            "type": "invoke",
            "title": card_type.button_title,
            "value": json.dumps({"type": "task/fetch", "data": card_type.id}),
        })

    hero_card_content = {
        "title": "Task Module Invocation from Hero Card",
        "buttons": buttons
    }
    
    return Attachment(
        content_type="application/vnd.microsoft.card.hero",
        content=hero_card_content
    )


def create_adaptive_card_options_attachment() -> Attachment:
    """
    Creates an AdaptiveCard with task module options.
    
    :return: An Attachment object containing the AdaptiveCard.
    """
    actions = []
    for card_type in [
        TaskModuleUIConstants.ADAPTIVE_CARD,
        TaskModuleUIConstants.CUSTOM_FORM,
        TaskModuleUIConstants.YOUTUBE,
    ]:
        actions.append({
            "type": "Action.Submit",
            "title": card_type.button_title,
            "data": {"msteams": {"type": "task/fetch"}, "data": card_type.id}
        })

    adaptive_card_content = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.0",
        "type": "AdaptiveCard",
        "body": [
            {
                "type": "TextBlock",
                "text": "Task Module Invocation from Adaptive Card",
                "weight": "bolder",
                "size": "large",
            }
        ],
        "actions": actions
    }

    return Attachment(
        content_type="application/vnd.microsoft.card.adaptive",
        content=adaptive_card_content
    )


def create_adaptive_card_for_task_module() -> Attachment:
    """
    Creates an AdaptiveCard attachment from a JSON file for task module.
    
    :return: An Attachment object containing the AdaptiveCard.
    """
    card_path = os.path.join(os.path.dirname(__file__), "resources", "adaptiveCard.json")
    with open(card_path, "r", encoding="utf-8") as in_file:
        card_data = json.load(in_file)

    return Attachment(
        content_type="application/vnd.microsoft.card.adaptive",
        content=card_data
    )


def set_task_info(task_info: TaskModuleTaskInfo, ui_constants: UISettings):
    """
    Sets the task info properties based on the given UI settings.
    
    :param task_info: The task module task info object.
    :param ui_constants: The UI settings object.
    """
    task_info.height = ui_constants.height
    task_info.width = ui_constants.width
    task_info.title = ui_constants.title


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """
    Handle incoming message activities and display two cards: 
    a HeroCard and an AdaptiveCard with task module options.
    """
    # Create message with both cards
    message = MessageActivityInput()
    message.attachments = [
        create_hero_card_attachment(),
        create_adaptive_card_options_attachment()
    ]
    
    await ctx.send(message)


@app.on_invoke
async def handle_invoke(ctx: ActivityContext[InvokeActivity]) -> InvokeResponse:
    """
    Handle invoke activities for task module fetch and submit.
    """
    activity_name = ctx.activity.name
    
    if activity_name == "task/fetch":
        return await handle_task_module_fetch(ctx)
    elif activity_name == "task/submit":
        return await handle_task_module_submit(ctx)
    
    # Return empty response for unhandled invoke types
    return InvokeResponse(status=200, body={})


async def handle_task_module_fetch(ctx: ActivityContext[InvokeActivity]) -> InvokeResponse:
    """
    Called when the user selects an option from the displayed HeroCard or AdaptiveCard.
    Returns a TaskModuleResponse with the appropriate task module configuration.
    """
    base_url = get_base_url()
    
    # Extract the task module request data
    # ctx.activity.value is a TaskModuleRequest Pydantic object
    request_value = ctx.activity.value
    
    # Get the data field - it could be in request_value.data or request_value.data.data
    # depending on whether it came from HeroCard or AdaptiveCard
    card_task_fetch_value = ""
    if request_value:
        # Try to access the data attribute
        if hasattr(request_value, 'data'):
            data = request_value.data
            if isinstance(data, str):
                card_task_fetch_value = data
            elif hasattr(data, 'data'):
                card_task_fetch_value = data.data if hasattr(data, 'data') else ""
            elif isinstance(data, dict):
                card_task_fetch_value = data.get("data", "")
    
    task_info = TaskModuleTaskInfo()

    if card_task_fetch_value == TaskModuleIds.YOUTUBE:
        task_info.url = f"{base_url}/{TaskModuleIds.YOUTUBE}.html"
        task_info.fallback_url = task_info.url
        set_task_info(task_info, TaskModuleUIConstants.YOUTUBE)
    elif card_task_fetch_value == TaskModuleIds.CUSTOM_FORM:
        task_info.url = f"{base_url}/{TaskModuleIds.CUSTOM_FORM}.html"
        task_info.fallback_url = task_info.url
        set_task_info(task_info, TaskModuleUIConstants.CUSTOM_FORM)
    elif card_task_fetch_value == TaskModuleIds.ADAPTIVE_CARD:
        task_info.card = create_adaptive_card_for_task_module()
        set_task_info(task_info, TaskModuleUIConstants.ADAPTIVE_CARD)

    response = TaskModuleResponseFactory.to_task_module_response(task_info)
    return InvokeResponse(status=200, body=response)


async def handle_task_module_submit(ctx: ActivityContext[InvokeActivity]) -> InvokeResponse:
    """
    Called when data is being returned from the selected task module option.
    Returns a TaskModuleResponse with a thank you message.
    """
    request_value = ctx.activity.value
    
    # Convert the Pydantic object to a dict for JSON serialization
    if hasattr(request_value, 'model_dump'):
        request_data = request_value.model_dump()
    elif hasattr(request_value, 'dict'):
        request_data = request_value.dict()
    else:
        request_data = str(request_value)
    
    # Send a message with the submitted data
    message = MessageActivityInput(
        text=f"on_teams_task_module_submit: {json.dumps(request_data, default=str)}"
    )
    await ctx.send(message)

    # Return a message response
    response = {
        "task": {
            "type": "message",
            "value": "Thanks!"
        }
    }
    return InvokeResponse(status=200, body=response)


if __name__ == "__main__":
    asyncio.run(app.start())
