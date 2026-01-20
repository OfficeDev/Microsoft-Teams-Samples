# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'adaptive_cards'))
from microsoft_teams.apps import App, ActivityContext
from microsoft_teams.api import MessageActivity, MessageActivityInput, ConversationUpdateActivity, Attachment
import cards
from config import Config

config = Config()

# Create the Teams App
app = App(
    client_id=config.APP_ID,
    client_secret=config.APP_PASSWORD,
    tenant_id=config.APP_TENANTID
)

# Welcome new members with adaptive card format options
@app.on_conversation_update
async def on_members_added(ctx: ActivityContext[ConversationUpdateActivity]):
    """Handle new members added to conversation."""
    if ctx.activity.members_added:
        for member in ctx.activity.members_added:
            if member.id != ctx.activity.recipient.id:
                welcome_message = (
                    "Welcome to Adaptive Card Format. This bot will introduce you to different types of formats. "
                    "Please select a card from the options provided."
                )
                await ctx.send(welcome_message)
                await send_adaptive_card_formats(ctx)


# Handle user messages and display requested card formats
@app.on_message
async def on_message_activity(ctx: ActivityContext[MessageActivity]):
    """Handle message activities."""
    text = ctx.activity.text

    # List of valid card options
    adaptive_format_cards = [
        "CodeBlock",
        "MentionSupport",
        "InfoMasking",
        "FullWidthCard",
        "StageViewImages",
        "OverflowMenu",
        "HTMLConnector",
        "CardWithEmoji",
        "Persona",
        "PersonaSet",
        "Layout",
        "Borders",
        "RoundedCorners",
        "FluentIcons",
        "MediaElements",
        "StarRatings",
        "ConditionalCard",
        "ScrollableContainer",
        "CompoundButton",
        "ContainerLayout",
        "DonutChart",
        "GaugeChart",
        "HorizontalChart",
        "HorizontalChartStacked",
        "LineChart",
        "PieChart",
        "VerticalBarChart",
        "VerticalBarGroupedChart",
    ]

    # Check if the received text is a valid card option
    if text in adaptive_format_cards:
        card_method_map = {
            "MentionSupport": cards.mentionSupport(),
            "InfoMasking": cards.informationMaskingCard(),
            "FullWidthCard": cards.sampleAdaptiveWithFullWidth(),
            "StageViewImages": cards.stageViewForImages(),
            "OverflowMenu": cards.overflowMenu(),
            "HTMLConnector": cards.formatHTMLConnectorCard(),
            "CardWithEmoji": cards.adaptiveCardWithEmoji(),
            "Persona": cards.adaptivePeoplePersonaCardIcon(),
            "PersonaSet": cards.adaptivePeoplePersonaCardSetIcon(),
            "CodeBlock": cards.codeBlocksCard(),
            "Layout": cards.AdaptiveCardResponsiveLayout(),
            "Borders": cards.adaptiveCardBorders(),
            "RoundedCorners": cards.adaptiveCardRoundedCorners(),
            "FluentIcons": cards.adaptiveCardFluentIcon(),
            "MediaElements": cards.adaptiveCardMediaElements(),
            "StarRatings": cards.adaptiveCardStarRatings(),
            "ConditionalCard": cards.adaptiveCardConditional(),
            "ScrollableContainer": cards.adaptiveCardScrollable(),
            "CompoundButton": cards.adaptiveCardCompoundButton(),
            "ContainerLayout": cards.adaptiveCardContainerLayouts(),
            "DonutChart": cards.adaptiveCardDonutChart(),
            "GaugeChart": cards.adaptiveCardGaugeChart(),
            "HorizontalChart": cards.adaptiveCardHorizontalBarChart(),
            "HorizontalChartStacked": cards.adaptiveCardHorizontalBarStacked(),
            "LineChart": cards.adaptiveCardLineChart(),
            "PieChart": cards.adaptiveCardPieChart(),
            "VerticalBarChart": cards.adaptiveCardVerticalBarChart(),
            "VerticalBarGroupedChart": cards.adaptiveCardVerticalBarGroupedChart(),
        }

        # Call the respective method dynamically
        if text in card_method_map:
            card_dict = card_method_map[text]
            attachment = Attachment(
                content=card_dict.get("content"),
                content_type=card_dict.get("contentType")
            )
            
            message = MessageActivityInput(attachments=[attachment])
            await ctx.send(message)
            await ctx.send(f"You have selected <b>{text}</b>")

    elif (
        ctx.activity.value is not None
        and ctx.activity.text is None
    ):
        activity_value = ctx.activity.value
        if "rating1" in activity_value and "rating2" in activity_value:
            await ctx.send(f"Ratings Feedback: {activity_value}")
    if text not in adaptive_format_cards:
        await send_adaptive_card_formats(ctx)


