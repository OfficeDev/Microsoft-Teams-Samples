"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

Description: initialize the app and listen for `message` activitys
"""

import os
import sys
import traceback
import sys
from logging import Logger, StreamHandler, DEBUG

logger = Logger("teamsai:openai", DEBUG)
logger.addHandler(StreamHandler(sys.stdout))
from botbuilder.core import MemoryStorage, TurnContext
from teams import Application, ApplicationOptions, TeamsAdapter
from teams.ai import AIOptions
from teams.ai.models import AzureOpenAIModelOptions, OpenAIModel
from teams.ai.planners import ActionPlanner, ActionPlannerOptions
from teams.ai.prompts import PromptManager, PromptManagerOptions, PromptTemplate
from teams.state import TurnState
from azure.identity import get_bearer_token_provider, DefaultAzureCredential
from pathlib import Path

from config import Config
from state import AppTurnState

config = Config()

if config.AZURE_OPENAI_ENDPOINT is None or config.AZURE_SEARCH_ENDPOINT is None or config.AZURE_SEARCH_INDEX is None:
    raise RuntimeError("Missing environment variables - please check that AZURE_OPENAI_ENDPOINT, AZURE_SEARCH_ENDPOINT and AZURE_SEARCH_INDEX is set.")

# Create AI components
model: OpenAIModel
logger = Logger("teamsai:openai", DEBUG)
logger.addHandler(StreamHandler(sys.stdout))

if config.AZURE_OPENAI_KEY:
    model = OpenAIModel(
        AzureOpenAIModelOptions(
            api_key=config.AZURE_OPENAI_KEY,
            default_model=config.AZURE_OPENAI_MODEL,
            api_version="2024-02-15-preview",
            endpoint=config.AZURE_OPENAI_ENDPOINT,
            logger=logger
        )
    )
else: 
    model = OpenAIModel(
        AzureOpenAIModelOptions(
            azure_ad_token_provider=get_bearer_token_provider(DefaultAzureCredential(), 'https://cognitiveservices.azure.com/.default'),
            default_model=config.AZURE_OPENAI_MODEL,
            api_version="2024-02-15-preview",
            endpoint=config.AZURE_OPENAI_ENDPOINT,
            logger=logger
        )
    )

prompts = PromptManager(
    PromptManagerOptions(prompts_folder=f"{os.path.dirname(os.path.abspath(__file__))}/prompts")
)

# gets prompt template and adds data source config
async def get_default_prompt(context: TurnContext, state: TurnState, planner: ActionPlanner) -> PromptTemplate:
    prompt = await prompts.get_prompt("chat")

    prompt.config.completion.model = config.AZURE_OPENAI_MODEL

    if config.AZURE_SEARCH_ENDPOINT:
        if config.AZURE_SEARCH_KEY:

            prompt.config.completion.data_sources = [
            {
                "type": 'azure_search',
                "parameters": {
                    "endpoint": config.AZURE_SEARCH_ENDPOINT,
                    "index_name": config.AZURE_SEARCH_INDEX,
                    "key": config.AZURE_SEARCH_KEY,
                    "semantic_configuration": 'default',
                    "query_type": 'simple',
                    "fields_mapping": { },
                    "in_scope": True,
                    "strictness": 3,
                    "top_n_documents": 5,
                    "role_information": Path(__file__).resolve().parent.joinpath('./prompts/chat/skprompt.txt').read_text(encoding='utf-8'),
                }  
            }
        ]
                
        else:
            prompt.config.completion.data_sources = [
                {
                    "type": 'azure_search',
                    "parameters": {
                        "endpoint": config.AZURE_SEARCH_ENDPOINT,
                        "index_name": config.AZURE_SEARCH_INDEX,
                        "semantic_configuration": 'default',
                        "query_type": 'simple',
                        "fields_mapping": { },
                        "in_scope": True,
                        "strictness": 3,
                        "top_n_documents": 5,
                        "role_information": Path(__file__).resolve().parent.joinpath('./prompts/chat/skprompt.txt').read_text(encoding='utf-8'),
                        "authentication": {
                            "type": 'system_assigned_managed_identity'
                        }
                    }  
                }
            ]
         

    return prompt

storage = MemoryStorage()
app = Application[AppTurnState](
    ApplicationOptions(
        bot_app_id=config.APP_ID,
        storage=storage,
        adapter=TeamsAdapter(config),
        ai=AIOptions(
            planner=ActionPlanner[AppTurnState](
                ActionPlannerOptions(model=model, prompts=prompts, default_prompt=get_default_prompt)
            )
        ),
    ),
)


@app.conversation_update("membersAdded")
async def conversation_update(context: TurnContext, state: AppTurnState):
    await context.send_activity(
        "Welcome! I'm a conversational bot that can tell you about your data. You can also type `/clear` to clear the conversation history."
    )
    return True


@app.message("/clear")
async def message(context: TurnContext, state: AppTurnState):
    del state.conversation
    await context.send_activity(
        "New chat session started: Previous messages won't be used as context for new queries."
    )
    return True


@app.turn_state_factory
async def turn_state_factory(context: TurnContext):
    return await AppTurnState.load(context, storage)


@app.error
async def on_error(context: TurnContext, error: Exception):
    # This check writes out errors to console log .vs. app insights.
    # NOTE: In production environment, you should consider logging this to Azure
    #       application insights.
    print(f"\n [on_turn_error] unhandled error: {error}", file=sys.stderr)
    traceback.print_exc()

    # Send a message to the user
    await context.send_activity("The bot encountered an error or bug.")
