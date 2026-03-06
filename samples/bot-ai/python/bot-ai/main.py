# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Bot AI - Teams SDK Sample

import asyncio
import json

from microsoft_teams.api import (
    MessageActivity,
    MessageActivityInput,
    InvokeActivity,
    CitationAppearance,
)
from microsoft_teams.api.activities.utils import strip_mentions_text
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.devtools import DevToolsPlugin


def _add_sensitivity(msg: MessageActivityInput, name: str, description: str) -> MessageActivityInput:
    """Add sensitivity usageInfo to the root message entity via extra fields."""
    entity = msg.ensure_single_root_level_message_entity()
    entity.__pydantic_extra__["usageInfo"] = {
        "@type": "CreativeWork",
        "name": name,
        "description": description,
    }
    return msg

app = App(plugins=[DevToolsPlugin()])


# Handle invoke activities (feedback button actions)
@app.on_invoke
async def handle_invoke(ctx: ActivityContext[InvokeActivity]):
    try:
        activity = ctx.activity

        if activity.name == "message/submitAction":
            reaction = "No reaction"
            feedback = "No feedback"

            if hasattr(activity, "value") and activity.value:
                action_value = activity.value.action_value
                reaction = action_value.reaction or "No reaction"
                feedback_raw = action_value.feedback
                if feedback_raw:
                    feedback_data = json.loads(feedback_raw)
                    feedback = feedback_data.get("feedbackText", "No feedback")

            await ctx.send(MessageActivityInput(text=f"Provided reaction: {reaction}<br> Feedback: {feedback}"))
        else:
            await ctx.send(MessageActivityInput(text=f"Unknown invoke activity handled as default - {activity.name}"))
    except Exception as err:
        print(f"Error in invoke activity: {err}")
        await ctx.send(MessageActivityInput(text=f"Invoke activity received - {ctx.activity.name}"))


# Handle message events
@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    text = strip_mentions_text(ctx.activity).strip().lower()

    if "label" in text:
        await add_ai_label(ctx)
    elif "sensitivity" in text:
        await add_sensitivity_label(ctx)
    elif "feedback" in text:
        await add_feedback_buttons(ctx)
    elif "citation" in text:
        await add_citations(ctx)
    elif "aitext" in text:
        await send_ai_message(ctx)
    else:
        await ctx.send("Welcome to Bot AI!")


# Send a message with AI label
async def add_ai_label(ctx: ActivityContext[MessageActivity]):
    await ctx.send(
        MessageActivityInput(
            text="Hey I'm a friendly AI bot. This message is generated via AI",
        ).add_ai_generated()
    )


# Send a message with sensitivity label
async def add_sensitivity_label(ctx: ActivityContext[MessageActivity]):
    msg = MessageActivityInput(
        text="This is an example for sensitivity label that help users identify the confidentiality of a message",
    )
    _add_sensitivity(msg, "Confidential \\ Contoso FTE", "Please be mindful of sharing outside of your team")
    await ctx.send(msg)


# Send a message with feedback buttons enabled
async def add_feedback_buttons(ctx: ActivityContext[MessageActivity]):
    await ctx.send(
        MessageActivityInput(
            text="This is an example for Feedback buttons that helps to provide feedback for a bot message",
        ).add_feedback()
    )


# Send a message with citations
async def add_citations(ctx: ActivityContext[MessageActivity]):
    await ctx.send(
        MessageActivityInput(
            text="Hey I'm a friendly AI bot. This message is generated through AI [1]",
        ).add_citation(
            position=1,
            appearance=CitationAppearance(
                name="AI bot",
                url="https://example.com/claim-1",
                abstract="Excerpt description",
                text='{"type":"AdaptiveCard","$schema":"http://adaptivecards.io/schemas/adaptive-card.json","version":"1.6","body":[{"type":"TextBlock","text":"Adaptive Card text"}]}',
                keywords=["keyword 1", "keyword 2", "keyword 3"],
                icon="Microsoft Word",
            ),
        )
    )


# Send a comprehensive AI message with all features
async def send_ai_message(ctx: ActivityContext[MessageActivity]):
    msg = (
        MessageActivityInput(
            text="Hey I'm a friendly AI bot. This message is generated via AI [1]",
        )
        .add_ai_generated()
        .add_feedback()
        .add_citation(
            position=1,
            appearance=CitationAppearance(
                name="AI bot",
                url="https://example.com/claim-1",
                abstract="Excerpt description",
                keywords=["keyword 1", "keyword 2", "keyword 3"],
            ),
        )
    )
    _add_sensitivity(msg, "Confidential \\ Contoso FTE", "Please be mindful of sharing outside of your team")
    await ctx.send(msg)


# Starts the Teams bot application and listens for incoming requests
if __name__ == "__main__":
    asyncio.run(app.start())
