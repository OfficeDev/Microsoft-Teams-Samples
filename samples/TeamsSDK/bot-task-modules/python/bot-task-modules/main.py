# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
from dotenv import load_dotenv

from microsoft_teams.api import (
    AdaptiveCardAttachment,
    CardTaskModuleTaskInfo,
    MessageActivity,
    MessageActivityInput,
    TaskFetchInvokeActivity,
    TaskModuleContinueResponse,
    TaskModuleMessageResponse,
    TaskModuleResponse,
    TaskSubmitInvokeActivity,
    UrlTaskModuleTaskInfo,
    card_attachment,
)
from microsoft_teams.apps import ActivityContext, App, ErrorEvent
from microsoft_teams.cards import AdaptiveCard, OpenDialogData, SubmitAction, SubmitData, TextBlock, TextInput

load_dotenv()

if not os.getenv("BOT_ENDPOINT"):
    print("No remote endpoint detected. Using webpages will not work as expected")

app = App()

app.page("customform", os.path.join(os.path.dirname(__file__), "pages", "CustomForm"), "/customform")


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    adaptive_card = AdaptiveCard(version="1.4").with_body(
        [TextBlock(text="Task Module Invocation from Adaptive Card", weight="Bolder", size="Large")]
    ).with_actions(
        [
            SubmitAction(title="Adaptive Card").with_data(OpenDialogData("adaptive_card")),
            SubmitAction(title="Custom Form").with_data(OpenDialogData("custom_form")),
            SubmitAction(title="Multi-step Form").with_data(OpenDialogData("multi_step_form")),
        ]
    )
    await ctx.send(MessageActivityInput().add_card(adaptive_card))


@app.on_dialog_open("adaptive_card")
async def handle_adaptive_card_open(ctx: ActivityContext[TaskFetchInvokeActivity]):
    dialog_card = AdaptiveCard(version="1.0").with_body([
        TextBlock(text="Enter Text Here", weight="Bolder"),
        TextInput(id="usertext").with_placeholder("add some text and submit").with_is_multiline(True),
    ]).with_actions([
        SubmitAction(title="Submit").with_data(SubmitData("adaptive_card_submit")),
    ])

    return TaskModuleResponse(
        task=TaskModuleContinueResponse(
            value=CardTaskModuleTaskInfo(
                title="Adaptive Card: Inputs",
                width=400,
                height=200,
                card=card_attachment(AdaptiveCardAttachment(content=dialog_card)),
            )
        )
    )


@app.on_dialog_open("custom_form")
async def handle_custom_form_open(ctx: ActivityContext[TaskFetchInvokeActivity]):
    return TaskModuleResponse(
        task=TaskModuleContinueResponse(
            value=UrlTaskModuleTaskInfo(
                title="Custom Form",
                width=510,
                height=450,
                url=f"{os.getenv('BOT_ENDPOINT', 'http://localhost:3978')}/customform",
                fallback_url=f"{os.getenv('BOT_ENDPOINT', 'http://localhost:3978')}/customform",
            )
        )
    )


@app.on_dialog_open("multi_step_form")
async def handle_multi_step_form_open(ctx: ActivityContext[TaskFetchInvokeActivity]):
    dialog_card = AdaptiveCard(version="1.4").with_body([
        TextBlock(text="Step 1 of 2 - Your Name", size="Large", weight="Bolder"),
        TextInput(id="name").with_label("Name").with_placeholder("Enter your name").with_is_required(True),
    ]).with_actions([
        SubmitAction(title="Next").with_data(SubmitData("multi_step_1")),
    ])

    return TaskModuleResponse(
        task=TaskModuleContinueResponse(
            value=CardTaskModuleTaskInfo(
                title="Multi-step Form",
                width=400,
                height=300,
                card=card_attachment(AdaptiveCardAttachment(content=dialog_card)),
            )
        )
    )


@app.on_dialog_submit("adaptive_card_submit")
async def handle_adaptive_card_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]):
    data = ctx.activity.value.data
    usertext = data.get("usertext") if data else None
    await ctx.send(f"You submitted: {usertext}")
    return TaskModuleResponse(task=TaskModuleMessageResponse(value="Thanks for submitting!"))


@app.on_dialog_submit("custom_form_submit")
async def handle_custom_form_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]):
    data = ctx.activity.value.data
    name = data.get("name") if data else None
    email = data.get("email") if data else None
    await ctx.send(f"Hi {name}, thanks for submitting! Your email is {email}")
    return TaskModuleResponse(task=TaskModuleMessageResponse(value="Form submitted successfully"))


@app.on_dialog_submit("multi_step_1")
async def handle_multi_step_1_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]):
    data = ctx.activity.value.data
    name = data.get("name") if data else None
    next_card = AdaptiveCard(version="1.4").with_body([
        TextBlock(text="Step 2 of 2 - Your Email", size="Large", weight="Bolder"),
        TextInput(id="email").with_label("Email").with_placeholder("Enter your email").with_is_required(True),
    ]).with_actions([
        SubmitAction(title="Submit").with_data(SubmitData("multi_step_2", {"name": name})),
    ])

    return TaskModuleResponse(
        task=TaskModuleContinueResponse(
            value=CardTaskModuleTaskInfo(
                title="Multi-step Form: Step 2",
                width=400,
                height=300,
                card=card_attachment(AdaptiveCardAttachment(content=next_card)),
            )
        )
    )


@app.on_dialog_submit("multi_step_2")
async def handle_multi_step_2_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]):
    data = ctx.activity.value.data
    name = data.get("name") if data else None
    email = data.get("email") if data else None
    await ctx.send(f"Hi {name}, thanks for submitting! Your email is {email}")
    return TaskModuleResponse(task=TaskModuleMessageResponse(value="Multi-step form completed!"))


@app.event("error")
async def handle_error(event: ErrorEvent) -> None:
    print(f"Error occurred: {event.error}")
    if event.context:
        print(f"Context: {event.context}")


if __name__ == "__main__":
    port = int(os.getenv("PORT", 3978))
    asyncio.run(app.start(port))
