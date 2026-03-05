# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio

from dotenv import load_dotenv
from microsoft_teams.api import (
    AdaptiveCardActionMessageResponse,
    AdaptiveCardInvokeActivity,
    AdaptiveCardInvokeResponse,
    MessageActivity,
)
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.cards import (
    AdaptiveCard,
    ExecuteAction,
    OpenUrlAction,
    ShowCardAction,
    TextBlock,
    TextInput,
    ToggleVisibilityAction,
)

load_dotenv()

app = App()


def create_card_actions_card() -> AdaptiveCard:
    """Create an adaptive card demonstrating various card actions."""
    innermost_card = AdaptiveCard(
        body=[
            TextBlock(text="**Welcome To Your New Card**"),
            TextBlock(text="This is your new card inside another card"),
        ]
    )

    middle_card = AdaptiveCard(
        body=[TextBlock(text="This card's action will show another card")],
        actions=[ShowCardAction(title="Action.ShowCard", card=innermost_card)],
    )

    submit_form_card = AdaptiveCard(
        body=[
            TextInput(id="name")
            .with_label("Please enter your name:")
            .with_is_required(True)
            .with_error_message("Name is required")
        ],
        actions=[
            ExecuteAction(title="Submit")
            .with_data({"action": "submit_name"})
            .with_associated_inputs("auto")
        ],
    )

    return AdaptiveCard(
        body=[TextBlock(text="Adaptive Card Actions")],
        actions=[
            OpenUrlAction(title="Action Open URL", url="https://adaptivecards.io"),
            ShowCardAction(title="Action Submit", card=submit_form_card),
            ShowCardAction(title="Action ShowCard", card=middle_card),
        ],
    )


def create_toggle_visibility_card() -> AdaptiveCard:
    """Create a card demonstrating the toggle visibility action."""
    return AdaptiveCard(
        body=[
            TextBlock(
                text="Click to show or hide the message"
            ),
            TextBlock(
                id="helloWorld",
                is_visible=False,
                text="**Hello World!**",
                size="ExtraLarge",
            ),
        ],
        actions=[
            ToggleVisibilityAction(title="Click me!", target_elements=["helloWorld"])
        ],
    )


@app.on_message_pattern("card actions")
async def handle_card_actions(ctx: ActivityContext[MessageActivity]) -> None:
    """Send adaptive card demonstrating various card actions."""
    await ctx.send(create_card_actions_card())


@app.on_message_pattern("toggle visibility")
async def handle_toggle_visibility(ctx: ActivityContext[MessageActivity]) -> None:
    """Send toggle visibility card."""
    await ctx.send(create_toggle_visibility_card())


@app.on_card_action
async def handle_card_action(ctx: ActivityContext[AdaptiveCardInvokeActivity]) -> AdaptiveCardInvokeResponse:
    """Handle card action submissions."""
    data = ctx.activity.value.action.data
    await ctx.send(f"Data Submitted: {data['name']}")

    return AdaptiveCardActionMessageResponse(
        value="Action processed successfully",
    )


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle incoming messages."""
    await ctx.send("Welcome to the Cards Bot! To interact with me, send one of the following commands: 'card actions' or 'toggle visibility'")


if __name__ == "__main__":
    asyncio.run(app.start())