# Display hero card with all available format options
async def send_adaptive_card_formats(ctx: ActivityContext):
    """Send a hero card with all available card format options."""
    
    card_actions = [
        {"type": "imBack", "title": "MentionSupport", "value": "MentionSupport"},
        {"type": "imBack", "title": "InfoMasking", "value": "InfoMasking"},
        {"type": "imBack", "title": "FullWidthCard", "value": "FullWidthCard"},
        {"type": "imBack", "title": "StageViewImages", "value": "StageViewImages"},
        {"type": "imBack", "title": "OverflowMenu", "value": "OverflowMenu"},
        {"type": "imBack", "title": "HTMLConnector", "value": "HTMLConnector"},
        {"type": "imBack", "title": "CardWithEmoji", "value": "CardWithEmoji"},
        {"type": "imBack", "title": "Persona", "value": "Persona"},
        {"type": "imBack", "title": "PersonaSet", "value": "PersonaSet"},
        {"type": "imBack", "title": "CodeBlock", "value": "CodeBlock"},
        {"type": "imBack", "title": "Layout", "value": "Layout"},
        {"type": "imBack", "title": "Borders", "value": "Borders"},
        {"type": "imBack", "title": "RoundedCorners", "value": "RoundedCorners"},
        {"type": "imBack", "title": "FluentIcons", "value": "FluentIcons"},
        {"type": "imBack", "title": "MediaElements", "value": "MediaElements"},
        {"type": "imBack", "title": "StarRatings", "value": "StarRatings"},
        {"type": "imBack", "title": "ConditionalCard", "value": "ConditionalCard"},
        {"type": "imBack", "title": "ScrollableContainer", "value": "ScrollableContainer"},
        {"type": "imBack", "title": "CompoundButton", "value": "CompoundButton"},
        {"type": "imBack", "title": "ContainerLayout", "value": "ContainerLayout"},
        {"type": "imBack", "title": "DonutChart", "value": "DonutChart"},
        {"type": "imBack", "title": "GaugeChart", "value": "GaugeChart"},
        {"type": "imBack", "title": "HorizontalChart", "value": "HorizontalChart"},
        {"type": "imBack", "title": "HorizontalChartStacked", "value": "HorizontalChartStacked"},
        {"type": "imBack", "title": "LineChart", "value": "LineChart"},
        {"type": "imBack", "title": "PieChart", "value": "PieChart"},
        {"type": "imBack", "title": "VerticalBarChart", "value": "VerticalBarChart"},
        {"type": "imBack", "title": "VerticalBarGroupedChart", "value": "VerticalBarGroupedChart"},
    ]

    # Create hero card with all options
    hero_card_attachment = Attachment(
        content={
            "title": "Please select a card from given options.",
            "buttons": card_actions
        },
        content_type="application/vnd.microsoft.card.hero"
    )
    message = MessageActivityInput(attachments=[hero_card_attachment])
    await ctx.send(message)

# Start the bot application
async def main():
    """Start the application."""
    await app.start()
    print(f"\nBot started, app listening to port 3978")

if __name__ == "__main__":
    asyncio.run(main())
