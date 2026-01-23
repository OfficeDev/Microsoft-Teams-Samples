# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import requests

from microsoft_teams.api import (
    MessageExtensionQueryInvokeActivity,
    MessagingExtensionResult,
    MessagingExtensionResultType,
    AttachmentLayout,
    MessagingExtensionInvokeResponse,
    HeroCardAttachment,
    AdaptiveCardAttachment,
    card_attachment,
    MessagingExtensionAttachment,
    InvokeResponse,
)
from microsoft_teams.api.models.card import CardAction, CardActionType
from microsoft_teams.cards import AdaptiveCard, TextBlock, OpenUrlAction
from microsoft_teams.apps import ActivityContext, App

from config import DefaultConfig

# Initialize configuration
config = DefaultConfig()

# Initialize the Teams app
app = App()


def get_search_results(query: str):
    """Search GitHub repositories."""
    url = f"https://api.github.com/search/repositories?q={query}"
    headers = {"Accept": "application/vnd.github.v3+json"}
    response = requests.get(url, headers=headers)
    search_results = []
    for result in response.json().get("items", []):
        name = result["name"]
        description = result["description"]
        repo_url = result["html_url"]
        search_results.append({
            "name": name, 
            "summary": description if description else "", 
            "url": repo_url
        })
    return search_results[:10]


@app.on_message_ext_query
async def handle_message_ext_query(ctx: ActivityContext[MessageExtensionQueryInvokeActivity]):
    """Handle message extension query actions."""
    print("[DEBUG] ====== on_message_ext_query called ======")
    
    command_id = ctx.activity.value.command_id
    search_query = ""
    
    if ctx.activity.value.parameters and len(ctx.activity.value.parameters) > 0:
        search_query = ctx.activity.value.parameters[0].value or ""
    
    print(f"[DEBUG] command_id: {command_id}, search_query: {search_query}")
    
    if command_id == "searchQuery":
        # Get search results from GitHub
        results = get_search_results(search_query) if search_query else []
        print(f"[DEBUG] Got {len(results)} results")
        
        attachments: list[MessagingExtensionAttachment] = []
        
        for item in results:
            # Create Adaptive Card for main content with button
            adaptive_card = AdaptiveCardAttachment(
                content=AdaptiveCard(
                    body=[
                        TextBlock(text=item["name"], size="Large", weight="Bolder"),
                        TextBlock(text=item["summary"][:100] if item["summary"] else "", wrap=True)
                    ],
                    actions=[
                        OpenUrlAction(title="View on GitHub", url=item["url"])
                    ]
                )
            )
            
            # Create hero card for preview
            thumbnail = HeroCardAttachment(
                content={
                    "title": item["name"],
                    "subtitle": item["summary"][:50] if item["summary"] else ""
                }
            )
            
            main_attachment = card_attachment(adaptive_card)
            preview_attachment = card_attachment(thumbnail)
            
            attachment = MessagingExtensionAttachment(
                content_type=main_attachment.content_type,
                content=main_attachment.content,
                preview=preview_attachment
            )
            attachments.append(attachment)
        
        result = MessagingExtensionResult(
            type=MessagingExtensionResultType.RESULT,
            attachment_layout=AttachmentLayout.LIST,
            attachments=attachments
        )
        
        print(f"[DEBUG] Returning {len(attachments)} attachments")
        return MessagingExtensionInvokeResponse(compose_extension=result)
    
    return InvokeResponse[MessagingExtensionInvokeResponse](status=400)


if __name__ == "__main__":
    asyncio.run(app.start())
