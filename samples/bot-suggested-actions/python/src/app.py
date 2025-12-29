# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.api import MessageActivity, MessageActivityInput, ConversationUpdateActivity
from microsoft_teams.api.models import SuggestedActions, CardAction, CardActionType
from config import Config

config = Config()

# Create the Teams App
app = App(
    client_id=config.APP_ID,
    client_secret=config.APP_PASSWORD,
    tenant_id=config.APP_TENANTID
)

# Welcome new members with suggested actions
@app.on_conversation_update
async def on_members_added(ctx: ActivityContext[ConversationUpdateActivity]):
    """Handle new members added to conversation."""
    if ctx.activity.members_added:
        for member in ctx.activity.members_added:
            if member.id != ctx.activity.recipient.id:
                welcome_message = "Welcome to the bot! Let's get started with some suggestions."
                await ctx.send(welcome_message)
                await send_suggested_actions(ctx)


# Handle user messages and respond based on input
@app.on_message
async def on_message_activity(ctx: ActivityContext[MessageActivity]):
    """Handle message activities."""
    user_message = ctx.activity.text

    if user_message == "Hello":
        await ctx.send("Hello! How can I assist you today?")
    elif user_message == "Welcome":
        await ctx.send("Welcome! How can I assist you today?")
    elif user_message == "@SuggestedActionsBot":
        await ctx.send("@SuggestedActionsBot! How can I assist you today?")
    else:
        await ctx.send("I didn't understand that. Please choose one of the suggested actions.")
    
    # Send suggested actions for the next turn
    await send_suggested_actions(ctx)


async def send_suggested_actions(ctx: ActivityContext):
    """
    Sends a message with suggested actions to the user.
    Uses native Teams SDK SuggestedActions with IMBack action type.
    When the user clicks a button, the text value will be displayed in the channel 
    as if the user typed it, and will automatically trigger the on_message handler.
    """
    # Create MessageActivity with SuggestedActions using native Teams SDK
    suggested_actions = SuggestedActions(
        to=[ctx.activity.from_.id],
        actions=[
            CardAction(
                type=CardActionType.IM_BACK,
                title="Hello",
                value="Hello"
            ),
            CardAction(
                type=CardActionType.IM_BACK,
                title="Welcome",
                value="Welcome"
            ),
            CardAction(
                type=CardActionType.IM_BACK,
                title="@SuggestedActionsBot",
                value="@SuggestedActionsBot"
            )
        ]
    )
    
    message = MessageActivityInput(
        text="Choose one of the action from the suggested actions",
        suggested_actions=suggested_actions
    )
    
    await ctx.send(message)


# Start the bot application
async def main():
    """Start the application."""
    await app.start()
    print(f"\nBot started, app listening to port 3978")

if __name__ == "__main__":
    asyncio.run(main())
