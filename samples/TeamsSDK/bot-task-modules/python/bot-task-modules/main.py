# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
from dotenv import load_dotenv

from microsoft_teams.api import (
    AdaptiveCardAttachment,
    CardTaskModuleTaskInfo,
    InvokeResponse,
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
from microsoft_teams.cards import AdaptiveCard, SubmitAction, SubmitActionData, TaskFetchAction, TextBlock, TextInput

load_dotenv()

if not os.getenv("BOT_ENDPOINT"):
    print("No remote endpoint detected. Using webpages will not work as expected")

app = App(client_id=os.getenv("CLIENT_ID"), client_secret=os.getenv("CLIENT_SECRET"))

app.page("customform", os.path.join(os.path.dirname(__file__), "pages", "CustomForm"), "/customform")


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    adaptive_card = AdaptiveCard(version="1.4").with_body(
        [TextBlock(text="Task Module Invocation from Adaptive Card", weight="Bolder", size="Large")]
    ).with_actions(
        [
            TaskFetchAction(value={"data": "AdaptiveCard"}).with_title("Adaptive Card"),
            TaskFetchAction(value={"data": "CustomForm"}).with_title("Custom Form"),
            TaskFetchAction(value={"data": "MultiStep"}).with_title("Multi-step Form"),
        ]
    )

    message = MessageActivityInput().add_card(adaptive_card)
    await ctx.send(message)


@app.on_dialog_open
async def handle_dialog_open(ctx: ActivityContext[TaskFetchInvokeActivity]):
    data = ctx.activity.value.data
    card_data = data.get("data") if isinstance(data, dict) else data

    if card_data == "CustomForm":
        return InvokeResponse(
            body=TaskModuleResponse(
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
        )

    elif card_data == "MultiStep":
        dialog_card = AdaptiveCard(version="1.4").with_body([
            TextBlock(text="Step 1 of 2 - Your Name", size="Large", weight="Bolder"),
            TextInput().with_id("name").with_label("Name").with_placeholder("Enter your name").with_is_required(True),
        ]).with_actions([
            SubmitAction().with_title("Next").with_data(SubmitActionData().with_data({"submissiontype": "multi_step_1"})),
        ])

        return InvokeResponse(
            body=TaskModuleResponse(
                task=TaskModuleContinueResponse(
                    value=CardTaskModuleTaskInfo(
                        title="Multi-step Form",
                        width=400,
                        height=300,
                        card=card_attachment(AdaptiveCardAttachment(content=dialog_card)),
                    )
                )
            )
        )

    dialog_card = AdaptiveCard(version="1.0").with_body([
        TextBlock(text="Enter Text Here", weight="Bolder"),
        TextInput().with_id("usertext").with_placeholder("add some text and submit").with_is_multiline(True),
    ]).with_actions([
        SubmitAction().with_title("Submit"),
    ])

    return InvokeResponse(
        body=TaskModuleResponse(
            task=TaskModuleContinueResponse(
                value=CardTaskModuleTaskInfo(
                    title="Adaptive Card: Inputs",
                    width=400,
                    height=200,
                    card=card_attachment(AdaptiveCardAttachment(content=dialog_card)),
                )
            )
        )
    )


@app.on_dialog_submit
async def handle_dialog_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]):
    data = ctx.activity.value.data
    submission_type = data.get("submissiontype") if isinstance(data, dict) else None

    if submission_type == "multi_step_1":
        name = data.get("name")
        next_card = AdaptiveCard(version="1.4").with_body([
            TextBlock(text="Step 2 of 2 - Your Email", size="Large", weight="Bolder"),
            TextInput().with_id("email").with_label("Email").with_placeholder("Enter your email").with_is_required(True),
        ]).with_actions([
            SubmitAction().with_title("Submit").with_data(SubmitActionData().with_data({"submissiontype": "multi_step_2", "name": name})),
        ])

        return InvokeResponse(
            body=TaskModuleResponse(
                task=TaskModuleContinueResponse(
                    value=CardTaskModuleTaskInfo(
                        title=f"Multi-step Form: Step 2",
                        width=400,
                        height=300,
                        card=card_attachment(AdaptiveCardAttachment(content=next_card)),
                    )
                )
            )
        )

    elif submission_type == "multi_step_2":
        name = data.get("name")
        email = data.get("email")
        await ctx.send(f"Hi {name}, thanks for submitting! Your email is {email}")
        return InvokeResponse(
            body=TaskModuleResponse(task=TaskModuleMessageResponse(value="Multi-step form completed!"))
        )

    elif submission_type == "custom_form":
        name = data.get("name") if data else None
        email = data.get("email") if data else None
        await ctx.send(f"Hi {name}, thanks for submitting! Your email is {email}")
        return InvokeResponse(
            body=TaskModuleResponse(task=TaskModuleMessageResponse(value="Form submitted successfully"))
        )

    usertext = data.get("usertext") if data else None
    await ctx.send(f"You submitted: {usertext}")
    return InvokeResponse(
        body=TaskModuleResponse(task=TaskModuleMessageResponse(value="Thanks for submitting!"))
    )


@app.event("error")
async def handle_error(event: ErrorEvent) -> None:
    print(f"Error occurred: {event.error}")
    if event.context:
        print(f"Context: {event.context}")


if __name__ == "__main__":
    port = int(os.getenv("PORT", 3978))
    asyncio.run(app.start(port))
