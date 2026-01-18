import asyncio
import os

from microsoft_teams.api import MessageActivity, MessageActivityInput
from microsoft_teams.api.activities.conversation import ConversationUpdateActivity
from microsoft_teams.apps import ActivityContext, App
from config import Config

# Sample bot messages and descriptions
SAMPLE_DESCRIPTION = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned"
OPTIONS = "Type 1 to know about the permissions required,  Type 2 for documentation link"
PERMISSION_REQUIRED = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app"
DOC_LINK = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc"


# Note: For Azure deployment with managed identity, configure through environment variables
# The Teams AI SDK handles token acquisition automatically when running on Azure

# Initialize the Teams Application
app = App()


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """
    Handle incoming message activities.
    
    Responds based on user input:
    - "1": Shows permission requirements
    - "2": Shows documentation link
    - Any other input: Shows sample description and menu options
    """
    try:
        # Get the text from the incoming message
        reply_text = ctx.activity.text
        
        if not reply_text:
            # Send default message if text is empty
            await ctx.send(MessageActivityInput(text=SAMPLE_DESCRIPTION))
            await ctx.send(MessageActivityInput(text=OPTIONS))
            return
        
        reply_text = reply_text.strip()
        
        # Respond based on user input
        if reply_text == "1":
            # Provide information about required permissions
            await ctx.send(MessageActivityInput(text=PERMISSION_REQUIRED))
        elif reply_text == "2":
            # Provide the documentation link
            await ctx.send(MessageActivityInput(text=DOC_LINK))
        else:
            # Send the default sample description and options for further interaction
            await ctx.send(MessageActivityInput(text=SAMPLE_DESCRIPTION))
            await ctx.send(MessageActivityInput(text=OPTIONS))
            
    except Exception as error:
        await ctx.send(MessageActivityInput(text="The bot encountered an error or bug."))
        await ctx.send(MessageActivityInput(text="To continue to run this bot, please fix the bot source code."))


@app.on_conversation_update
async def handle_members_added(ctx: ActivityContext[ConversationUpdateActivity]) -> None:
    """
    Handle conversation update events including when new members are added to a team.
    Send a welcome message to new members (except the bot itself).
    """
    try:
        # Check if there are members added in this update
        if not ctx.activity.members_added:
            return
            
        welcome_text = "Hello and welcome! With this sample, your bot can receive user messages across standard channels in a team without being @mentioned."
        
        for member in ctx.activity.members_added:
            # Ensure the bot does not send a welcome message to itself
            if member.id != ctx.activity.recipient.id:
                await ctx.send(MessageActivityInput(text=welcome_text))
    except Exception as error:
        pass


if __name__ == "__main__":
    asyncio.run(app.start())
