import os
from pathlib import Path
from typing import Any, Dict, List
import asyncio
from azure.identity import ManagedIdentityCredential
from microsoft.teams.ai import ChatPrompt
from microsoft.teams.openai import OpenAICompletionsAIModel
from microsoft.teams.api import MessageActivity, MessageActivityInput, MessageSubmitActionInvokeActivity
from microsoft.teams.api.models.entity.citation_entity import CitationAppearance
from microsoft.teams.apps import ActivityContext, App
from microsoft.teams.common import LocalStorage
from microsoft.teams.devtools import DevToolsPlugin


# Load instructions from file
def load_instructions() -> str:
    """Load agent instructions from the instructions.txt file."""
    instructions_path = Path(__file__).parent / "prompts" / "chat" / "skprompt.txt"
    return instructions_path.read_text(encoding="utf-8").strip()


# Create token factory for Managed Identity authentication
def create_token_factory(client_id: str):
    """Create a token factory using Azure Managed Identity."""
    async def token_factory(scope: str | List[str], tenant_id: str | None = None):
        credential = ManagedIdentityCredential(client_id=client_id)
        scopes = scope if isinstance(scope, list) else [scope]
        token_response = await credential.get_token(*scopes, tenant_id=tenant_id)
        return token_response.token
    
    return token_factory

# Load instructions once at startup
instructions = load_instructions()

# Create storage for conversation history
storage = LocalStorage()

# Configure authentication credentials
app_id = os.getenv("MicrosoftAppId", "")
app_password = os.getenv("MicrosoftAppPassword", "")
bot_type = os.getenv("MicrosoftAppType", "")

credential_options: Dict[str, Any] = {}
if bot_type == "UserAssignedMsi":
    # For Managed Identity (MSI)
    credential_options = {
        "client_id": app_id,
        "token": create_token_factory(app_id)
    }
elif app_id and app_password:
    # For Multitenant or SingleTenant bots
    credential_options = {
        "client_id": app_id,
        "client_secret": app_password
    }

# Create the app with storage and credentials
app = App(
    **credential_options,
    storage=storage,
    plugins=[DevToolsPlugin()]
)

@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle incoming messages with AI-powered responses."""
    # Get conversation key for storing history
    conversation_key = f"{ctx.activity.conversation.id}/{ctx.activity.from_.id}"
    
    try:
        # Create OpenAI model
        openai_model = OpenAICompletionsAIModel(
            model=os.getenv("OPENAI_MODEL_NAME", "gpt-3.5-turbo"),
            key=os.getenv("OPENAI_API_KEY", "")
        )
        
        # Create chat prompt with model only
        agent = ChatPrompt(model=openai_model)
        
        # Send message with instructions
        chat_result = await agent.send(
            input=ctx.activity.text,
            instructions=instructions
        )
        result = chat_result.response
        
        if result.content:
            # Create message activity with AI-generated flag
            message = MessageActivityInput(text=result.content).add_ai_generated()
            
            # Generate citations based on response content matching skprompt.txt categories
            cited_docs = []
            response_lower = result.content.lower()
            
            # Define citation sources based on skprompt.txt categories
            potential_sources = [
                {
                    "keywords": ["study", "learning", "study plan", "study technique", "academic", "concept", "explanation", "research"],
                    "citation": {
                        "title": "Study Support Resources",
                        "abstract": "Educational support with course recommendations, study plans, and effective learning techniques.",
                        "url": "https://learn.microsoft.com/en-us/training/"
                    }
                },
                {
                    "keywords": ["career", "skill", "job", "professional", "certification", "career path", "advancement", "development"],
                    "citation": {
                        "title": "Career Development Guide",
                        "abstract": "Skills assessment, career advancement courses, and professional development planning resources.",
                        "url": "https://learn.microsoft.com/en-us/certifications/"
                    }
                },
                {
                    "keywords": ["course", "curriculum", "plan", "prerequisite", "sequence", "timeline", "learning objective"],
                    "citation": {
                        "title": "Course Planning Resources",
                        "abstract": "Customized curriculum design, course sequences, prerequisites, and learning timelines.",
                        "url": "https://www.coursera.org/"
                    }
                },
                {
                    "keywords": ["ai", "machine learning", "artificial intelligence", "data science", "programming", "technology", "ml", "deep learning"],
                    "citation": {
                        "title": "AI and Technology Skills",
                        "abstract": "AI, ML, and data science courses with hands-on projects from top platforms and universities.",
                        "url": "https://www.coursera.org/browse/data-science/machine-learning"
                    }
                },
                {
                    "keywords": ["coursera", "edx", "udacity", "mit", "stanford", "institution", "platform", "certification"],
                    "citation": {
                        "title": "Top Educational Platforms",
                        "abstract": "Leading learning platforms including Coursera, edX, Udacity with free and premium courses.",
                        "url": "https://www.edx.org/"
                    }
                }
            ]
            
            # Check which sources are relevant to the response
            for source in potential_sources:
                if any(keyword in response_lower for keyword in source["keywords"]):
                    cited_docs.append(source["citation"])
            
            # Add citations to the message if they exist
            if cited_docs:
                message_text = result.content
                for i, doc in enumerate(cited_docs, start=1):
                    message_text += f"[{i}]"
                    citation = CitationAppearance(
                        name=doc["title"],
                        abstract=doc["abstract"],
                        url=doc["url"]
                    )
                    message.add_citation(i, citation)
                
                # Update message text with citation numbers
                message.text = message_text
            
            # Add feedback functionality
            message.add_feedback()
            
            # Send the final message with citations
            await ctx.send(message)
        
    except Exception as error:
        print(f"Error: {error}")
        await ctx.send("The agent encountered an error or bug.")
        await ctx.send("To continue to run this agent, please fix the agent source code.")


@app.on_message_submit_feedback
async def handle_feedback(ctx: ActivityContext[MessageSubmitActionInvokeActivity]) -> None:
    """Handle feedback submissions from users."""
    # Add custom feedback processing logic here
    print(f"Your feedback is {ctx.activity.value}")


if __name__ == "__main__":
    
    # Get port from environment or use default
    port = int(os.getenv("PORT", 3978))
    
    print(f"\nAgent started, app listening on port {port}")
    
    # Start the Teams app server
    # The App class has built-in HTTP server support
    asyncio.run(app.start(port=port))
