# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Import required modules for the Teams bot application
import os
from pathlib import Path
from typing import Any, Dict, List
import asyncio

# Azure authentication for Managed Identity
from azure.identity import ManagedIdentityCredential

# Teams AI SDK components
from microsoft.teams.ai import ChatPrompt
from microsoft.teams.openai import OpenAICompletionsAIModel

# Teams API models for activities and messages
from microsoft.teams.api import MessageActivity, MessageActivityInput, MessageSubmitActionInvokeActivity

# Teams app core components
from microsoft.teams.apps import ActivityContext, App
from microsoft.teams.common import LocalStorage
from microsoft.teams.devtools import DevToolsPlugin


def load_instructions() -> str:
    """Load agent instructions from instructions.txt file for AI context"""
    # Construct path to the instructions file
    instructions_path = Path(__file__).parent / "prompts" / "chat" / "instructions.txt"
    # Read and return the instructions as a string
    return instructions_path.read_text(encoding="utf-8").strip()


def create_token_factory(client_id: str):
    """Create an async token factory for Azure Managed Identity authentication"""
    async def token_factory(scope: str | List[str], tenant_id: str | None = None):
        # Initialize managed identity credential with client ID
        credential = ManagedIdentityCredential(client_id=client_id)
        # Ensure scopes is a list
        scopes = scope if isinstance(scope, list) else [scope]
        # Get access token from Azure
        token_response = await credential.get_token(*scopes, tenant_id=tenant_id)
        return token_response.token
    
    return token_factory

# Load AI agent instructions once at application startup
instructions = load_instructions()

# Initialize local storage for conversation state management
storage = LocalStorage()

# Load bot authentication credentials from environment variables
app_id = os.getenv("MicrosoftAppId", "")
app_password = os.getenv("MicrosoftAppPassword", "")
bot_type = os.getenv("MicrosoftAppType", "")

# Configure authentication options based on bot type
credential_options: Dict[str, Any] = {}
if bot_type == "UserAssignedMsi":
    # Use Managed Identity for MSI bots
    credential_options = {
        "client_id": app_id,
        "token": create_token_factory(app_id)
    }
elif app_id and app_password:
    # Use client credentials for Multitenant or SingleTenant bots
    credential_options = {
        "client_id": app_id,
        "client_secret": app_password
    }

# Initialize the Teams app with authentication, storage, and DevTools plugin
app = App(
    **credential_options,
    storage=storage,
    plugins=[DevToolsPlugin()]
)

@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle incoming messages from Teams and generate AI-powered responses with citations"""
    # Create unique conversation identifier for this user and conversation
    conversation_key = f"{ctx.activity.conversation.id}/{ctx.activity.from_.id}"
    
    try:
        # Load Azure OpenAI configuration from environment variables
        azure_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT", "")
        azure_key = os.getenv("AZURE_OPENAI_API_KEY", "")
        deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
        
        # Initialize Azure OpenAI model with deployment details and API version
        openai_model = OpenAICompletionsAIModel(
            model=deployment_name,
            key=azure_key,
            azure_endpoint=azure_endpoint,
            api_version="2024-10-21"
        )
        
        # Create chat prompt instance with the configured model
        agent = ChatPrompt(model=openai_model)
        
        # Send user message to AI with system instructions and get response
        chat_result = await agent.send(
            input=ctx.activity.text,
            instructions=instructions
        )
        result = chat_result.response
        
        if result.content:
            # Create message with AI-generated flag for Teams UI indicator
            message = MessageActivityInput(text=result.content).add_ai_generated()
            
            # Add thumbs up/down feedback buttons to the message
            message.add_feedback()
            
            # Send the final message with AI response and feedback buttons
            await ctx.send(message)
        
    except Exception as error:
        # Log error to console for debugging
        print(f"Error: {error}")
        # Send user-friendly error messages to Teams
        await ctx.send("The agent encountered an error or bug.")
        await ctx.send("To continue to run this agent, please fix the agent source code.")


@app.on_message_submit_feedback
async def handle_feedback(ctx: ActivityContext[MessageSubmitActionInvokeActivity]) -> None:
    """Handle user feedback submissions (thumbs up/down) from Teams messages"""
    # Log feedback value to console (can be extended with custom logic)
    print(f"Your feedback is {ctx.activity.value}")


if __name__ == "__main__":
    # Get port number from environment variable or use default 3978
    port = int(os.getenv("PORT", 3978))
    
    # Display startup message with port information
    print(f"\nAgent started, app listening on port {port}")
    
    # Start the Teams app server asynchronously using uvicorn
    asyncio.run(app.start(port=port))
